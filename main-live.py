import pyodbc
import requests
import re
from fastapi import FastAPI
from html import unescape
import uvicorn
from fastapi.responses import HTMLResponse

# =====================================================
# FASTAPI CONFIG
# =====================================================
app = FastAPI(title="Personnel Pen Picture API")

APP_PORT = 8001

# =====================================================
# LLM CONFIG
# =====================================================
LM_STUDIO_URL = "http://localhost:8080/v1/chat/completions"
MODEL_NAME = "humanizerai"
sample_paragraph = """
Uma Abx (PAKNO 11234) is a dedicated officer currently serving as a Flying Captain in the Pakistan Air Force. Commissioned on 1st June 2003, he has built a strong career in the Engineering (Operations Track) 
branch and is presently appointed as Director Engineering Group at BHQ Islamabad. Throughout his service, he has maintained an active status and contributed significantly to operational excellence. 
His professional record reflects strong performance, with OER averages of approximately 8.27 over 5 years, 8.18 over 10 years, and 7.85 overall, highlighting consistent excellence. He has also been 
recognized with several awards, including the prestigious Imtiazi Sanad. Over the years, he has served in multiple locations 
such as Karachi, Lahore, and Rawalpindi, gaining valuable experience. With more than two decades of service, he continues to demonstrate leadership, technical expertise, and commitment to duty.
"""
# =====================================================
# SQL SERVER CONNECTION
# =====================================================
import pyodbc


connection = pyodbc.connect(

#"Data Source=172.32.3.8\\MSSQL2K19;Initial Catalog=OIS;User ID=nastpstg;Password=nastp@7121;Persist Security Info=True;TrustServerCertificate=Yes;MultipleActiveResultSets=True",

    "DRIVER={SQL Server};"
    "SERVER=172.32.3.8\\MSSQL2K19;"
    "DATABASE=OIS;"
    "UID=nastpstg;"
    "PWD=nastp@7121;"
    "TrustServerCertificate=yes;"
)
# =====================================================
# LLM CONFIG
# =====================================================
LM_STUDIO_URL = "http://localhost:8080/v1/chat/completions"
MODEL_NAME = "ministral-3-14b-instruct-2512"
APP_PORT = 8001


ABBREVIATIONS = {
    "L O APP": "Letter of Appreciation",
    "CAS Commendation": "Chief of Air Staff Commendation",
    "PEB": "Performance Excellence Batch",
    "AHQ": "Air Headquarters",
    "BHQ": "Base Headquarters",
    "PAF": "Pakistan Air Force"
}
def get_officer(pakno: int):

    cursor = connection.cursor()

    query = """
    SELECT 
        PAKNO,
        FULL_NAME,
        RANK_ABBREVIATED,
        Current_Appoinment,
        COMMAND_DECODE_SHORT,
        BASE_DECODE_SHORT,
        SUB_BRANCH_DECODE,
        UNIT_DECODE_SHORT,
        DATE_OF_COMMISSION,
        TOC_CODE,
        CURR_STATUS_DECODE,
        BASIC_INFO,
        OER_INFO,
        FLYING_HOURS,
        AWARDS_DISCIPLINE,
        COURSES,
        SERVICE_DETAILS,
        AREA_STAY
    FROM dbo.ALL_OIS_MASTER_TABLE
    WHERE PAKNO = ?
    """

    cursor.execute(query, pakno)
    row = cursor.fetchone()

    if not row:
        return None

    columns = [column[0] for column in cursor.description]

    return dict(zip(columns, row))
# =====================================================
# CLEAN HTML
# =====================================================
# def clean_html(text):

#     if not text:
#         return ""

#     text = unescape(str(text))

#     text = re.sub(r'<br\s*/?>', '\n', text)
#     text = re.sub(r'<.*?>', '', text)

#     return text.strip()
def clean_html(text):

    if text is None:
        return ""

    clean = re.sub(r'<.*?>', ' ', str(text))
    clean = re.sub(r'\s+', ' ', clean)

    return clean.strip()
# Added New Function to Normalize Abbreviations
def normalize_abbreviations(text):
    if not text:
        return ""

    for abbr, full in ABBREVIATIONS.items():
        # Replace abbreviation with full form
        text = re.sub(rf"\b{abbr}\b", full, text, flags=re.IGNORECASE)

    return text
# =====================================================
# CLEAN DATASET
# =====================================================
def clean_data(data):

    cleaned = {}

    for key, value in data.items():
        clean_val = clean_html(value)
        clean_val = normalize_abbreviations(clean_val)  # 🔥 ADD THIS
        cleaned[key] = clean_val

    return cleaned

