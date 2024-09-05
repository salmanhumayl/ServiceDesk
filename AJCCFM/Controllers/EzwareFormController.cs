using AJCCFM.Core;
using AJCCFM.Models.Service;
using AJESeForm.Models;
using Core.Domain;
using Core.Domain.EzwareRequest;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Model;
using Model.EzwareProject;
using Services.EzwareProject;
using Services.GroupRequest;
using Services.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static AJCCFM.Models.Extention;

namespace AJCCFM.Controllers
{
    public class EzwareFormController : Controller
    {
        private IGroupRequest _GroupRequest;
        EzwareProjectService _EzwareProjectServices;
        List<RightModel> rights = new List<RightModel>();
        private string mailcontent;

        public ActionResult Index(string Mode)
        {
            List<UserDetail> lADUser;
            if (TempData["EmpCode"] != null)
            {
                List<ProjectDetail> objProject;
                EzwareModel EzwareModel = new EzwareModel();
                EmployeeDetail empDetail = new EmployeeDetail();

                ViewBag.IsEmployeeExist = false;
                if (TempData["Mode"].ToString() == "P")
                {
                    rights = GetBlankUserRights();
                }
                else
                {
                    rights = GetBlankSOUserRights();
                }
                empDetail = Services.Helper.Common.GetEmpData<EmployeeDetail>(TempData["EmpCode"].ToString());
                objProject = Common.GetProject<ProjectDetail>(TempData["Mode"].ToString()).ToList();
                 ViewBag.ToProject = new SelectList(objProject, "Code", "Name");
               
                ViewBag.EmpCode = TempData["EmpCode"].ToString();
                if (empDetail != null)
                {
                    if (TempData["Mode"].ToString() == "S")
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

                    EzwareModel.EzwareRights = rights;
                    EzwareModel.empdetail = empDetail;
                    return View(EzwareModel);
               }

            }
            ViewBag.Mode = Mode;
            return View();
        }

