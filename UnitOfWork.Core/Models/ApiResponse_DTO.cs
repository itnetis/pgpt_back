using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class ApiResponse
    {
        public bool success { get; set; }
        public string response { get; set; }
        public List<DataItem> data { get; set; } // Note: 'data' is a list (even if it has only one item)
    }
    // Root object returned by the API

    // One item inside the “data” array
    public class DataItem
    {
        public int BASE_CODE { get; set; }
        public string UNIT_CODE { get; set; }
        public int BASE_CODE_PRISM { get; set; }
        public string BASE { get; set; }
        public string UNIT { get; set; }
        public string RANK_DECODE { get; set; }
        public string BRANCH_DECODE { get; set; }
        public int CURRENT_RANK_CODE { get; set; }
        public string FULL_NAME { get; set; }
        public string MAIN_BRANCH_CODE { get; set; }
    }


}
