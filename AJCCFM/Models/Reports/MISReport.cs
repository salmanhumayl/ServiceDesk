using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.Reports
{
    public class MISReport
    {
        public class ShowClaimDetail
        {
            public int CompanyCode { get; set; }
            public int BUCode { get; set; }
            public string PaymentStatus { get; set; }

            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

        }
    }
}