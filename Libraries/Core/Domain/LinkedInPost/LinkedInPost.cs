using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Core.Domain
{
    public class LinkedInPost
    {
        public int ID { get; set; }

        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string ProjectCode { get; set; }
        public string Project { get; set; }
        public string Remarks { get; set; }

        public string Email { get; set; }

        public string SubmittedTo { get; set; }
        public string SubmittedToEmail { get; set; }

        [Required(ErrorMessage = "Required")]

        [AllowHtml]
        public string Post { get; set; }
        public bool IsAttachment { get; set; }

       
        public HttpPostedFileBase[] PostDocument{ get; set; }

        public string Createdby { get; set; }


    }
}
