using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class PromptLog
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }

        public string ModelRequest { get; set; }

        public string UserPrompt { get; set; }
        public string ResponseTime { get; set; }
        public DateTime TimeStamp { get; set; }




    }
}
