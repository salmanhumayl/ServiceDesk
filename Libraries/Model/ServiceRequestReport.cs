using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ServiceRequestReport
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

        public string Path { get; set; }

        public string AccessType { get; set; }

        public int Status { get; set; }
        public string Submitedto { get; set; }
        public string ServiceName { get; set; }

        public string ServiceCode { get; set; }
        public string Others { get; set; }

        public string ApprovedBy {get;set;}


        public DateTime ApprovedOn { get; set; }

    }
    }

