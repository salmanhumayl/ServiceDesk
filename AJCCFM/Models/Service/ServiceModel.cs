using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.Service
{
    public class ServiceProgress
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
        public string Service { get; set; }

        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }

        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }


    }

    public class ServicePending
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
        public string Service { get; set; }

        public string EmpCode { get; set; }
        public string Name { get; set; }

     
        public DateTime CreatedOn { get; set; }
        public string Createdby { get; set; }

        public int Status { get; set; }

        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public string StatusDescription { get; set; }
        public string AssystNo { get; set; }
        public string Project { get; set; }

    }
}