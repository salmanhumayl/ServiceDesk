using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.GroupRequest
{
    public class SharefolderModel
    {

        public class ShareFolderProgress
        {
            public int ID { get; set; }
            public string RefNo { get; set; }
            public string Project { get; set; }
            public DateTime CreatedOn { get; set; }
            public string ProcessOwner { get; set; }

        }

        public class ShareFolderPending
        {
            public int ID { get; set; }
            public string RefNo { get; set; }
            public string EmpCode { get; set; }
            public string Name { get; set; }

            public string Position { get; set; }
            public string Project { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime ApprovedOn { get; set; }
            public string ProcessOwner { get; set; }
            public string Status { get; set; }
            public string Createdby { get; set; }
            public string AssystNo { get; set; }
            public string IsReview { get; set; }
            public string ProcessOwnerLoginID { get; set; }
        }

        public class ReportPDF
        {
            public string RefNo { get; set; }
            public string EmpCode { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string ProjectCode { get; set; }
            public string Project { get; set; }

            public string ProcessOwner { get; set; }

            public DateTime CreatedOn { get; set; }
            public DateTime ApprovedOn { get; set; }
            public string Group_Name { get; set; }
            public string RequiredAccess { get; set; }
            public string Remarks { get; set; }

        }


        public class GroupRequestModel
        {
            public int ID { get; set; }

            public string RefNo { get; set; }
            public string EmpCode { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string ProjectCode { get; set; }
            public string Project { get; set; }

            public string ProcessOwner { get; set; }

            public string ProcessOwnerLoginID { get; set; }

            public string ProcessOwnerEmail { get; set; }



            public string CreatedByEmail { get; set; }
            public string Createdby { get; set; }

            public DateTime CreatedOn { get; set; } 
            public DateTime ApprovedOn { get; set; }


            public string Group_Name { get; set; }
            public string RequiredAccess { get; set; }
            public string Remarks { get; set; }
            public string Reason { get; set; }
            public string ApprovedRemarks { get; set; }
            public bool IsReview { get; set; }
            public string ReviewRemarks { get; set; }

            public DateTime RejectedOn { get; set; }
            public string RejectedRemarks { get; set; }
            public DateTime ReviewedOn { get; set; }
            public int  Status { get; set; }
        }

        public class ProcessedServiceCount
        {
            public string Project { get; set; }
            public string ServiceName { get; set; }
            public int Total { get; set; }
        }
    }
}