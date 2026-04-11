using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class Response_DTO
    {
        public int? newId { get; set; }
        public string message { get; set; }
        public bool status { get; set; }
    }
}
