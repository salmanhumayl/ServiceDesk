using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Core.Domain
{
    public class CartService
    {
        public string SNo { get; set; }
        public  int RecordId { get; set; }
        public string CartId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
    
        public string Remarks { get; set; }

        public string Path { get; set; }

        public string EmpCode { get; set; }

        public string EmpName { get; set; }

       


    }
    

  

}
