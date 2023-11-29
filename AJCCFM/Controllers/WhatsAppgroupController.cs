using AJESActiveDirectoryInterface;
using Core.Domain;
using Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{
    public class WhatsAppgroupController : Controller
    {
        // GET: WhatsAppgroup

        public ActionResult Add()
        {
            List<UserDetail> lADUser;

            var post = new WhatsAppGroup();
            string Empcode = AJESAD.GetEmpNo(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            var empDetail = Common.GetEmpData<Core.EmployeeDetail>(Empcode);
            post.Name = empDetail.EmpName;
            post.EmpCode = empDetail.EmpCode;
            post.Position = empDetail.Position;
            post.Project = empDetail.Project;
            post.ProjectCode = empDetail.ProjectCode;


            if (empDetail.ProjectCode == "8000" || empDetail.ProjectCode == "8000_1")
            {
                AJESActiveDirectoryInterface.AJESAD.RoleID = 1036; //Support office HOD 
                lADUser = AJESActiveDirectoryInterface.AJESAD.GetADUsers();
                ViewBag.ForemanCode = empDetail.ForemanCode;
            }
            else
            {
                AJESActiveDirectoryInterface.AJESAD.RoleID = 1037;
                lADUser = AJESActiveDirectoryInterface.AJESAD.GetADUsers();

            }
            UserDetail userdetail = new UserDetail();
            userdetail.DisplayText = "Please Select";
            // userdetail.LoginName = "X";
            lADUser.Add(userdetail);
            ViewBag.ADUser = lADUser;
            return View(post);


        }


       

    }
}