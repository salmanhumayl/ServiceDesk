using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models
{
    public class LogHistoryModel
    {
        public string RefNo { get; set; }
        public string Status { get; set; }
        public DateTime ProcessOn { get; set; }
        public string ProcessBy { get; set; }
        public string Submittedto { get; set; }

        public string Remarks { get; set; }
        public DateTime Approvedon { get; set; }
        public string StepName { get; set; }
        public int Doc_Code { get; set; }
    }
}