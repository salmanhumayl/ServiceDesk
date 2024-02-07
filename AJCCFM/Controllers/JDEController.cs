using AJCCFM.Action_Filters.ErroHandler;
using AJCCFM.Core;
using AJCCFM.Core.Domain.SD_VPN;
using AJCCFM.Models;
using AJCCFM.Models.Service;
using AJESeForm.Models;
using Core.Domain;

using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Model;
using Newtonsoft.Json;
using Services.AJESServices;
using Services.GroupRequest;
using Services.JDE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace AJCCFM.Controllers
{
    [AjeErrorHandler]
    public class JDEController : Controller
    {

        private IJDE _JDEServices;
        private IGroupRequest _GroupRequest;
        private string mailcontent;

        public ActionResult Index()
        {
            List<UserDetail> lADUser;

            EmployeeDetail empDetail = new EmployeeDetail();
            JDEModel jdemodel = new JDEModel();

            if (TempData["EmpCode"] != null)
            {
                ViewBag.IsEmployeeExist = false;
                empDetail = Services.Helper.Common.GetEmpData<EmployeeDetail>(TempData["EmpCode"].ToString());

                ViewBag.EmpCode = TempData["EmpCode"].ToString();
                if (empDetail != null)
                {
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
                    ViewBag.IsEmployeeExist = true;
                    jdemodel.empdetail = empDetail;

                }
                return View(jdemodel);
            }
            // ViewBag.ADUser = obj;
            return View();

        }

        public ActionResult GetEmployee(string AJESEmpCode)
        {
            if (AJESEmpCode != "")
            {
                TempData["EmpCode"] = AJESEmpCode;
            }

            return RedirectToAction("Index");
        }

      
        [HttpPost]
        public async Task<ActionResult> SubmitRequest(JDEModel model, string forwardto, string forwardName)
        {
            if (model.empdetail == null)
            {
                return RedirectToAction("Index");
            }
            string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.empdetail.EmpCode);
            string SubmittToAddress = AJESActiveDirectoryInterface.AJESAD.GetEmailAddress(forwardto);

            string[] Name = forwardName.Split('-');

            _JDEServices = new JDEService();
            _GroupRequest = new GroupRequestService();


            IResponse result = _JDEServices.SubmitJDERequest(model, forwardto, EmpEmailAddress, SubmittToAddress);
            if (result.ErrorMessage!=null)
            {
                return RedirectToAction("Index");
            }
           
            // Send Email.....
            string body;
            string mGuid = Guid.NewGuid().ToString();
            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            await _GroupRequest.LogEmail(result.RecordID, mGuid, "J");

            string Link = url + "/JDE/ShowRequest?token=" + mGuid + "&Mode=E";

            EmailManager VCTEmailService = new EmailManager();
            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDERequest-DH.html");
            mailcontent = body.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@urllink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
            VCTEmailService.ReceiverAddress = SubmittToAddress;
            await VCTEmailService.SendEmail();


            if (!string.IsNullOrEmpty(EmpEmailAddress))
            {
                //Send Email to requestor
                EmailManager VCTEmailServiceInit = new EmailManager();
                body = VCTEmailServiceInit.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDERequest-Init.html");
                mailcontent = body.Replace("@forwardto", Name[0]); //Replace Contenct...
                mailcontent = mailcontent.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
                VCTEmailServiceInit.Body = mailcontent;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                VCTEmailServiceInit.ReceiverAddress = EmpEmailAddress;
                VCTEmailServiceInit.ReceiverDisplayName = model.empdetail.EmpName;
                await VCTEmailServiceInit.SendEmail();
            }
            else
            {
                EmailManager VCTEmailServiceInit = new EmailManager();

                VCTEmailServiceInit.Body = "Update Employee No In Active Direcotry:" + model.empdetail.EmpCode;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                VCTEmailServiceInit.ReceiverAddress = "ithelpdesk@ajes.ae";
                VCTEmailServiceInit.ReceiverDisplayName = "IT HELP DESK";
                await VCTEmailServiceInit.SendEmail();
            }

              return RedirectToAction("Index", "Dashboard");



        }

        public ActionResult ViewRequest(int TransactionID, string Service, string Mode)
        {

           
           
            _JDEServices = new JDEService();
        

            var obj = _JDEServices.ViewRequest<Core.Domain.SD_JDE.JDE>(TransactionID);
            ViewBag.Mode = Mode;


            return View(obj);
        }

        [HttpPost]
        public async Task<ActionResult> ApproveRequest(Core.Domain.SD_JDE.JDE model, string remarks)
        {
            _JDEServices = new JDEService();
            _GroupRequest = new GroupRequestService();
            if (model.Status == 0) // PM Level
            {
                model.Status = 1; //AWATING FOR JDE ADDRESSS NUMBER 
                model.Submitedto = model.Submitedto = System.Configuration.ConfigurationManager.AppSettings.Get("HRManager");

            }
            else
            {
                model.Status = -1;
            }

            var affectedRows = await _JDEServices.SubmitForApproval(model, remarks);

            //Send Email to 
            string body;
            if (model.Status == 1)
            {
                //string mGuid = Guid.NewGuid().ToString();
               // string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
              //  string Link = url + "/JDE/ShowRequest?token=" + mGuid + "&Mode=E";
              //  await _GroupRequest.LogEmail(model.ID, mGuid, "J");
                EmailManager VCTEmailService = new EmailManager();
                body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDERequest-HRManager.html");
                mailcontent = body.Replace("@Content", GenerateContent(model));
                mailcontent = mailcontent.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                VCTEmailService.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("HRManagerEmail");
                await VCTEmailService.SendEmail();

                EmailManager VCTEmailServiceIT = new EmailManager();
                body = VCTEmailServiceIT.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDERequest-HRManager.html");
                mailcontent = body.Replace("@Content", GenerateContent(model));
                mailcontent = mailcontent.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                VCTEmailServiceIT.Body = mailcontent;
                VCTEmailServiceIT.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                VCTEmailServiceIT.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("ProcessOwnerEmailIT");
                await VCTEmailServiceIT.SendEmail();

                if (!string.IsNullOrEmpty(model.Email)) //User 
                {

                    //Send Email to User .... 
                    EmailManager VCTEmailServiceUser = new EmailManager();
                    body = VCTEmailServiceUser.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDEStatusUpdate-Approved.html");
                    mailcontent = body.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                    VCTEmailServiceUser.Body = mailcontent;
                    VCTEmailServiceUser.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                    VCTEmailServiceUser.ReceiverAddress = model.Email;
                    await VCTEmailServiceUser.SendEmail();
                }
            }
            else if (model.Status == -1) //Approved //Send Email to User  and IT HELP DESK AS WELL 
            {

                if (!string.IsNullOrEmpty(model.Email))
                {
                    EmailManager VCTEmailService = new EmailManager();
                    body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDEStatusUpdate-Processed.html");
                    mailcontent = body.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                    VCTEmailService.Body = mailcontent;
                    VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                    VCTEmailService.ReceiverAddress = model.Email;
                    await VCTEmailService.SendEmail();
                }

                EmailManager VCTEmailServiceIT = new EmailManager();
                body = VCTEmailServiceIT.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDERequestStatus-Approved(IT).html");
                mailcontent = body.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                VCTEmailServiceIT.Body = mailcontent;
                VCTEmailServiceIT.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectJDE");
                VCTEmailServiceIT.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("groupdistribution");
                await VCTEmailServiceIT.SendEmail();



            }
            return RedirectToAction("Index", "Dashboard");
        }



        public async Task<ActionResult> RejectForm(int ID, string Remarks, string ServiceCode)
        {
            string returnURL = "";

            _JDEServices = new JDEService();

            var obj = _JDEServices.ViewRequest<AJCCFM.Core.Domain.SD_JDE.JDE>(ID);
            if (ID > 0)
            {
                var affectedRows = await _JDEServices.RejectForm(ID, Remarks, ServiceCode);
                EmailManager VCTEmailService = new EmailManager();
                string body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewJDEStatusUpdate-Rejected.html");
                mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                mailcontent = mailcontent.Replace("@Reason", Remarks); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectRejectJDE");
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



        public async Task<ActionResult> ArchiveJDEReturnPartialView(string AssetsNo, int RecordID)
        {


            _JDEServices = new JDEService();

            var response = await _JDEServices.ArchiveRecord(AssetsNo, RecordID);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };





        }
           
          public async Task<ActionResult> AddressNoReturnPartialView(string JDENo, int RecordID)
        {


            _JDEServices = new JDEService();

            var response = await _JDEServices.UpdateJDEAddressNo(JDENo, RecordID);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };





        }
           

        public async Task<ActionResult> ShowRequest(string token, string Mode)
        {

            _GroupRequest = new GroupRequestService();
            _JDEServices = new JDEService();
            int TransactionID = await _GroupRequest.GetToken(token, "J");

            if (TransactionID > 0)
            {
                return RedirectToAction("ViewRequest", new { TransactionID = TransactionID, Mode = Mode });

            }
            else
            {
                ViewBag.Token = token;

                return View("Processed");
            }
        }
      
    
        public ActionResult MyJDERequest()
        {
            _JDEServices = new JDEService();

            var obj = _JDEServices.MyJDERequest<JDEPending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return PartialView("MyJDERequest", obj);
        }

        public ActionResult AllJDERequest()
        {
            _JDEServices = new JDEService();

            var obj = _JDEServices.AllJDERequest<JDEPending>();

            return PartialView("AllJDERequest", obj);
        }


    
      

        public async Task<FileResult> GeneratePDF(int RecordID)
        {
            _JDEServices = new JDEService();

            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");

            url = url + "/content/images/logo.png";

            var obj = await _JDEServices.GeneratePDF<PFDModel>(RecordID);

            StringBuilder sb = new StringBuilder();
            sb.Append("<header class='clearfix'>");
            sb.Append("<img src='" + url + "' > ");
            sb.Append("<br>");
            sb.Append("<br>");

            sb.Append("<h1>JDE Request - " + obj[0].RefNo + " </h1>");
            sb.Append("<br>");
           
            sb.Append("<br>");
            sb.Append("<table border='1'>");
            sb.Append("<tr>");
            sb.Append("<td>Employee No</td>");
            sb.Append("<td> " + obj[0].EmpCode + " </td>");
            sb.Append("<td>Name</td>");
            sb.Append("<td>" + obj[0].Name + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Position</td>");
            sb.Append("<td>" + obj[0].Position + "</td>");
            sb.Append("<td>Project</td>");
            sb.Append("<td>" + obj[0].ProjectCode + "</td>");
            sb.Append("<td>JDE Role & Security</td>");
            sb.Append("<td colspan=3>" + obj[0].Reason + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>JDE Address No </td>");
            sb.Append("<td colspan=3>" + obj[0].JDEAddressNO + "</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</header>");
            sb.Append("<br>");
            sb.Append("<main>");

            sb.Append("<table border='1'>");

            sb.Append("<tr>");


            sb.Append("<td >Approved By</th>");
            sb.Append("<td >Approved On</th>");
            sb.Append("</tr>");

            for (int i = 0; i <= obj.Count - 1; i++)
            {

                sb.Append("<tr>");
                sb.Append("<td>" + obj[i].ApprovedBy + "</th>");
                sb.Append("<td>" + obj[i].ApprovedOn.ToString("dd/MM/yyyy hh:mm:ss") + "</th>");
                sb.Append("</tr>");

            }

            sb.Append("</table>");
            sb.Append("<br>");

            sb.Append("</main>");
            sb.Append("<footer>");
            sb.Append("<br>");
            sb.Append("Document was created on a computer and is valid without the signature and seal.");
            sb.Append("</footer>");


            StringReader sr = new StringReader(sb.ToString());
            Document pdfDoc = new Document(PageSize.EXECUTIVE);
            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                pdfDoc.Open();

                htmlparser.Parse(sr);
                pdfDoc.Close();

                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();
                return File(bytes, "application/pdf", "JDERequest-" + obj[0].RefNo + ".PDF");
            }

        }


        private string GenerateContent(Core.Domain.SD_JDE.JDE model)
        {
          


            StringBuilder sb = new StringBuilder();
            sb.Append("<table cellpadding='5' cellspacing='2' style='border:1px solid black'><tr><td style='border: 1px solid black'>Emp Code</td><td style='border: 1px solid black'>" + model.EmpCode + "</td></tr>");
            sb.Append("<tr><td style='border: 1px solid black'>Emp Name</td><td style='border: 1px solid black'>" + model.Name + "</td></tr>");
            sb.Append("<tr><td style='border: 1px solid black'>Designation</td><td style='border: 1px solid black'>" + model.Position + "</td></tr>");
            sb.Append("<tr><td style='border: 1px solid black'>Department</td><td style='border: 1px solid black'>" + model.Department + "</td></tr>");
            sb.Append("<tr><td style='border: 1px solid black'>Procject Code</td><td style='border: 1px solid black'>" + model.ProjectCode  + "</td></tr>");
            sb.Append("<tr><td style='border: 1px solid black'>JDE Role & Security</td><td style='border: 1px solid black'>" + model.Reason + "</td></tr>");
            sb.Append("<tr><td style='border: 1px solid black'>JDE Address Number</td><td style='border: 1px solid black'>&nbsp;&nbsp</td></tr></table>");
            return sb.ToString();

        }


    }


}