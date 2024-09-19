using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJCCFM.Core.Domain.SD_JDE
{
    public class JDE
    {
        public int ID { get; set; }

        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string Department { get; set; }
        public string Position { get; set; }
        public string ProjectCode { get; set; }
        public string Project { get; set; }

        public string Remarks { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Justification { get; set; }
        public string JDEAddressNO { get; set; }

        public int Status { get; set; }
        public string Submitedto { get; set; }
      
    }
}
