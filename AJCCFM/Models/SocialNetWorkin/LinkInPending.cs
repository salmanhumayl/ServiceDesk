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
        public string StatusDes { get; set; }
        public string Post { get; set; }

        public string AssystNo { get; set; }
        public string Project { get; set; }
        public DateTime CreatedOn { get; set; }
        public string SNType  { get; set; }
    }

    public class LinkedRequestModel
    {

        public int ID { get; set; }
        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Project { get; set; }

        public string Email { get; set; }
        public string Post { get; set; }

        public IEnumerable<string> Postedimages { get; set; }

        public string[] Images { get; set; }

    }


    public class WhatsAppRequestModel
    {

        public int ID { get; set; }
        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Project { get; set; }

        public int Status { get; set; }
        public string Email { get; set; }
        public string Post { get; set; }

     


        public IEnumerable<string> Postedimages { get; set; }

        public string[] Images { get; set; }

    }
}