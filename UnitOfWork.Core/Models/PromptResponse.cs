using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class PromptResponse
    {
        public bool success { get; set; }
        public bool allowed { get; set; }
        public int remaining { get; set; }
        public string? message { get; set; }
    }
}
