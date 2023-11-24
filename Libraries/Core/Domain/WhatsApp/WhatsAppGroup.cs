using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class WhatsAppGroup
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

        [StringLength(35, ErrorMessage = "Not allowed more than 35 characters")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\+971(\d{9}|\s\(\d{3}\)\s\(\d{3}\s\d{4}\))$", ErrorMessage = "Invalid Mobile Number")]
        [Required(ErrorMessage = "Required Mobile")]
        public string Phone { get; set; }

        public bool IsJoined { get; set; }
        public string JoinedOn { get; set; }
    }
}
