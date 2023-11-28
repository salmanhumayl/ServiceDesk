using AJCCFM.Models.SocialNetWorking;
using AJESActiveDirectoryInterface;
using AJESeForm.Models;
using Core.Domain;
using Model;
using Services.GroupRequest;
using Services.Helper;
using Services.WhatsAppPost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{
  
    public class WhatsAppController : Controller
    {
        private IWhatsAppPost _WhatsAppPost;

        private IGroupRequest _GroupRequest;

        public string DirectoryPath;
        private string mailcontent;
        private string body;
        public ActionResult Add()
        {
            List<UserDetail> lADUser;

            var post = new WhatsApp();
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

        [ValidateInput(false)]
        [HttpPost]
        public async Task<ActionResult> SubmitRequest(WhatsApp model, string forwardto, string forwardName)
        {
            _WhatsAppPost = new WhatsAppPostService();
            _GroupRequest = new GroupRequestService();

            model.Email = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.EmpCode);
            model.Createdby = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
            model.SubmittedToEmail= AJESActiveDirectoryInterface.AJESAD.GetEmailAddress(forwardto);
            model.SubmittedTo = forwardto;

            string[] Name = forwardName.Split('-');

            RefNoID result = await _WhatsAppPost.SubmitLinkedInRequest(model);

            if (result == null)
            {
                return RedirectToAction("Add");
            }

            string mGuid = Guid.NewGuid().ToString();

            await _GroupRequest.LogEmail(result.ID, mGuid, "MW");


            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            string Link = url + "/WhatsApp/ShowRequest?token=" + mGuid;
            EmailManager VCTEmailService = new EmailManager();

            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LinkedInRequestDH.html");
            mailcontent = body.Replace("@ReqNo", result.RefNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@pwdchangelink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("LinkedSubject");
            VCTEmailService.ReceiverAddress = model.SubmittedToEmail;
            VCTEmailService.ReceiverDisplayName = "";
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

            _WhatsAppPost = new WhatsAppPostService();
            _GroupRequest = new GroupRequestService();

            int TransactionID = await _GroupRequest.GetToken(token, "M");

            if (TransactionID > 0)
            {
                WhatsAppRequestModel obj = _WhatsAppPost.ViewRequest<WhatsAppRequestModel>(TransactionID);
                DirectoryPath = Server.MapPath("~/Content/images_upload/") + obj.RefNo + @"\";

                ViewBag.Directory = DirectoryPath;

                if (Directory.Exists(DirectoryPath))
                {
                    obj.Images = Directory.GetFiles(DirectoryPath);
                    
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
            _WhatsAppPost = new WhatsAppPostService();
            WhatsAppRequestModel obj = _WhatsAppPost.ViewRequest<WhatsAppRequestModel>(TransactionID);

            DirectoryPath = Server.MapPath("~/Content/images_upload/") + obj.RefNo + @"\";

            ViewBag.Directory = DirectoryPath;

            if (Directory.Exists(DirectoryPath))
            {
                obj.Images = Directory.GetFiles(DirectoryPath);
               
            }

            return View(obj);
        }


    }
}