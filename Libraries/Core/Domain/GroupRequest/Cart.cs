using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Core.Domain
{
    public class Cart
    {
        public string SNo { get; set; }
        public  int RecordId { get; set; }
        public string CartId { get; set; }
        public string Group_Name { get; set; }
        public string RequiredAccess { get; set; }
        public string Reason { get; set; }
        public string Remarks { get; set; }

        public string ProcessOwner { get; set; }

        public string ProcessOwnerEmail { get; set; }
        public string ProcessOwnerLoginID { get; set; }
        public string EmpCode { get; set; }

        public string EmpName { get; set; }

       


    }
    

    public class CartDetail
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public decimal Amount { get; set; }
        public string InvoiceNo { get; set; }
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; }
    }

    public class ProcesOwnerDetail
    {
        public string ProcessOwnerEmail { get; set; }
        public string ProcessOwnerLoginID { get; set; }
        

    }

}
