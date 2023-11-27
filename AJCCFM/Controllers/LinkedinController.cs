using AJCCFM.Core;
using AJCCFM.Models.SocialNetWorking;
using AJESActiveDirectoryInterface;
using Core.Domain;
using Services.Helper;
using Services.LinkedInPost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{
    public class LinkedinController : Controller
    {
        private ILinkedInPost _LinkedInPost;
        public string DirectoryPath;

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
        [ValidateInput(false)]
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

        public async Task<ActionResult> ShowRequest(string token)
        {

            _LinkedInPost = new LinkedInPostService();

            int TransactionID = await _LinkedInPost.GetToken(token, "F");

            if (TransactionID > 0)
            {
                var obj = _LinkedInPost.ViewRequest<LinkedRequestModel>(TransactionID);
                return View("ViewRequest", obj);
            }
            else
            {
                ViewBag.Token = token;

                return View("Processed");
            }
        }

        public ActionResult ViewRequest(int TransactionID, string mode)
        {

            ViewBag.Mode = mode;
            _LinkedInPost = new LinkedInPostService();
            LinkedRequestModel obj = _LinkedInPost.ViewRequest<LinkedRequestModel>(TransactionID);

            DirectoryPath = Server.MapPath("~/Content/images_upload/") + obj.RefNo + @"\";

            ViewBag.Directory = DirectoryPath;

            obj.Images = Directory.GetFiles(DirectoryPath);
            obj.Postedimages = Directory.EnumerateFiles(DirectoryPath).Select(fn => DirectoryPath +  Path.GetFileName(fn));


            return View(obj);
        }


        public FileContentResult DownloadSupportFiles(string RefNo,string fileName)
        {
            DirectoryPath = Server.MapPath("~/Content/images_upload/") + RefNo + @"\";
            var filePath = Path.Combine(DirectoryPath, fileName);

            return File(System.IO.File.ReadAllBytes(filePath), "application/octet-stream", fileName);
          
        }
     
    }
}