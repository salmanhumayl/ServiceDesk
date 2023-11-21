using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Core
{
    public class EmployeeDetail
    {
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
       
        public string Position { get; set; }
        public string Project { get; set; }
        public string ProjectCode { get; set; }
      
        public string Department { get; set; }
        public string ForemanCode { get; set; }

    }
}