using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class IResponse
    {
        public string ErrorMessage { get; set; }
        public string RequestNo { get; set; }
        public int RecordID { get; set; }
    }
}
