using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class tbl_MODELS_DTO
    {
        public string? Action { get; set; }
        public int? id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public int? context_window_tokens { get; set; }
        public string features { get; set; }
        public bool? is_locked { get; set; }
        public string icon_class { get; set; }
        public string access_level { get; set; }  // Changed from int to string to match VARCHAR(20)
        public bool? is_enabled { get; set; }
        public int? created_by { get; set; }      // Changed from string to int to match INT
        public int? updated_by { get; set; }      // Added for update operations

        // For tracking changes in UI if needed
        //public DateTime? last_updated { get; set; }
    }

}