# =====================================================
# LLM EXPLAIN FIELD
# =====================================================
def explain_field(field_name, text):

    if not text:
        return ""

    prompt = f"""
Explain the following {field_name} in a professional military biography style.

Data:
{text}

Write a short clear paragraph.
"""

    payload = {
        "model": MODEL_NAME,
        "messages": [
            {
                "role": "system",
                "content": "You convert military service records into professional biography text."
            },
            {
                "role": "user",
                "content": prompt
            }
        ],
        "temperature": 0.1
    }

    response = requests.post(LM_STUDIO_URL, json=payload)

    result = response.json()

    return result["choices"][0]["message"]["content"]

# =====================================================
# BUILD PEN PICTURE (PYTHON STRUCTURE)
# =====================================================
def build_pen_picture(data, oer_text, flying_text, pakno):

    def val(key):
        return str(data.get(key, "")).strip()

    html = f"""
<h2>Pen Picture of PakNo {pakno}</h2>

<h3>{val('RANK_ABBREVIATED')} {val('FULL_NAME')}</h3>

<p><b>Introduction:</b><br>
{val('RANK_ABBREVIATED')} {val('FULL_NAME')} is currently serving as {val('Current_Appoinment')} 
at {val('BASE_DECODE_SHORT')} under {val('COMMAND_DECODE_SHORT')} Command.
He belongs to the {val('SUB_BRANCH_DECODE')} branch and is associated with {val('UNIT_DECODE_SHORT')}.
Commissioned on {val('DATE_OF_COMMISSION')}, he is presently serving with the status {val('CURR_STATUS_DECODE')}.
</p>

<p><b>Basic Information:</b><br>
{val('BASIC_INFO')}
</p>

<p><b>Qualification and Courses:</b><br>
{val('COURSES')}
</p>

<p><b>OER Evaluation:</b><br>
{oer_text}
</p>

<p><b>Flying Hours:</b><br>
{flying_text}
</p>

<p><b>Awards and Discipline:</b><br>
{val('AWARDS_DISCIPLINE')}
</p>

<p><b>Service Details:</b><br>
{val('SERVICE_DETAILS')}
</p>

<p><b>Area Stay / Operational Exposure:</b><br>
{val('AREA_STAY')}
</p>
"""

    return html
# =====================================================
# API ENDPOINT
# =====================================================
# @app.get("/api/generate_pen_picture/{pakno}", response_class=HTMLResponse)
# def generate_pen_picture(pakno: int):

#     officer = get_officer(pakno)

#     if not officer:
#         return {"error": "Officer not found"}

#     officer = clean_data(officer)

#     # LLM explanations
#     oer_text = explain_field("Officer Evaluation Report performance", officer.get("OER_INFO", ""))

#     flying_text = explain_field("aviation flying hours experience", officer.get("FLYING_HOURS", ""))

#     # Build final pen picture
#     pen_picture = build_pen_picture(officer, oer_text, flying_text, pakno)

#     return {
#         "pakno": pakno,
#         "name": officer["FULL_NAME"],
#         "pen_picture": pen_picture
#     }
# return {"pakno": pakno, "name": officer["FULL_NAME"], "pen_picture_html": pen_picture}

#New Function

