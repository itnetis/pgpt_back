using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UnitOfWork.Core.Models
{

    public class PromptHistory
    {
        [Key]
        public int Id { get; set; }
        public String UserName { get; set; }

        public DateTime TimeStamp { get; set; }
        public int Count { get; set; }

    }
}
