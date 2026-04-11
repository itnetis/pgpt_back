using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class UserBasedRequest_DTO
    {
        public string Action { get; set; }  // INSERT | SELECT | UPDATE | REVIEW | DELETE
        public int? id { get; set; }
        public int? model_id { get; set; }
        public int? user_id { get; set; }
        public string? detail_justification { get; set; }
        public string? priority { get; set; }
        public string? tel_ext { get; set; }
        public string? additional_notes { get; set; }
        public string? admin_notes { get; set; }
        public string? status { get; set; }  // Pending | Approved | Rejected | Cancelled
        public int? total_promptperday { get; set; }
        public int? reviewed_by { get; set; }

        // New parameters from SQL stored procedure
        public string? RankCode { get; set; }
        public string? RankDecode { get; set; }
        public string? Username { get; set; }
        public string? PType { get; set; }
        public string? BaseCode { get; set; }
        public string? BaseDecode { get; set; }
        public string? UnitCode { get; set; }
        public string? UnitDecode { get; set; }
    }
}
