using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RoleApproval
{
    public class ApprovalAuthorityDetail
    {
        public string AuthorityName { get; set; }
        public string LoginID { get; set; }
        public int RoleID { get; set; }
    }

    public class NextLevel
    {
        public string SeqName { get; set; }
        public int SeqNo { get; set; }
        public int RoleID { get; set; }
    }
}
