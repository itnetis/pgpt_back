using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class Prompt
    {
        [Key]
        public int Id { get; set; }
        public int? PromptAllowed { get; set;}

    }
}
