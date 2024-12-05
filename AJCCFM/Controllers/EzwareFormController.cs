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
        RightModel objMain=null;
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

            string Link = url + "/EzwareForm/ShowRequest?token=" + mGuid + "&Mode=E";

            EmailManager VCTEmailService = new EmailManager();
            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewEzwareRequest-DH.html");
            mailcontent = body.Replace("@ReqNo", result.RequestNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@urllink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectEzWare");
            VCTEmailService.ReceiverAddress = SubmittToAddress;
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

        public async Task<FileResult> GeneratePDF(int RecordID)
        {
            _EzwareProjectServices = new EzwareProjectService();
            StringBuilder sb = new StringBuilder();
           
            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");

            url = url + "/content/images/logo.png";

            var obj = await _EzwareProjectServices.GeneratePDF<rptReportModel>(RecordID);

          

          
            sb.Append("<img src='" + url + "' > ");
            sb.Append("<br>");
     

            sb.Append("<h3> Al Jaber Energy Services</h3>");
            sb.Append("<br>");
            sb.Append("<h3> EzBusiness User Rights <h3>");
            sb.Append("<br>");
            sb.Append("<h3> RefNo:" + obj[0].RefNo + " </h3>");
            sb.Append("<br>");
            sb.Append("<table border='1'>");
            sb.Append("<tr>");
            sb.Append("<td> Employee Code </td>");
            sb.Append("<td>" + obj[0].EmpCode + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td> Employee Name </td>");
            sb.Append("<td>" + obj[0].Name + "</td>");
            sb.Append("<td> Designation </td>");
            sb.Append("<td>" + obj[0].Position + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td> Project </td>");
            sb.Append("<td>" + obj[0].AssignedProject + "</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("<br>");
            sb.Append("<table border='1'>");

            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.Append("<th width='35%'>Form Name</th>");
            sb.Append("<th  > All </ th >");
            sb.Append("<th >Create</th>");
            sb.Append("<th  > View </ th >");
            sb.Append("<th >Edit</th>");
            sb.Append("<th > Delete </ th >");
            sb.Append("<th >Print</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");

            for (int i = 0; i <= obj.Count - 1; i++)
            {
                   
                sb.Append("<tr>");
                if (obj[i].Parent == 1)
                {
                    sb.Append("<td style=font-size:large;font-style:italic;font-weight:bold colspan=7>" + obj[i].form_name + "</td>");
                }
                else
                {

                    sb.Append("<td style=font-size:small;font-weight:bold>" + obj[i].form_name + "</td>");
                    sb.Append("<td style=font-size:large;font-weight:bold>" + ((bool)obj[i].All ? "Y" : "") + "</td>");
                    sb.Append("<td style=font-size:large;font-weight:bold>" + ((bool)obj[i].Create ? "Y" : "") + "</td>");
                    sb.Append("<td style=font-size:large;font-weight:bold>" + ((bool)obj[i].View ? "Y" : "") + "</td>");
                    sb.Append("<td style=font-size:large;font-weight:bold>" + ((bool)obj[i].Edit ? "Y" : "") + "</td>");
                    sb.Append("<td style=font-size:large;font-weight:bold>" + ((bool)obj[i].Delete ? "Y" : "") + "</td>");
                    sb.Append("<td style=font-size:large;font-weight:bold>" + ((bool)obj[i].Print ? "Y" : "") + "</td>");
                }
                    sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("<br>");

            sb.Append("<table border='1'>");

            sb.Append("<tr>");
            sb.Append("<td >Prepared By</th>");
            sb.Append("<td >Approved By</th>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>" + obj[0].Createdby +  "</th>");
            sb.Append("<td>" + obj[0].ApprovedBy + "</th>");
            sb.Append("</tr>");

            sb.Append("<tr>");
            sb.Append("<td>" + obj[0].CreatedOn.ToString("dd/MM/yyyy hh:mm:ss") + "</th>");
            sb.Append("<td>" + obj[0].ApprovedOn.ToString("dd/MM/yyyy hh:mm:ss") + "</th>");
            sb.Append("</tr>");

            sb.Append("</table>");

            sb.Append("<br>");
            sb.Append("Document was created on a computer and is valid without the signature and seal.");
            
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
                return File(bytes, "application/pdf", "UserRights-" + obj[0].RefNo + ".PDF");
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
            objMain = new RightModel();
            objMain.form_name = "Forms TimeKeeper";
            objMain.Parent = 1;
            rights.Add(objMain);
            foreach (String s in this.GetFormsTimeKeeper())
            {
             
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name =  s;
                rights.Add(obj);
               
            }
        
            objMain = new RightModel();
            objMain.form_name = " Forms Cost Control";
            objMain.Parent = 1;
            rights.Add(objMain);
            foreach (String s in this.GetFormsCostControl())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = s;
                rights.Add(obj);
                
            }
            objMain = new RightModel();
            objMain.form_name = " Reports TimeKeeper";
            objMain.Parent = 1;
            rights.Add(objMain);

            foreach (String s in this.GetReportsTimeKeeper())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name =  s;
                rights.Add(obj);
            }

            objMain = new RightModel();
            objMain.form_name = " Reports Cost Control";
            objMain.Parent = 1;
            rights.Add(objMain);
            foreach (String s in this.GetReportsCostControl())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name =  s;
                rights.Add(obj);
            }


            objMain = new RightModel();
            objMain.form_name = " Reports Equipment";
            objMain.Parent = 1;
            rights.Add(objMain);
            foreach (String s in this.GetReportsEquipments())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name =  s;
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
                "Employee Transfer In",
                "Transfer To Al Jaber EST",
                "Transfer From Al Jaber EST",
                "Transfer To Qatar",
                "Transfer From Qatar",
                "Employee Dept. Transfer",
                "Foreman Change",
                "Foreman Transfer",
                "Budgets",
                "Productivity",
                "Offshore Master"
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
                "E04 - Equipment Cost Analysis - sorted by Activity & Location",
                "E05 - Equipment Cost Analysis - sorted by Equipment & Location",
                "E06 - Equipment Cost Analysis - sorted by Equipment & SubLocation",
                "Disciplie List",
                "Activity List",
                "Sub Activity List",
                "Item List",
                "New Activity List",
                "Project Sub Activty List",
                "D01 - Project Diesel Delivery",
                "D02 - Monthly Diesel Distribution",
                "D03 - Daily Diesel Distribution",
                "D04 - Equipment Diesel Consumption - sorted by Activity & Location",
                "D05 - Diesel Summary"

            };
        }

        // Returns list of form names
        private List<String> GetReportsCostControl()
        {
            return new List<string> {
            "C01 - Basic Project Information",
            "C02 - Cumalative Man-hours - sorted by Designation & Location",
            "C03 - Cumalative Man-hours - sorted by Activity & Location",
            "C04 - Man-hour Summary - sorted by Designation & Location",
            "C05 - Man-hour Summary - sorted by Activity & Location",
            "C06 - Man-hour Summary - sorted by Activity,Location & Sub Location",
            "C07 - Man-hour Cost Summary - sorted by Activity & Location",
            "C08 - Cumalative Man-hous Cost sorted by Activity & Location",
            "C09 - Man-hour Cost - sorted by Activity,Location & Sub Location",
            "C10 - Company/Projects Monthly Cost Analysis",
            "C20 - Subcontracted Man-hour Cost Summary",
            "C11 - Budget vs Actual Manhours",
            "C12 - Designation Actual Unit Rate",
            "C13 - Activity Actual Unit Rate",
            "C14 - Company/Project Actual Work Force",
            "C15 - Weekly Productivity Report",
            "C16 - Support Office Timesheet",
            "C17 - Support Office Timesheet Summary...",
            "C18 - Support Office Timesheet Summary (Dept Wise)"
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
            "T15 - Productive Incentive Date Wise",
            "T16 - Absent Days",
            "T17 - Overtime",
            "Employee Transfers",
            "Sick Leave Details",
            "Employee Transfer (To AJE)",
            "Employee Transfer (From AJE)",
            "Transfer Not Yet Recieved",
            "Offshore Register"
            };
        }

        // Support OFFICE 
        public List<RightModel> GetBlankSOUserRights()
        {
            RightModel obj;
            RightModel objMain;
            objMain = new RightModel();
            objMain.form_name = "Forms HRM";
            objMain.Parent = 1;
            rights.Add(objMain);
            foreach (String s in this.GetFormsHR())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = s;
                rights.Add(obj);

            }

           
            objMain = new RightModel();
            objMain.form_name = "Report - Time Keeper";
            objMain.Parent = 1;
            rights.Add(objMain);
            foreach (String s in this.GetReportTimeKeepingSO())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name =  s;
                rights.Add(obj);

            }
            objMain = new RightModel();
            objMain.form_name = "Report - Personal";
            objMain.Parent = 1;
            rights.Add(objMain);

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

            objMain = new RightModel();
            objMain.form_name = "Report - Human Resource";
            objMain.Parent = 1;
            rights.Add(objMain);

            foreach (String s in this.GetReportHumanResource())
            {
                obj = new RightModel();
                obj.View = false;
                obj.Delete = false;
                obj.Create = false;
                obj.Print = false;
                obj.Edit = false;
                obj.All = false;
                obj.form_name = s;
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
                "T15 - Productive Incentive Date Wise",
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