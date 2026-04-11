using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class CreateUser
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public String? PhoneNumber { get; set; }
        public Role? role { get; set; }   
        public Prompt? prompt { get; set; }
        public Int32 TokenAllowed { get; set; }
        public Model? model { get; set; }
    }
}
