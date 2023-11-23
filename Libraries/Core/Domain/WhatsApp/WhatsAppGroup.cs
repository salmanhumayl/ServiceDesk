using System;
using System.Collections.Generic;
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

        public string Phone { get; set; }

        public bool IsJoined { get; set; }
        public string JoinedOn { get; set; }
    }
}
