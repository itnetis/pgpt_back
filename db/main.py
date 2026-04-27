import os
import json
import socket
import logging
import time
import uuid
from datetime import datetime, timedelta
from functools import lru_cache
from typing import List, Dict, Any, Optional
import httpx
import asyncio
import csv
from datetime import datetime
from pathlib import Path
from fastapi import FastAPI, Request, HTTPException
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import requests
from fastapi.responses import StreamingResponse

MODEL_URL = "http://localhost:1234/v1/chat/completions"
MODEL_NAME = "llama-3-13b-instruct-v0.1.q8_0"
# ---------------------------
# Environment / Uvicorn
# ---------------------------
APP_PORT = 9000
os.environ['KMP_DUPLICATE_LIB_OK'] = 'TRUE'

# ---------------------------
# Logging setup
# ---------------------------
LOG_DIR = "logs"
os.makedirs(LOG_DIR, exist_ok=True)

logging.basicConfig(
    filename=os.path.join(LOG_DIR, "server.log"),
    level=logging.INFO,
    format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger("chatbot")

# Additional logs files
FEED_LOG = os.path.join(LOG_DIR, "feed.log")
PROCESS_TIMES_FILE = os.path.join(LOG_DIR, "process_times.json")

# Ensure process_times file exists
if not os.path.exists(PROCESS_TIMES_FILE):
    with open(PROCESS_TIMES_FILE, "w", encoding="utf-8") as f:
        json.dump([], f)

# ---------------------------
# FastAPI app
# ---------------------------
app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ---------------------------
# Constants
# ---------------------------
MODEL_DIR = "vectorstore_merged"
IRRELEVANT_QUESTIONS = ["hi", "hello", "hey", "how are you", "date", "time", "what's up"]

PAF_CONTENT_SYSTEM_PROMPT = """
You are an AI assistant that writes strictly in Pakistan Air Force (PAF) official correspondence style.

OBJECTIVE:
Generate formal service-style content (letters, minutes, applications, briefs, official replies, summaries, and notes) in PAF correspondence tone.

STYLE:
- Formal, disciplined, and concise.
- Avoid slang, emojis, casual chat, and contractions.
- Prefer official phrasing and passive voice where suitable.
- Use PAF-leaning diction naturally (not excessive): "it is submitted that", "it is intimated that",
  "the undersigned is directed to", "kindly", "apprised", "requested", "keeping in view",
  "in view of the foregoing", "for kind information", "for necessary action", "requisite action",
  "may please be accorded", "please be guided accordingly".

FORMAT (apply when user asks for letter/minute/application/official note):
- SUBJECT in UPPERCASE.
- "Sir," / "Ma’am," salutation.
- Body in short paragraphs or numbered points.
- Close with: "Submitted for kind information and necessary action, please."
- Do not invent ref numbers/dates/ranks/units; use placeholders like [Ref], [Date], [Rank/Name], [Unit], [Appointment].

GUARDRAILS:
- Do not fabricate official policies or orders.
- If user input lacks details, use placeholders instead of guessing.
- Output ONLY the final drafted content. No explanations.
""".strip()


PAF_GUIDELINES_PATH = Path(__file__).parent / "paf_guidelines_condensed.txt"

def load_paf_guidelines() -> str:
    try:
        if PAF_GUIDELINES_PATH.exists():
            txt = PAF_GUIDELINES_PATH.read_text(encoding="utf-8").strip()
            return txt[:12000]
    except Exception as e:
        print(f"[load_paf_guidelines] Warning: {e}")
    return ""

# ---------------------------
# Pydantic models
# ---------------------------
# class ChatRequest(BaseModel):
#     question: str
#     session_id: Optional[str] = None
class ChatRequest(BaseModel):
    text: str
    session_id: Optional[str] = None

class ChatResponse(BaseModel):
    response: str
    session_id: str
    sources: List[Dict[str, Any]]
    processing_time: float

class SessionHistoryResponse(BaseModel):
    session_id: str
    history: List[Dict[str, Any]]

# ---------------------------
# Utilities
# ---------------------------
def is_valid_question(q: str) -> bool:
    import re
    q = q.strip().lower()
    if len(q) < 4: return False
    if re.fullmatch(r"[a-zA-Z]{1,4}", q): return False
    if not re.search(r"[a-zA-Z]{2,}", q): return False
    return True

def get_page_number(metadata: Dict[str, Any]) -> int:
    return metadata.get("page_number", metadata.get("page", 0) + 1)

def docs_to_jsonable(docs: List[Any]) -> List[Dict[str, Any]]:
    return [
        {
            "source": os.path.basename(doc.metadata['source']),
            "page_number": get_page_number(doc.metadata),
            "content": doc.page_content[:200] + "..." if len(doc.page_content) > 200 else doc.page_content,
        } for doc in docs
    ]

# ------------------------------------------------
# Async streaming RAG helper 
# ------------------------------------------------
async def stream_rag_model(prompt: str):
    payload = {
        "model": MODEL_NAME,
        "messages": [
            {"role": "system", "content": "You are an expert document assistant."},
            {"role": "user", "content": prompt}
        ],
        "temperature": 0.3,
        "stream": True
    }

    async with httpx.AsyncClient(timeout=None) as client:
        async with client.stream("POST", MODEL_URL, json=payload) as response:
            async for line in response.aiter_lines():

                if not line or not line.strip():
                    continue

                if line.startswith("data:"):
                    line = line[5:].strip()

                if line == "[DONE]":
                    break

                try:
                    data = json.loads(line)
                    delta = data["choices"][0]["delta"].get("content")

                    if delta:
                        yield f"data: {delta}\n\n"

                except:
                    continue
# -------------------------------
# Request Schema Summarize and Rephrase and ContentWriting
# -------------------------------
class TextRequest(BaseModel):
    text: str

# ------------------------------------------------
# Async streaming helper
# ------------------------------------------------
async def stream_model(system_prompt: str, user_text: str):
    payload = {
        "model": MODEL_NAME,
        "messages": [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_text},
        ],
        "temperature": 0.7,
        "max_tokens": -1,
        "stream": True,
    }

    async with httpx.AsyncClient(timeout=None) as client:
        async with client.stream("POST", MODEL_URL, json=payload) as response:
            async for line in response.aiter_lines():
                if not line or not line.strip():
                    continue

                if line.startswith("data:"):
                    line = line[len("data:"):].strip()

                if line == "[DONE]":
                    break

                try:
                    data = json.loads(line)
                    delta = data.get("choices", [{}])[0].get("delta", {}).get("content")
                    if delta:
                        yield f"data: {delta}\n\n"
                        await asyncio.sleep(0)  # let event loop flush chunk
                except Exception:
                    continue

