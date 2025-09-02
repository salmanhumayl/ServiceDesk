using AJCCFM.Core;

using AJCCFM.Models;
using AJCCFM.Models.Service;
using AJESeForm.Models;
using Core.Domain;
using Model;
using Services.GroupRequest;
using Services.LogIncedent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{

    public class LogIncedentController : Controller
    {

        private ILog _LogServices;
        private LogService _LogService;
        private IGroupRequest _GroupRequest;
        private string mailcontent;

        // GET: LogIncedent
        public ActionResult Index()
        {
            List<UserDetail> lADUser;

            EmployeeDetail empDetail = new EmployeeDetail();
            LogModel logmodel = new LogModel();

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
                    logmodel.empdetail = empDetail;

                }
                return View(logmodel);
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
        public async Task<ActionResult> SubmitRequest(LogModel model)
        {
            if (model.empdetail == null)
            {
                return RedirectToAction("Index");
            }
            string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.empdetail.EmpCode);
            string SubmittToAddress = "salman.mazhar.ajes.ae";

          
            _LogService = new LogService();
            _GroupRequest = new GroupRequestService();


            IResponse result = _LogService.SubmitJDERequest(model, "smazhar", EmpEmailAddress, SubmittToAddress);
            if (result.ErrorMessage != null)
            {
                return RedirectToAction("Index");
            }

            // Send Email.....
            string body;
            string mGuid = Guid.NewGuid().ToString();
            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            

            string Link = url + "/LogIncedent/ViewRequest?TransactionID=" + result.RecordID + "&mode=E";


            if (!string.IsNullOrEmpty(EmpEmailAddress))
            {
                //Send Email to requestor
                EmailManager VCTEmailServiceInit = new EmailManager();
                body = VCTEmailServiceInit.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewLogRequest-Init.html");
                mailcontent = body.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
                VCTEmailServiceInit.Body = mailcontent;
                VCTEmailServiceInit.Subject = "Log Request";
                VCTEmailServiceInit.ReceiverAddress = EmpEmailAddress;
                VCTEmailServiceInit.ReceiverDisplayName = model.empdetail.EmpName;
                await VCTEmailServiceInit.SendEmail();
            }
            
                EmailManager VCTEmailService = new EmailManager();

               body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewLogRequest-IT.html");
               mailcontent = body.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
               mailcontent = mailcontent.Replace("@urllink", Link); //Replace Contenct...
               VCTEmailService.Body = mailcontent;
               VCTEmailService.ReceiverAddress = "salman.mazhar@ajes.ae";
               VCTEmailService.ReceiverDisplayName = "IT HELP DESK";
               await VCTEmailService.SendEmail();
                return RedirectToAction("Processed");



        }

        public ActionResult Processed()
        {
            return View();
        }
        public ActionResult ViewRequest(int TransactionID)
        {

            _LogServices = new LogService();
            var obj = _LogServices.ViewRequest<Core.Domain.Log.Log>(TransactionID);
            Dictionary<string, string> lstProjectManager = new Dictionary<string, string>();
            lstProjectManager.Add("knazeer", "Kashif Nazir");
            lstProjectManager.Add("apasha", "Ashraf Pasha");
            lstProjectManager.Add("rdeveraj", "Roshan");
            lstProjectManager.Add("rkrishna", "Rama Krishna");
            lstProjectManager.Add("aansari", "Amir Ansari");
            lstProjectManager.Add("ithelpdesk", "Bushra Murtuza");
            lstProjectManager.Add("smazhar", "Salman Mazhar");

            ViewBag.ProcessOwner = new SelectList(lstProjectManager, "Key", "Value");


            return View(obj);
        }


        [HttpPost]
        public async Task<ActionResult> Assignment(int ID, string ProcessOwner, string Remarks, string RefNo,string AffectedUser)
        {
            _LogService = new LogService();
            string body = "";
            ProcesOwnerDetail processOwnerDetail = new ProcesOwnerDetail();
            _GroupRequest = new GroupRequestService();
            processOwnerDetail = AJESActiveDirectoryInterface.AJESAD.ProcessOwnerDetail(ProcessOwner);

            await _LogService.Assignment(ID, processOwnerDetail.ProcessOwnerLoginID, Remarks);

           
          
            EmailManager VCTEmailService = new EmailManager();

            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewLogAssignment.html");
            mailcontent = body.Replace("@Name", ProcessOwner); //Replace Contenct...
            mailcontent = mailcontent.Replace("@ReqNo", RefNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@Remarks", Remarks); //Replace Contenct...
            mailcontent = mailcontent.Replace("@AffectedUser", AffectedUser); //Replace Contenct...

            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject ="Incident " + RefNo + " has been Assigned to you";
            VCTEmailService.ReceiverAddress = processOwnerDetail.ProcessOwnerEmail;
            VCTEmailService.ReceiverDisplayName = ProcessOwner;
            await VCTEmailService.SendEmail();


            return Json(new { Result = Url.Action("index", "Dashboard") });
        }


        public async Task<ActionResult> Completed(int ID, string Remarks, string RefNo,string Email,string Name)
        {
            _LogService = new LogService();
            string body = "";
            
            await _LogService.Completed(ID,Remarks);
            
            EmailManager VCTEmailService = new EmailManager();

            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\LogCompleted.html");
            mailcontent = body.Replace("@ReqNo", RefNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@Remarks", Remarks); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = "Incident " + RefNo + " has been Completed";
            VCTEmailService.ReceiverAddress = Email;
            VCTEmailService.ReceiverDisplayName = Name;
            await VCTEmailService.SendEmail();


            return Json(new { Result = Url.Action("index", "Dashboard") });
        }

        public ActionResult AllLog()
        {
            _LogServices = new LogService();

            var obj = _LogServices.AllLogRequest<LogIncident>();

            return PartialView("AllLog", obj);
        }










    }
}