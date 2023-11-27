using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.SocialNetWorking
{
    public class LinkInPending
    {
        public int ID { get; set; }
        public string RefNo { get; set; }
    

        public string EmpCode { get; set; }
        public string Name { get; set; }

             
        public int Status { get; set; }

      
        public string AssystNo { get; set; }
        public string Project { get; set; }

        public string SNType  { get; set; }
    }

    public class LinkedRequestModel
    {

        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Project { get; set; }
        public string Post { get; set; }

        public IEnumerable<string> Postedimages { get; set; }

        public string[] Images { get; set; }

    }
}