# -------------------------------
# Request Schema For Logs
# -------------------------------
# Add this class after TextRequest
class ChatLogRequest(BaseModel):
    pakno: str
    mode: str
    user_question: str
    ai_response: str
    timestamp: str = None

# Add this function to handle CSV logging
def save_chat_to_json(pakno: str, mode: str, user_question: str, ai_response: str):
    """
    Save chat log to JSON file (append mode)
    """
    # Create chat_logs directory next to this script
    logs_dir = Path(__file__).parent / "PAFGPT_chat_logs"
    logs_dir.mkdir(parents=True, exist_ok=True)

    # Sanitize pakno for filename
    safe_pakno = str(pakno or "").replace(" ", "_")
    safe_pakno = "".join(c for c in safe_pakno if c.isalnum() or c in ('-', '_'))
    if not safe_pakno:
        safe_pakno = "unknown"

    filename = f"Chat_{safe_pakno}.json"
    filepath = logs_dir / filename

    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    log_entry = {
        "timestamp": timestamp,
        "mode": mode,
        "user_question": user_question,
        "ai_response": ai_response
    }

    try:
        # Read existing logs if file exists
        logs = []
        if filepath.exists():
            try:
                with open(filepath, "r", encoding="utf-8") as f:
                    content = f.read().strip()
                    if content:  # Only parse if not empty
                        logs = json.loads(content)
                    else:
                        logs = []
            except (json.JSONDecodeError, ValueError) as e:
                logs = []

        # Append new entry
        logs.append(log_entry)

        # Write back to file
        with open(filepath, "w", encoding="utf-8") as f:
            json.dump(logs, f, ensure_ascii=False, indent=4)
            f.flush()
            try:
                os.fsync(f.fileno())
            except Exception:
                pass
        return {"status": "success", "file": filename}
    
    except Exception as e:
        return {"status": "error", "message": str(e)}

# -------------------------------
# Helper to call model for PAF Publication Chatbot
# -------------------------------

