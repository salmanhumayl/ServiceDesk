using Core.Domain.EzwareRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.EzwareProject
{
    public class EzwareModel
    {
        public List<RightModel> EzwareRights { get; set; }
        public AJCCFM.Core.EmployeeDetail empdetail { get; set; }
        public string ToProject { get; set; }
    }
}