        public ActionResult GetEmployee(string AJESEmpCode,string Mode)
        {
            if (AJESEmpCode != "")
            {
                TempData["EmpCode"] = AJESEmpCode;
                TempData["Mode"] = Mode;
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<ActionResult> SubmitRequest(EzwareModel model, string forwardto, string forwardName)
        {
            if (model.empdetail == null)
            {
                return RedirectToAction("Index");
            }
            _GroupRequest = new GroupRequestService();
            _EzwareProjectServices = new EzwareProjectService();
            string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.empdetail.EmpCode);
            string SubmittToAddress = AJESActiveDirectoryInterface.AJESAD.GetEmailAddress(forwardto);
            string[] Name = forwardName.Split('-');
         
            IResponse result = _EzwareProjectServices.SubmitRequest(model, forwardto, EmpEmailAddress, SubmittToAddress);
            if (result.ErrorMessage != null)
            {
                return RedirectToAction("Index");
            }
            
            // Send Email.....
            string body;
            string mGuid = Guid.NewGuid().ToString();
            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            await _GroupRequest.LogEmail(result.RecordID, mGuid, "EZP");

            string Link = url + "/JDE/ShowRequest?token=" + mGuid + "&Mode=E";

            EmailManager VCTEmailService = new EmailManager();
            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewEzwareRequest-DH.html");
            mailcontent = body.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@urllink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectEzWare");
            VCTEmailService.ReceiverAddress = EmpEmailAddress;
            await VCTEmailService.SendEmail();


            if (!string.IsNullOrEmpty(EmpEmailAddress))
            {
                //Send Email to requestor
                EmailManager VCTEmailServiceInit = new EmailManager();
                body = VCTEmailServiceInit.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewEzwareRequest-Init.html");
                mailcontent = body.Replace("@forwardto", Name[0]); //Replace Contenct...
                mailcontent = mailcontent.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
                VCTEmailServiceInit.Body = mailcontent;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectEzWare");
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


        public async Task<ActionResult> ShowRequest(string token, string Mode)
        {
            _GroupRequest = new GroupRequestService();
            _EzwareProjectServices = new EzwareProjectService();
            
            int TransactionID = await _GroupRequest.GetToken(token, "EZP");

            if (TransactionID > 0)
            {
                return RedirectToAction("ViewRequest", new { TransactionID = TransactionID });
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
            _EzwareProjectServices = new EzwareProjectService();
            var obj = _EzwareProjectServices.ViewRequest<RequestHeader>(TransactionID);
            return View(obj);
        }




        [HttpPost]
        public async Task<ActionResult> ApproveRequest(int ID, string RefNo, string Email, string remarks)
        {
            _EzwareProjectServices = new EzwareProjectService();
            _GroupRequest = new GroupRequestService();
            
            var affectedRows = await _EzwareProjectServices.SubmitForApproval(ID, remarks);

            //Send Email to 
            string body;
            if (!string.IsNullOrEmpty(Email))
                 {
                    EmailManager VCTEmailService = new EmailManager();
                    body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewEZStatusUpdate-Processed.html");
                    mailcontent = body.Replace("@ReqNo", RefNo); //Replace Contenct...
                    VCTEmailService.Body = mailcontent;
                    VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectEzWare");
                    VCTEmailService.ReceiverAddress = Email;
                    await VCTEmailService.SendEmail();
                 }
                    EmailManager VCTEmailServiceIT = new EmailManager();
                    body = VCTEmailServiceIT.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewEZRequestStatus-Approved(IT).html");
                    mailcontent = body.Replace("@ReqNo", RefNo); //Replace Contenct...
                    VCTEmailServiceIT.Body = mailcontent;
                    VCTEmailServiceIT.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectEzWare");
                    VCTEmailServiceIT.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("groupdistribution");
                    await VCTEmailServiceIT.SendEmail();
                    return RedirectToAction("Index", "Dashboard");
        }



        public async Task<ActionResult> RejectForm(int ID, string Remarks)
        {
            string returnURL = "";

            _EzwareProjectServices = new EzwareProjectService();

            var obj = _EzwareProjectServices.ViewRequest<RequestHeader>(ID);
           
            if (ID > 0)
            {
                var affectedRows = await _EzwareProjectServices.RejectForm(ID, Remarks);
                EmailManager VCTEmailService = new EmailManager();
                string body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewEZStatusUpdate-Rejected.html");
                mailcontent = body.Replace("@ReqNo", obj.empdetail.RefNo); //Replace Contenct...
                mailcontent = mailcontent.Replace("@Reason", Remarks); //Replace Contenct.
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectRejectEZ");
                if (!string.IsNullOrEmpty(obj.empdetail.Email))
                {
                    VCTEmailService.ReceiverAddress = obj.empdetail.Email;
                    VCTEmailService.ReceiverDisplayName = obj.empdetail.Name;
                    await VCTEmailService.SendEmail();
                }
                returnURL = Url.Action("Index", "Dashboard");


            }
            return Json(new { Result = returnURL });

        }

        public FileResult GeneratePDF(int RecordID)
        {
            _EzwareProjectServices = new EzwareProjectService();
            StringBuilder sb = new StringBuilder();
           
            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");

            url = url + "/content/images/logo.png";

            var obj = _EzwareProjectServices.ViewRequest<RequestHeader>(RecordID);

          

            sb.Append("<header class='clearfix'>");
            sb.Append("<img src='" + url + "' > ");
            sb.Append("<br>");
     

            sb.Append("<h3> Al Jaber Energy Services</h3>");
            sb.Append("<br>");
            sb.Append("<h3> EzBusiness User Rights <h3>");
            sb.Append("<br>");
            sb.Append("<h3> RefNo:" + obj.empdetail.RefNo + " </h3>");
            sb.Append("<br>");
            sb.Append("<table border='1'>");
            sb.Append("<tr>");
            sb.Append("<td> Employee Code </td>");
            sb.Append("<td>" + obj.empdetail.EmpCode + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td> Employee Name </td>");
            sb.Append("<td>" + obj.empdetail.Name + "</td>");
            sb.Append("<td> Designation </td>");
            sb.Append("<td>" + obj.empdetail.Position + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td> Project </td>");
            sb.Append("<td>" + obj.empdetail.AssignedProject + "</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("<br>");
            sb.Append("<table border='1'>");

            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.Append("<th scope = 'col' width='40%'>Form Name</th>");
            sb.Append("<th scope = 'col' > All </ th >");
            sb.Append("<th scope='col'>Create</th>");
            sb.Append("<th scope = 'col' > View </ th >");
            sb.Append("<th scope='col'>Edit</th>");
            sb.Append("<th scope = 'col' > Delete </ th >");
            sb.Append("<th scope='col'>Print</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");

            foreach (var item in obj.EzwareRights)
            {
                   
                    sb.Append("<tr>");
                    sb.Append("<td>" + item.form_name + "</td>");
                    sb.Append("<td>" + ((bool)item.All ? "Y" : "N") + "</td>");
                    sb.Append("<td>" + ((bool)item.Create ? "Y" : "N") + "</td>");
                    sb.Append("<td>" + ((bool)item.View ? "Y" : "N") + "</td>");
                    sb.Append("<td>" + ((bool)item.Edit ? "Y" : "N") + "</td>");
                    sb.Append("<td>" + ((bool)item.Delete ? "Y" : "N") + "</td>");
                    sb.Append("<td>" + ((bool)item.Print ? "Y" : "N") + "</td>");
                    sb.Append("</tr>");
            }
            sb.Append("</tbody>");
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
                return File(bytes, "application/pdf", "UserRights-" + obj.empdetail.RefNo + ".PDF");
            }
        }

        public ActionResult AllEzwareRequest()
        {
            _EzwareProjectServices = new EzwareProjectService();

            var obj = _EzwareProjectServices.AllEzwareRequest<EzwarePending>();

            return PartialView("AllEZRequest", obj);
        }
        public List<RightModel> GetBlankUserRights()
        {
            RightModel obj;
            foreach (String s in this.GetFormsTimeKeeper())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Forms - Time Keeper: " + s;
                rights.Add(obj);
               
            }

            foreach (String s in this.GetFormsCostControl())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Forms - Cost Control: " + s;
                rights.Add(obj);
                
            }

            foreach (String s in this.GetReportsTimeKeeper())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Reports - Time Keeper: " + s;
                rights.Add(obj);
            }

            foreach (String s in this.GetReportsCostControl())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Reports - Cost Control: " + s;
                rights.Add(obj);
            }

            foreach (String s in this.GetReportsEquipments())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Reports - Equipment: " + s;
                rights.Add(obj);
                
            }

            
            return rights;


        }
        private List<String> GetFormsTimeKeeper()
        {
            return new List<String> {
                "Employee",
                "Sub Contractor Labour",
                "Daily Time Sheet",
                "Single Time Sheet 1",
                "Single Time SHeet 2",
                "Equipment Time Sheet",
                "Scan Details",
                "Activity Change",
                "Timesheet Query",
                "Employee Transfer Out",
                "Employee Transfer Out",
                "Transfer To Al Jaber EST",
                "Transfer From Al Jaber EST",
                "Transfer To Qatar",
                "Transfer From Qatar",
                "Employee Dept. Transfer",
                "Foreman Change",
                "Foreman Transfer",
                "Budgets",
                "Productivity",
            };
        }

        // Returns list of form names
        private List<String> GetFormsCostControl()
        {
            return new List<String>
            {
                "Location",
                "Sub Location",
                "Owner",
                "Unit",
                "Activity",
                "Sub Activity",
                "Project Activity",
                "Project Opening Balance",
                "Disciplines",
                "Item",
                "New Activity",
                "Foreman Activity",
                "Catergory",
                "Equipments",
                "Consultant/Others",
                "Diesel Rate",
                "Diesel Reciept",
                "Fuel Card",
                "Sub Contracted Hours",
                "Departments",
                "Support Office Timesheet",
                "Equipment Transfer",
                "Equipment Reciept",
                "Dynamic Reports"

            };
        }

        // Returns list of form names
        private List<String> GetReportsEquipments()
        {
            return new List<String>
            {
                "E01 - Equipment List",
                "E02 - Monthly Equipment Timesheet",
                "E04 - Equipment Cost Analysis - s...",
                "E05 - Equipment Cost Analysis - s...",
                "Disciplie List",
                "Activity List",
                "Sub Activity List",
                "Item List",
                "New Activity List",
                "Project Sub Activty List",
                "D01 - Prject Diesel Distrubution",
                "D02 - Monthly Diesel Distrubution",
                "D03 - Daily Disiel Distrubution",
                "D04 - Equipment Disiel Consumption",
                "D05 - Disiel Summary"

            };
        }

        // Returns list of form names
        private List<String> GetReportsCostControl()
        {
            return new List<string> {
            "C01 - Basic Project Information",
            "C02 - Cumalative Man-hours - sorted",
            "C03 - Cumalative Man-hours - sorted",
            "C04 - Man-hour Summary - sorted",
            "C05 - Man-hour Summary - sorted",
            "C06 - Man-hour Summary - sorted",
            "C07 - Man-hour Cost Summary - sorted",
            "C08 - Cumalative Man-hous Cost ...",
            "C09 - Man-hour Cost - Sorted by A...",
            "C10 - Company/Projects Monthly ...",
            "C20 - Subcontracted Man-hour Cost",
            "C11 - Budget vs Actual Manhours",
            "C12 - Designation Actual Unit Rate",
            "C13 - Activity Actual Unit Rate",
            "C14 - Company/Project Actual Work",
            "C15 - Weekly Productivity Report",
            "C16 - Support Office Timesheet",
            "C17 - Support Office Timesheet Su...",
            "C18 - Support Office Timesheet Su..."
            };
        }

        // Returns list of form names
        private List<String> GetReportsTimeKeeper()
        {
            return new List<string> {
            "T01 - Employee List",
            "T02 - Monthly Timesheet",
            "T13 - Subcontractor Timesheet",
            "T03 - Foreman Timesheet",
            "T04 - Missing Timesheet",
            "T04 - Absence Report for Day",
            "T06 - Employee at Site",
            "T07 - Absence Report for Month",
            "T08 - Daily Manpower by Location",
            "T09 - Daily Manpower by Designation",
            "T10 - Man-hour Summary (All Emp...",
            "T11 - Leave Register",
            "T12 - Employee Transfer History",
            "T13 - Offshore Worked Days",
            "T14 - Productive Incentive",
            "T15 - Productive Incentive Date",
            "T16 - Absent Days",
            "T17 - Overtime",
            "Employee Transfers",
            "Sick Leave Details",
            "Employee Transfer (To AJE)",
            "Employee Transfer (From AJE)",
            "Transfer Not Yet Recieved"
            };
        }

        // Support OFFICE 
        public List<RightModel> GetBlankSOUserRights()
        {
            RightModel obj;
            foreach (String s in this.GetFormsHR())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Forms - HR: " + s;
                rights.Add(obj);

            }

            foreach (String s in this.GetReportTimeKeepingSO())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Report - Time Keeper: " + s;
                rights.Add(obj);

            }

