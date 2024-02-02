using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models
{
    public class JDEModel
    {
       public Core.Domain.SD_JDE.JDE jde { get; set; }
        public AJCCFM.Core.EmployeeDetail empdetail { get; set; }

    }
}