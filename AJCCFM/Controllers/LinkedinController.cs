using AJCCFM.Core;
using AJCCFM.Models.SocialNetWorking;
using AJESActiveDirectoryInterface;
using AJESeForm.Models;
using Core.Domain;
using Model;
using Services.GroupRequest;
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
        private IGroupRequest _GroupRequest;

        public string DirectoryPath;
        private string mailcontent;
        private string body;

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
        [ValidateInput(false)]
        [HttpPost]
        public async Task<ActionResult> SubmitRequest(LinkedInPost model)
        {
            _LinkedInPost = new LinkedInPostService();
            _GroupRequest = new GroupRequestService();

            model.Email = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.EmpCode);
            model.Createdby = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
            model.SubmittedTo = System.Configuration.ConfigurationManager.AppSettings.Get("HRForwardTo");
            model.SubmittedToEmail= System.Configuration.ConfigurationManager.AppSettings.Get("HRManagerEmail");

            RefNoID result  = await _LinkedInPost.SubmitLinkedInRequest(model);

            if(result==null)
            {
                return RedirectToAction("Add");
            }

            string mGuid = Guid.NewGuid().ToString();

            await _GroupRequest.LogEmail(result.ID, mGuid, "M");

            
            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            string Link = url + "/Linkedin/ShowRequest?token=" + mGuid;
            EmailManager VCTEmailService = new EmailManager();

            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LinkedInRequestDH.html");
            mailcontent = body.Replace("@ReqNo", result.RefNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@pwdchangelink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("LinkedSubject");
            VCTEmailService.ReceiverAddress = model.SubmittedToEmail;
            VCTEmailService.ReceiverDisplayName ="";
            await VCTEmailService.SendEmail();

            //Users 

            if (!string.IsNullOrEmpty(model.Email))
            {
                //Send Email to requestor
                EmailManager VCTEmailServiceInit = new EmailManager();
                body = VCTEmailServiceInit.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LinkedInRequest-Init.html");
                mailcontent = body.Replace("@ReqNo", result.RefNo); //Replace Contenct...
                VCTEmailServiceInit.Body = mailcontent;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("LinkedSubject");
                VCTEmailServiceInit.ReceiverAddress = model.Email;
                VCTEmailServiceInit.ReceiverDisplayName = model.Name;
                await VCTEmailServiceInit.SendEmail();
            }
                return RedirectToAction("Index", "Dashboard");

        }

        public async Task<ActionResult> ShowRequest(string token)
        {

            _LinkedInPost = new LinkedInPostService();
            _GroupRequest = new GroupRequestService();
            int TransactionID = await _GroupRequest.GetToken(token, "M");

            if (TransactionID > 0)
            {
                LinkedRequestModel obj = _LinkedInPost.ViewRequest<LinkedRequestModel>(TransactionID);
                DirectoryPath = Server.MapPath("~/Content/images_upload/") + obj.RefNo + @"\";

                ViewBag.Directory = DirectoryPath;

                if (Directory.Exists(DirectoryPath))
                {
                    obj.Images = Directory.GetFiles(DirectoryPath);
                    // obj.Postedimages = Directory.EnumerateFiles(DirectoryPath).Select(fn => DirectoryPath + Path.GetFileName(fn));
                }
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

            if (Directory.Exists(DirectoryPath))
            {
                obj.Images = Directory.GetFiles(DirectoryPath);
               // obj.Postedimages = Directory.EnumerateFiles(DirectoryPath).Select(fn => DirectoryPath + Path.GetFileName(fn));
            }

            return View(obj);
        }


        public async Task<ActionResult> SubmitForApproval(int ID, string Remarks)
        {
            string returnURL = "";
            string body;
            _LinkedInPost = new LinkedInPostService();

            var obj = _LinkedInPost.ViewRequest<LinkedRequestModel>(ID);
            if (ID > 0)
            {
                var affectedRows = await _LinkedInPost.SubmitForApproval(ID, Remarks);
                if (!string.IsNullOrEmpty(obj.Email))
                {
                    string PName = AJESActiveDirectoryInterface.AJESAD.GetName(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));
                    EmailManager VCTEmailService = new EmailManager();
                    body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LinkedInStatusUpdate-Approved.html");
                    mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                    VCTEmailService.Body = mailcontent;
                    VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("LinkedSubject");
                    VCTEmailService.ReceiverAddress = obj.Email;
                    VCTEmailService.ReceiverDisplayName = obj.Name;
                    await VCTEmailService.SendEmail();
                }

                EmailManager VCTEmailServiceIT = new EmailManager();
                body = VCTEmailServiceIT.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LinkedInStatus-Approved(IT).html");
                mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                VCTEmailServiceIT.Body = mailcontent;
                VCTEmailServiceIT.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("LinkedSubject");
                VCTEmailServiceIT.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("SocialNetworkingdistribution");
                VCTEmailServiceIT.ReceiverDisplayName = System.Configuration.ConfigurationManager.AppSettings.Get("SocialNetworkingdistributionName"); ;
                await VCTEmailServiceIT.SendEmail();

                returnURL = Url.Action("Index", "Dashboard");


            }
            return Json(new { Result = returnURL });

        }


        public async Task<ActionResult> RejectForm(int ID, string Remarks)
        {
            string returnURL = "";

            _LinkedInPost = new LinkedInPostService();
            var affectedRows = await _LinkedInPost.RejectForm(ID, Remarks);

            var obj = _LinkedInPost.ViewRequest<GroupRequest>(ID);
            if (ID > 0)
            {
              
                EmailManager VCTEmailService = new EmailManager();
                string body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LinkedInStatusUpdate-Rejected.html");
                mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
               
                mailcontent = mailcontent.Replace("@Reason", Remarks); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("LinkedInReject");
                if (!string.IsNullOrEmpty(obj.Email))
                {
                    VCTEmailService.ReceiverAddress = obj.Email;
                    VCTEmailService.ReceiverDisplayName = obj.Name;
                    await VCTEmailService.SendEmail();
                }
                returnURL = Url.Action("Index", "Dashboard");


            }
            return Json(new { Result = returnURL });

        }

        public FileContentResult DownloadSupportFiles(string RefNo,string fileName)
        {
            DirectoryPath = Server.MapPath("~/Content/images_upload/") + RefNo + @"\";
            var filePath = Path.Combine(DirectoryPath, fileName);

            return File(System.IO.File.ReadAllBytes(filePath), "application/octet-stream", fileName);
          
        }
     
    }
}