            foreach (String s in this.GetReportPersonal())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Report - Personal: " + s;
                rights.Add(obj);

            }

            foreach (String s in this.GetReportHumanResource())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = "Report - Human Resource: " + s;
                rights.Add(obj);

            }
            return rights;


        }
        private List<String> GetFormsHR()
        {
            return new List<String> {
                "Employee",
                "Salary Proposal",
                "Salary Approval",
                "Ticket Payment",            };
        }

        private List<String> GetReportTimeKeepingSO()
        {
            return new List<String> {
                "T01 - Employee List",
                "T02 - Monthly TimeSheet",
                "T13 - Subcontractor Timesheet",
                "T03 - Foreman Timesheet",
                "T04 - Missing Timesheet",
                "T04 - Absence Report for Day",
                "T06 - Employee at Site",
                "T07 - Absence Report for Month",
                "T08 - Daily Manpower by Location",
                "T09 - Daily Manpower by Designation",
                "T10 - Man-hour Summat (All Emp)",
                "T11 - Leave Register",
                "T12 - Employee Transfer History",
                "T13 - Offshore Worked Days",
                "T14 - Productive Incentive",
                "T15 - Productive Incentive Date",
                "T16 - Absent Days",
                "T17 - Overtime",
                "Employee Transfers",
                "Sick Leave Details",
                "Employee Transfer (To AJE)",
                "Employee Transfer (From AJE)",
                "Transfer Not Yet Recieved"
            };
      }

        private List<String> GetReportPersonal()
        {
            return new List<String> {
                "Expiry Of Documents",
                "Absconding List",
                "Employee To Rejoin"
            };
        }

        private List<String> GetReportHumanResource()
        {
            return new List<String> {
                "Employee Details",
                "T01 - Employee List",
                "Terminated Employee List",
                "T11 - Leave Register",
                "T09 - Daily Manpower by Desingation",
                "Designation List",
                "Promotion List",
                "Salary History",
                "Current Salary",
                "Air Ticket Payment",
            };
        }



    }
}