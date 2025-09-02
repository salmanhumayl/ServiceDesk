using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.Service
{
    public class EzwareProgress
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }



    }

    public class EzwarePending
    {
        public int ID { get; set; }
        public string RefNo { get; set; }


        public string EmpCode { get; set; }
        public string Name { get; set; }


        public DateTime CreatedOn { get; set; }
        public string Createdby { get; set; }

        public string Status { get; set; }


        public string StatusDescription { get; set; }
        public string AssystNo { get; set; }
        public string Project { get; set; }
        public string JDEAddressNO { get; set; }

    }


    public class LogIncident
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Assignedto { get; set; }
        public DateTime AssignedOn { get; set; }

        public DateTime Completedon { get; set; }


        public string Createdby { get; set; }

        public string Status { get; set; }


        public string StatusDescription { get; set; }
        public string AssystNo { get; set; }
        public string Project { get; set; }
        

    }
    public class Ezware
    {
    }
}