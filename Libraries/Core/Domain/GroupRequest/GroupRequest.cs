using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public class GroupRequest
    {
        public int ID{ get; set; }

        public string RefNo { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string ProjectCode { get; set; }
        public string Project { get; set; }

        public string ProcessOwner { get; set; }

        public string ProcessOwnerLoginID { get; set; }

        public string ProcessOwnerEmail { get; set; }


        public string Email { get; set; }

   



    }
}
