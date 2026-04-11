using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class LoginUser_DTO
    {
        public string? pakno { get; set; }
        public int? peR_TYPE { get; set; }
        public string? currenT_RANK_CODE { get; set; }
        public string? currenT_RANK_DECODE { get; set; }
        public string? fulL_NAME { get; set; }
        public int? basE_CODE { get; set; }
        public string? basE_DECODE { get; set; }
        public int? uniT_CODE { get; set; }
        public string? uniT_DECODE { get; set; } 
        public int? sectioN_CODE { get; set; }
        public string? sectioN_DECODE { get; set; }
        public string? maiN_BRANCH_CODE { get; set; }
        public string? maiN_BRANCH_DECODE { get; set; }
    }
}
