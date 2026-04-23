using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class PromptRequest
    {
        public string user_id { get; set; }
        public string model_name { get; set; }
        public string prompt_text { get; set; }

        public string? session_id { get; set; }
        public bool has_attachments { get; set; } = false;
        public string? attachment_names { get; set; }
    }
}
