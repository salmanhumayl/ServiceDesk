using AJCCFM.Core;
using AJESActiveDirectoryInterface;
using Core.Domain;
using Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{
    public class LinkedinController : Controller
    {
        
        public ActionResult Add()
        {
            var post = new LinkedInPost();
            string Empcode = AJESAD.GetEmpNo(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));
            var empDetail = Common.GetEmpData<Core.EmployeeDetail>(Empcode);
            post.Name = empDetail.EmpName;
            post.EmpCode = empDetail.EmpCode;
            post.Position = empDetail.Position;
            post.Project = empDetail.Project;
            post.ProjectCode = empDetail.ProjectCode;
            return View(post);
            
        }
    }
}