def generate_summary_paragraph(data, pakno):

    abbr_text = "\n".join([f"{k} = {v}" for k, v in ABBREVIATIONS.items()])

    prompt = f"""
PAKISTAN AIR FORCE
OFFICIAL CORRESPONDENCE GUIDELINES (CONDENSED AI VERSION)
Based on AFM 10-1 (Vol-I), 27 April 2018

------------------------------------------------------------
CORE OBJECTIVE OF SERVICE WRITING
------------------------------------------------------------
The aim of service writing is to:
- Initiate required action quickly and efficiently.
- Convey orders, intentions and information clearly.
- Record decisions and discussions.
- Persuade through logical and concise arguments.

Service writing must always maintain:
1. Accuracy
2. Clarity
3. Conciseness
4. Logical and convincing presentation
5. Standardized format

There is no scope for ambiguity, repetition, casual tone, or leisurely style.

------------------------------------------------------------
MANDATORY STYLE REQUIREMENTS
------------------------------------------------------------
• Formal, disciplined and objective tone.
• Use clear, simple and concise sentences.
• Avoid slang, emojis, contractions and conversational phrases.
• Avoid jargon, clichés and unnecessary verbosity.
• Prefer common words over complicated expressions.
• Avoid omnibus letters (one subject per communication).
• Maintain professional restraint and dignity.

Preferred:
- “start” instead of “commence”
- “before” instead of “prior to”
- “after” instead of “subsequent to”
- “kindly” instead of “it is requested”

------------------------------------------------------------
DATE AND TIME
------------------------------------------------------------
• Date format: 16 April, 2018 or 16 Apr 18
• 24-hour clock system (e.g., 2230 hours)

------------------------------------------------------------
USE OF CAPITALS
------------------------------------------------------------
• Titles in BLOCK CAPITALS.
• Important headings in Initial Capitals.
• Avoid overuse of capitals.
• Names and ranks must appear in correct order.

------------------------------------------------------------
ENFORCEMENT OF TONE FOR AI OUTPUT
------------------------------------------------------------
When generating responses:

• Maintain formal PAF service tone.
• Use restrained and disciplined language.
• Avoid emotional, casual or exaggerated expressions.
• Avoid unnecessary adjectives.
• Use structured formatting where applicable.
• Do not invent ranks, references, dates or authorities.
• If missing information, use placeholders:
  [Ref], [Date], [Rank/Name], [Unit]

------------------------------------------------------------
OPTIONAL PAF PHRASES (USE NATURALLY, NOT EXCESSIVELY)
------------------------------------------------------------
• it is submitted that
• it is intimated that
• the undersigned is directed to
• kindly
• keeping in view
• in view of the foregoing
• for kind information
• for necessary action
• requisite action may please be initiated
• approval may please be accorded
• please be guided accordingly

------------------------------------------------------------
PROHIBITED ELEMENTS
------------------------------------------------------------
• Emojis
• Casual greetings (Hi, Hello)
• AI disclaimers
• Explanatory commentary unless specifically asked
• Bullet formatting unless suitable to document type
• Conversational tone

------------------------------------------------------------
FINAL RULE FOR AI MODEL
------------------------------------------------------------
All generated responses must reflect Pakistan Air Force
service writing discipline in tone, structure and clarity,
while remaining concise, logical and professionally restrained.

END OF GUIDELINES

Approved Abbreviations:
{abbr_text}

Officer Data:
Name: {data.get("FULL_NAME")}
PAKNO: {pakno}
Rank: {data.get("RANK_ABBREVIATED")}
Appointment: {data.get("Current_Appoinment")}
Command: {data.get("COMMAND_DECODE_SHORT")}
Base: {data.get("BASE_DECODE_SHORT")}
Branch: {data.get("SUB_BRANCH_DECODE")}
Unit: {data.get("UNIT_DECODE_SHORT")}
Commission Date: {data.get("DATE_OF_COMMISSION")}
Status: {data.get("CURR_STATUS_DECODE")}

Performance:
{data.get("OER_INFO")}

Awards:
{data.get("AWARDS_DISCIPLINE")}

Courses:
{data.get("COURSES")}

Service Details:
{data.get("SERVICE_DETAILS")}

Area Stay:
{data.get("AREA_STAY")}

Sample Paragraph Style:
{sample_paragraph}

Now generate the pen picture in the same style.
"""

    payload = {
        "model": MODEL_NAME,
        "messages": [
            {
                "role": "system",
                "content": "You write concise professional military biographies."
            },
            {
                "role": "user",
                "content": prompt
            }
        ],
        "temperature": 0.1
    }

    response = requests.post(LM_STUDIO_URL, json=payload)

    result = response.json()

    return result["choices"][0]["message"]["content"]


@app.get("/api/generate_pen_picture/{pakno}", response_class=HTMLResponse)
def generate_pen_picture(pakno: int):
    officer = get_officer(pakno)

    if not officer:
        return {"error": "Officer not found"}  

    officer = clean_data(officer)

    oer_text = explain_field("Officer Evaluation Report performance", officer.get("OER_INFO", ""))
    flying_text = explain_field("aviation flying hours experience", officer.get("FLYING_HOURS", ""))

    pen_picture = build_pen_picture(officer, oer_text, flying_text, pakno)
    return pen_picture  # Return the HTML string directly

@app.get("/api/generate_pen_picture_summary/{pakno}")
def generate_pen_picture_summary(pakno: int):

    officer = get_officer(pakno)

    if not officer:
        return {"error": "Officer not found"}

    officer = clean_data(officer)

    summary = generate_summary_paragraph(officer, pakno)

    return {
        "pakno": pakno,
        "name": officer["FULL_NAME"],
        "pen_picture": summary
    }
# =====================================================
# RUN SERVER
# =====================================================
if __name__ == "__main__":

    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=APP_PORT,
        access_log=False
    )
