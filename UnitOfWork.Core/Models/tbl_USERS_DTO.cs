using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class tbl_USERS_DTO
    {
        public string Action { get; set; }
        public int id { get; set; }
        public string username { get; set; }
        public int role_id { get; set; }
        public bool is_active { get; set; }
        public bool is_locked { get; set; }
        public bool is_deleted { get; set; }
      
    }
}
