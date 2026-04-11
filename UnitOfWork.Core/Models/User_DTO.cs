using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class User_DTO
    {
        public string? Pakno { get; set; }
        public string? PasswordHash { get; set; }
        public int? PType { get; set; }
    }
}