def call_custom_chat_model(prompt: str, timeout: int = 100) -> str:
    url = "http://localhost:8080/v1/chat/completions"
    payload = {
        "model": "llama-3-8b-instruct.q4_k_s",
        "messages": [
            {"role": "system", "content": "You are an expert document assistant."},
            {"role": "user", "content": prompt}
        ],
        "max_tokens": 100
    }
    try:
        r = requests.post(url, json=payload, timeout=timeout)
        r.raise_for_status()
        return r.json()["choices"][0]["message"]["content"]
    except Exception as e:
        return f"⚠️ Error: {e}"

# ---------------------------
# Chat service with RAG
# ---------------------------
class ChatService:
    def __init__(self):
        self.vectorstore = self.load_vectorstore()
        self.rag_chain = self.create_rag_chain()
        self.sessions: Dict[str, List[Dict]] = {}

    @lru_cache
    def load_vectorstore(self):
        if not os.path.exists(MODEL_DIR):
            logger.warning("⚠️ No vectorstore found. RAG will be disabled.")
            return None
        from langchain_community.vectorstores import FAISS
        from langchain_ollama import OllamaEmbeddings
        embeddings = OllamaEmbeddings(model="nomic-embed-text")
        return FAISS.load_local(MODEL_DIR, embeddings, allow_dangerous_deserialization=True)

    def create_rag_chain(self):
        if not self.vectorstore:
            return lambda q: ("RAG not available", [])
        retriever = self.vectorstore.as_retriever(
            search_type="mmr",
            search_kwargs={"k": 8, "fetch_k": 30, "lambda_mult": 0.8}
        )
        prompt_template = """You are an expert document assistant.
Answer ONLY using the context below.
Always cite sources exactly: [Source: filename (page X)]
If unsure, say: "I couldn't find relevant information."

Context:
{context}

Question:
{question}
"""
        def rag_chain(question: str):
            docs = retriever.invoke(question)
            context = "\n\n".join([
                f"### {os.path.basename(doc.metadata['source'])} (Page {get_page_number(doc.metadata)})\n{doc.page_content}"
                for doc in docs
            ])
            prompt = prompt_template.format(context=context, question=question)
            answer = call_custom_chat_model(prompt)
            return answer, docs
        return rag_chain

    def create_session(self, session_id: Optional[str] = None) -> str:
        if session_id is None:
            session_id = f"session_{uuid.uuid4().hex}"
        self.sessions.setdefault(session_id, [])
        return session_id

    def update_session(self, session_id: str):
        self.sessions.setdefault(session_id, [])

    def cleanup_sessions(self, timeout_minutes: int = 2):
        now = datetime.now()
        expired = []
        for sid, entries in list(self.sessions.items()):
            # entries stored with timestamp like "dd-mm-YYYY HH:MM:SS" in entries[0]
            if entries:
                try:
                    last_time = datetime.strptime(entries[0]["timestamp"], "%d-%m-%Y %H:%M:%S")
                except Exception:
                    # fallback to now to avoid accidental deletion if format unexpected
                    last_time = now
            else:
                last_time = now
            if now - last_time > timedelta(minutes=timeout_minutes):
                expired.append(sid)
                del self.sessions[sid]
                print(f"INFO: 🗑️ Session {sid} expired and removed")
                logger.info(f"Session {sid} expired and removed")
        return expired

    def active_sessions_count(self, timeout_minutes: int = 2) -> int:
        self.cleanup_sessions(timeout_minutes)
        return len(self.sessions)

    def process_question(self, session_id: str, question: str, client_ip: str, hostname: str) -> Dict[str, Any]:
        start_time = time.time()
        session = self.sessions.get(session_id, [])

        lower_q = question.lower().strip()
        if lower_q in IRRELEVANT_QUESTIONS:
            response = "🤖 I'm focused on document-related questions. Please ask something relevant."
            docs_list = []
        else:
            try:
                response, docs = self.rag_chain(question)
                docs_list = docs_to_jsonable(docs)
                # if docs and "Source:" not in response:
                #     sources = "\n".join(
                #         f"[Source: {doc['source']} (page {doc['page_number']})]" for doc in docs_list
                #     )
                #     response += f"\n\n{sources}"
            except Exception as e:
                response = f"⚠️ Error: {str(e)}"
                docs_list = []

        elapsed = time.time() - start_time
        response += f"\n\n_(Processed in {elapsed:.2f}s)_"

        # Timestamp in local time (dd-mm-YYYY HH:MM:SS)
        timestamp = datetime.now().strftime("%d-%m-%Y %H:%M:%S")

        # Create entry and prepend so newest is on top
        entry = {
            "query": question,
            "response": response,
            "timestamp": timestamp,
            "sources": docs_list,
            "client_ip": client_ip,
            "hostname": hostname,
            "processing_time": round(elapsed, 2)
        }

        session.insert(0, entry)
        # Keep only last 3 entries in memory
        if len(session) > 300:
            session = session[:300]

        self.sessions[session_id] = session

        # Save session logs (IP + hostname) - file name: chat_session_<ip>_<host>.json
        safe_ip = client_ip.replace(":", "_")
        safe_host = hostname.replace(" ", "_")
        log_file = os.path.join(LOG_DIR, f"chat_session_{safe_ip}_{safe_host}.json")

        try:
            previous_entries = json.load(open(log_file, encoding="utf-8")) if os.path.exists(log_file) else []
            if not isinstance(previous_entries, list):
                previous_entries = []
        except Exception:
            previous_entries = []

        # Prepend new entry and keep only 300 in persistent log
        all_entries = [entry] + previous_entries[:299]

        try:
            with open(log_file, "w", encoding="utf-8") as f:
                json.dump(all_entries, f, indent=2, ensure_ascii=False)
        except Exception as e:
            logger.warning(f"Could not write session log {log_file}: {e}")

        # ---- Append to feed.log with processing time and brief metadata ----
        try:
            feed_line = (
                f"[{datetime.now().strftime('%d-%m-%Y %H:%M:%S')}] "
                f"session:{session_id} ip:{client_ip} host:{hostname} "
                f"query:{question[:200]} | time:{round(elapsed,2)}s\n"
            )
            with open(FEED_LOG, "a", encoding="utf-8") as f:
                f.write(feed_line)
        except Exception as e:
            logger.warning(f"Could not append to feed.log: {e}")

        # ---- Persist processing times (keep last 100) ----
        try:
            pt_list = []
            if os.path.exists(PROCESS_TIMES_FILE):
                try:
                    with open(PROCESS_TIMES_FILE, "r", encoding="utf-8") as f:
                        pt_list = json.load(f)
                        if not isinstance(pt_list, list):
                            pt_list = []
                except Exception:
                    pt_list = []
            pt_list.append(round(elapsed, 2))
            if len(pt_list) > 100:
                pt_list = pt_list[-100:]
            with open(PROCESS_TIMES_FILE, "w", encoding="utf-8") as f:
                json.dump(pt_list, f)
        except Exception as e:
            logger.warning(f"Could not update process_times.json: {e}")

        return {"response": response, "sources": docs_list, "processing_time": elapsed}


