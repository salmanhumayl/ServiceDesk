using AJCCFM.Core;
using AJESActiveDirectoryInterface;
using Core.Domain;
using Services.Helper;
using Services.LinkedInPost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{
    public class LinkedinController : Controller
    {
        private ILinkedInPost _LinkedInPost;


        public ActionResult Add()
        {
            var post = new LinkedInPost();
            string Empcode = AJESAD.GetEmpNo(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));
            string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(Empcode);
            var empDetail = Common.GetEmpData<Core.EmployeeDetail>(Empcode);
            post.Name = empDetail.EmpName;
            post.EmpCode = empDetail.EmpCode;
            post.Position = empDetail.Position;
            post.Project = empDetail.Project;
            post.ProjectCode = empDetail.ProjectCode;
            post.Email = EmpEmailAddress;
            return View(post);

        }
        [HttpPost]
        public async Task<ActionResult> SubmitRequest(LinkedInPost model)
        {
            _LinkedInPost = new LinkedInPostService();
          
            model.Createdby = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
            model.SubmittedTo = System.Configuration.ConfigurationManager.AppSettings.Get("HRForwardTo");
            model.SubmittedToEmail= System.Configuration.ConfigurationManager.AppSettings.Get("HRManagerEmail");

            var result =await _LinkedInPost.SubmitLinkedInRequest(model);
            return RedirectToAction("Index", "Dashboard");

        }


        public ActionResult ViewRequest()
        {
            return View("View");
        }
        }
}