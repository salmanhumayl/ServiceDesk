using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Core.Domain.EzwareRequest
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


    
    
}