chat_service = ChatService()

# ---------------------------
# API Endpoints
# ---------------------------
@app.get("/health")
async def health():
    # Keep endpoint, but hide logs in terminal
    chat_service.cleanup_sessions()
    return {"status": "online", "sessions": chat_service.active_sessions_count()}

@app.post("/chat")
async def chat_endpoint(chat: ChatRequest, request: Request):

    forwarded_for = request.headers.get("x-forwarded-for")
    client_ip = forwarded_for.split(",")[0].strip() if forwarded_for else request.client.host

    try:
        hostname = socket.gethostbyaddr(client_ip)[0]
    except Exception:
        hostname = "Unknown"

    session_id = chat.session_id if chat.session_id else chat_service.create_session()
    chat_service.update_session(session_id)

    async def stream():

        start_time = time.time()

        try:
            docs = chat_service.vectorstore.similarity_search(chat.text, k=6)

            context = "\n\n".join([
                f"{os.path.basename(doc.metadata['source'])} (Page {get_page_number(doc.metadata)})\n{doc.page_content}"
                for doc in docs
            ])

            prompt = f"""
Answer the question using ONLY the context.

Context:
{context}

Question:
{chat.text}
"""

            docs_list = docs_to_jsonable(docs)

            # send sources first (frontend can ignore)
            yield f"data: {json.dumps({'sources': docs_list})}\n\n"

            async for chunk in stream_rag_model(prompt):
                yield chunk

        except Exception as e:
            yield f"data: ⚠️ Error: {str(e)}\n\n"

        elapsed = time.time() - start_time
        yield f"data: \n\n_(Processed in {elapsed:.2f}s)_\n\n"

    return StreamingResponse(stream(), media_type="text/event-stream")

@app.post("/session/new")
async def create_new_session():
    session_id = chat_service.create_session()
    return JSONResponse(content={"session_id": session_id, "message": "New session created"})


@app.get("/history/{session_id}", response_model=SessionHistoryResponse)
async def get_session_history(session_id: str):
    if session_id not in chat_service.sessions:
        raise HTTPException(status_code=404, detail="Session not found")
    return SessionHistoryResponse(session_id=session_id, history=chat_service.sessions[session_id])


