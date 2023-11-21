using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models
{
    public class Folder
    {
        public int id { get; set; }
        public int srno { get; set; }
        public string ProcessOwner { get; set; }
        public string FolderName { get; set; }
        public string FolderDetail { get; set; }
        public string Approvedon { get; set; }
        public bool IsPendingApproval { get; set; }
        public int ReportID { get; set; }

        public bool remindar { get; set; }
        public string EmailAddress { get; set; }

        public string ProjectName { get; set; }
    }
}