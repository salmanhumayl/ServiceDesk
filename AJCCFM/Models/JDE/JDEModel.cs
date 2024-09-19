using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.Service
{
    public class JDEProgress
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }

       

    }

    public class JDEPending
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
      

        public string EmpCode { get; set; }
        public string Name { get; set; }


        public DateTime CreatedOn { get; set; }
        public string Createdby { get; set; }

        public int Status { get; set; }

      
        public string StatusDescription { get; set; }
        public string AssystNo { get; set; }
        public string Project { get; set; }
        public string JDEAddressNO { get; set; }

    }

    public class PFDModel
    {
        public int ID { get; set; }

        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string Department { get; set; }
        public string Position { get; set; }
        public string ProjectCode { get; set; }
        public string Project { get; set; }

        public string Remarks { get; set; }


        public string Reason { get; set; }
        public string Justification { get; set; }
        public string JDEAddressNO { get; set; }

        public int Status { get; set; }
        public string Submitedto { get; set; }

        public string ApprovedBy { get; set; }


        public DateTime ApprovedOn { get; set; }
    }
}