# ------------------------------------------------
# Summarize Endpoint
# ------------------------------------------------
@app.post("/api/summarize")
async def summarize(req: TextRequest):
    system_prompt = """
You are an expert text summarizer.
Your task is to summarize the following text in a clear, accurate, and concise way.
Rules:
- Capture only the essential ideas and remove unnecessary details.
- Maintain the same meaning and logical flow.
- Use neutral, formal, and objective tone.
- If the text is technical or academic, preserve key terms and definitions.
- If the input text is a list, return a short bullet summary.
- Output format: A short paragraph or a bullet summary depending on input structure.
"""
    return StreamingResponse(stream_model(system_prompt, req.text), media_type="text/event-stream")


# ------------------------------------------------
# Rephrase Endpoint
# ------------------------------------------------
@app.post("/api/rephrase")
async def rephrase(req: TextRequest):
    system_prompt = """
You are a professional rewriter and language improver.
Your task is to paraphrase the following text for better clarity and readability while keeping the original meaning intact.
Rules:
- Do not add or remove information.
- Improve grammar, sentence structure, and fluency.
- Maintain the same tone (formal/informal) as the original.
- Avoid repetition and use natural phrasing.
- Ensure the rewritten version sounds like it was written by a human, not an AI.
- Output only the improved text, no explanations.
"""
    return StreamingResponse(stream_model(system_prompt, req.text), media_type="text/event-stream")


# ------------------------------------------------
# ContentWriting Endpoint
# ------------------------------------------------
@app.post("/api/ContentWriting")
async def ContentWriting(req: TextRequest):

    paf_reference = load_paf_guidelines()
    system_prompt = PAF_CONTENT_SYSTEM_PROMPT

    if paf_reference:
        system_prompt += "\n\nREFERENCE (PAF GUIDELINES - CONSULT):\n" + paf_reference

    async def stream():
        payload = {
            "model": MODEL_NAME,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": req.text},
            ],
            "temperature": 0.4,   # lower = more formal/controlled
            "top_p": 0.9,
            "max_tokens": -1,
            "stream": True,
        }

        async with httpx.AsyncClient(timeout=None) as client:
            async with client.stream("POST", MODEL_URL, json=payload) as response:
                async for line in response.aiter_lines():
                    if not line or not line.strip():
                        continue

                    if line.startswith("data:"):
                        line = line[len("data:"):].strip()

                    if line == "[DONE]":
                        break

                    try:
                        data = json.loads(line)
                        delta = data.get("choices", [{}])[0].get("delta", {}).get("content")
                        if delta:
                            yield f"data: {delta}\n\n"
                            await asyncio.sleep(0)
                    except Exception:
                        continue

    return StreamingResponse(stream(), media_type="text/event-stream")
    
# ------------------------------------------------
# Generalized use of ChatGpt
# ------------------------------------------------
@app.post("/chatgpt")
async def ContentWriting(req: TextRequest):
    system_prompt = """
You are an AI assistant specialized in helping users with a wide range of tasks,
including answering questions, explaining concepts, writing content, summarizing,
analyzing information, and providing practical guidance.

Rules:
- Be accurate, clear, and concise.
- Maintain logical flow and correctness.
- Match the response style to the task (explanatory, instructional, analytical, etc.).
- Avoid unnecessary verbosity unless requested.
- Preserve important terminology when relevant.
- Follow user instructions strictly.

Goal:
Help the user achieve their objective efficiently and correctly.

"""
    return StreamingResponse(stream_model(system_prompt, req.text), media_type="text/event-stream")



# -------------------------------------------
# SaveLogs
# -------------------------------------------



# Add this endpoint after your existing endpoints
@app.post("/api/save-chat-log")
async def save_chat_log(log_data: ChatLogRequest):
    """
    Endpoint to save chat conversation to CSV file
    """
    result = save_chat_to_json(
        pakno=log_data.pakno,
        mode=log_data.mode,
        user_question=log_data.user_question,
        ai_response=log_data.ai_response
    )
    return result


# -------------------------------
# Root route
# -------------------------------
@app.get("/api/check_api")
async def root():
    return {"message": "Summarizer & Rephraser API is running ✅"}

# ---------------------------
# Run server
# ---------------------------
if __name__ == "__main__":
    logger.info("🚀 Starting Chat Service...")
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=APP_PORT, access_log=False)
