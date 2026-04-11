using System;

namespace UnitOfWork.Core.Models
{
    public class tbl_USER_PROMPTS_DTO
    {
        public string Action { get; set; } = string.Empty;
        public string user_id { get; set; } = string.Empty;
        public string? username { get; set; }
        public string? rank_decode { get; set; }
        public string? base_decode { get; set; }
        public string model_name { get; set; } = string.Empty;
        public string prompt_text { get; set; } = string.Empty;
        public bool? has_attachments { get; set; }
        public string? attachment_names { get; set; }
        public string? session_id { get; set; }
        public DateTime? timestamp { get; set; }
    }
}