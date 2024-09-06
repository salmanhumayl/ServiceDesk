using Core.Domain.EzwareRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.EzwareProject
{
    public class EzwareModel
    {
        public List<RightModel> EzwareRights { get; set; }
        public AJCCFM.Core.EmployeeDetail empdetail { get; set; }
        public string ToProject { get; set; }
    }

    public class EzwareViewModel
    {
        public List<RightModel> EzwareRights { get; set; }
        public RequestHeader empdetail { get; set; }
        
    }

    public class RequestHeader
    {
        public int ID  { get; set; }
        public int Status { get; set; }
        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }

        public string Position { get; set; }
        public string Project { get; set; }
        public string ProjectCode { get; set; }

        public string AssignedProject { get; set; }
        public string Email { get; set; }
    }



    public class rptReportModel
    {
        public int ID { get; set; }
        public int Status { get; set; }
        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }

        public string Position { get; set; }
        public string Project { get; set; }
        public string ProjectCode { get; set; }

        public string AssignedProject { get; set; }
        public string Email { get; set; }

        public string ApprovedBy { get; set; }
        public DateTime ApprovedOn { get; set; }

        public DateTime CreatedOn { get; set; }
        public string Createdby { get; set; }
        public int Parent { get; set; }
        public string form_name { get; set; }
        public bool View { get; set; }
        public bool Delete { get; set; }
        public bool Print { get; set; }
        public bool Edit { get; set; }
        public bool All { get; set; }
        public bool Create { get; set; }


    }
}
