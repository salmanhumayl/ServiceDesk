using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.EzwareRequest
{
    public class RightModel
    {

         
        public string form_name { get; set; }
        public bool View { get; set; }
        public bool Delete { get; set; }
        public bool Print { get; set; }
        public bool Edit { get; set; }
        public bool All { get; set; }
        public bool Create { get; set; }
      

    }


    public class EzwareModel
    {
        public List<RightModel> EzwareRights   { get; set; }

        public AJCCFM.Core.EmployeeDetail empdetail { get; set; }

        public string ToProject { get; set; }

    }
    public class ProjectDetail
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}