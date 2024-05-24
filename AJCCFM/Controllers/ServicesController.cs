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
    public class ServicesController : Controller
    {
        private IServices _Services;
        private IGroupRequest _GroupRequest;
        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        private string mailcontent;

        public ActionResult Index()
        {
           
         
            List<UserDetail> lADUser;

            EmployeeDetail empDetail = new EmployeeDetail();
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
                }
                return View(empDetail);
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
        public ActionResult NewServiceItem(string EmpCode)
        {

            if (EmpCode == "")
            {
                return Json(new { Response = "Select Employee No." });
            }
            string File = Server.MapPath("~/") + "\\App_Data\\Services.Json";

            using (StreamReader r = new StreamReader(File))
            {
                string json = r.ReadToEnd();
                var project = JsonConvert.DeserializeObject<List<AJESServices>>(json);
                ViewBag.Services = new SelectList(project, "Code", "Name");
            }

            string viewContent = ConvertViewToStringWithoutModel("_NewClaim", ViewBag.EmpID);
            return Json(new { PartialView = viewContent, Response = "" });
            // return PartialView("_NewClaim");
        }



        [HttpPost]
        public async Task<ActionResult>  SubmitRequest(EmployeeDetail model,string forwardto,string forwardName)
        {


            if (model.EmpCode == null)
            {
                return RedirectToAction("Index");
            }

                ShoppingCartId = GetCartId();
                string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.EmpCode);
                string SubmittToAddress = AJESActiveDirectoryInterface.AJESAD.GetEmailAddress(forwardto);

          
          
                string[] Name = forwardName.Split('-');

                _Services = new AjesServices();

                string message = _Services.SubmitServiceRequest(model, ShoppingCartId, forwardto, EmpEmailAddress, SubmittToAddress);
                if (!message.Equals(""))
                {
                   return RedirectToAction("Index");
                }
                var obj = _Services.GetRequestByGuid<ServiceRequestModel_SD_VPN>(ShoppingCartId);
                // Send Email.....
                string body;

                EmailManager VCTEmailService = new EmailManager();
                body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceRequest-DH.html");
                string ContentManager = GenerareBodyWithLink(obj);
                mailcontent = body.Replace("@ReqNo", ContentManager); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                VCTEmailService.ReceiverAddress = SubmittToAddress;
                await VCTEmailService.SendEmail();


            if (!string.IsNullOrEmpty(EmpEmailAddress))
            {
                //Send Email to requestor
                EmailManager VCTEmailServiceInit = new EmailManager();
                body = VCTEmailServiceInit.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceRequest-Init.html");
                mailcontent = body.Replace("@forwardto", Name[0]); //Replace Contenct...
                string Content = GenerareBody(obj);
                mailcontent = mailcontent.Replace("@ReqNo", Content); //Replace Contenct...
                VCTEmailServiceInit.Body = mailcontent;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                VCTEmailServiceInit.ReceiverAddress = EmpEmailAddress;
                VCTEmailServiceInit.ReceiverDisplayName = model.EmpName;
                await VCTEmailServiceInit.SendEmail();
            }
            else
            {
                EmailManager VCTEmailServiceInit = new EmailManager();
               
                VCTEmailServiceInit.Body = "Update Employee No In Active Direcotry:" + model.EmpCode;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                VCTEmailServiceInit.ReceiverAddress = "ithelpdesk@ajes.ae";
                VCTEmailServiceInit.ReceiverDisplayName = "IT HELP DESK";
                await VCTEmailServiceInit.SendEmail();
            }

                HttpContext.Session.Remove(CartSessionKey);
                return RedirectToAction("Index", "Dashboard");
            
           
            
        }


        [HttpPost]
        public ActionResult SaveService(CartService ClaimData)
        {
            string Error = "";
            _Services = new AjesServices();
          
            ShoppingCartId = GetCartId();


            if (_Services.IsServiceAlreadySelected(ClaimData.ServiceName, ShoppingCartId) == true)
            {
                return Json(new { Response = "Service Already selected" });
            }

            ClaimData.CartId = ShoppingCartId;
            Error = _Services.SaveService(ClaimData);
            var ViewModel = _Services.GetServiceDetails<CartService>(ClaimData.CartId);
            string viewContent = ConvertViewToString("_ListServiceDetail", ViewModel);
            return Json(new { PartialView = viewContent, Response = Error });


        }


        public async Task<ActionResult> ApproveRequest(ServiceRequestModel_SD_VPN model,string remarks)
        {
            _Services = new AjesServices();
            _GroupRequest = new GroupRequestService();
            if (model.Status==0) // PM Level
            {
                model.Status = 1;
                model.Submitedto = System.Configuration.ConfigurationManager.AppSettings.Get("ForwardToIT");
            }
            else 
            {
                model.Status = -1;
               
            }

            var affectedRows = await _Services.SubmitForApproval(model, remarks);

            //Send Email to 
            string body;
            if (model.Status == 1)
            {
                string mGuid = Guid.NewGuid().ToString();
                string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
                string Link = url + "/Services/ShowRequest?token=" + mGuid + "&Service=" + model.ServiceCode+ "&Mode=E";
                await _GroupRequest.LogEmail(model.ID, mGuid, "S");
                EmailManager VCTEmailService = new EmailManager();
                body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceRequest-ITManager.html");
                mailcontent = body.Replace("@pwdchangelink", Link); //Replace Contenct...
                mailcontent = mailcontent.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                mailcontent = mailcontent.Replace("@ServiceName", model.ServiceName); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                VCTEmailService.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("ITManagerEmail"); 
                await VCTEmailService.SendEmail();


                if (!string.IsNullOrEmpty(model.Email))
                {

                    //Send Email to User .... 
                    EmailManager VCTEmailServiceUser = new EmailManager();
                    body = VCTEmailServiceUser.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceStatusUpdate-Approved.html");
                    mailcontent = body.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                    VCTEmailServiceUser.Body = mailcontent;
                    VCTEmailServiceUser.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                    VCTEmailServiceUser.ReceiverAddress = model.Email;
                    await VCTEmailServiceUser.SendEmail();
                }
            }
            else if  (model.Status == -1) //Approved //Send Email to User  and IT HELP DESK AS WELL 
                {

                if (!string.IsNullOrEmpty(model.Email))
                {
                    EmailManager VCTEmailService = new EmailManager();
                    body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceStatusUpdate-Processed.html");
                    mailcontent = body.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                    VCTEmailService.Body = mailcontent;
                    VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                    VCTEmailService.ReceiverAddress = model.Email;
                    await VCTEmailService.SendEmail();
                }

                    EmailManager VCTEmailServiceIT = new EmailManager();
                    body = VCTEmailServiceIT.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceRequestStatus-Approved(IT).html");
                    mailcontent = body.Replace("@ReqNo", model.RefNo); //Replace Contenct...
                    VCTEmailServiceIT.Body = mailcontent;
                    VCTEmailServiceIT.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectService");
                    VCTEmailServiceIT.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("groupdistribution"); 
                    await VCTEmailServiceIT.SendEmail();



            }
            return RedirectToAction("Index", "Dashboard");
        }



        public async Task<ActionResult> RejectForm(int ID, string Remarks,string ServiceCode)
        {
            string returnURL = "";

            _Services = new AjesServices();

            var obj =  _Services.ViewRequest<ServiceRequestModel_SD_VPN>(ID);
            if (ID > 0)
            {
                var affectedRows = await _Services.RejectForm(ID, Remarks, ServiceCode);
                EmailManager VCTEmailService = new EmailManager();
                string body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewServiceStatusUpdate-Rejected.html");
                mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                mailcontent = mailcontent.Replace("@Reason", Remarks); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectRejectService");
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



        public async Task<ActionResult> ArchiveFolderReturnPartialView(string AssetsNo,int RecordID)
        {


            _Services = new AjesServices();

            var response = await _Services.ArchiveRecord(AssetsNo,RecordID);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };





        }
        public async Task<ActionResult> DeleteLineItemReturnPartialView(int RecordID)
        {


            _Services = new AjesServices();

            var response = await _Services.DeleteDetailRecord(RecordID);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };





        }


        public ActionResult GetServiceDetails()
        {
            _Services = new AjesServices();
          
            ShoppingCartId = GetCartId();

            var obj = _Services.GetServiceDetails<CartService>(ShoppingCartId);
            return PartialView("_ListServiceDetail", obj);
        }



        public ActionResult ViewRequest(int TransactionID, string Service,string Mode)
        {

            Dictionary<string, string> lstAccessType = new Dictionary<string, string>();
            dynamic  obj=null ;
            _Services = new AjesServices();
            if (Service == "SD_VPN")
            {
                lstAccessType.Add("Remote Desktop", "Remote Desktop");
                lstAccessType.Add("Direct Network Access", "Direct Network Access");
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }
            else if (Service == "SD_IA")
            {
                lstAccessType.Add("12-1", "12-1");
                lstAccessType.Add("1-2", "1-2");
                lstAccessType.Add("Fulltime", "Fulltime");
                
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }

            else if (Service == "SD_USBACC")
            {
                lstAccessType.Add("Temporary", "Temporary");
                lstAccessType.Add("Permanent", "Permanent");
                
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }

            else if (Service == "SD_SUA")
            {
                lstAccessType.Add("Meeting Zoom", "Meeting Zoom");
                lstAccessType.Add("Meeting Team", "Meeting Team");
                lstAccessType.Add("Online Storage", "Online Storage");
                lstAccessType.Add("Govt Portal", "Govt Portal");
                lstAccessType.Add("Client Portal", "Client Portal");
                lstAccessType.Add("Others", "Others");
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }

            else if (Service == "SD_IRL")
            {
                lstAccessType.Add("30", "30");
                lstAccessType.Add("Others", "Others");
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }
            else if (Service == "SD_PC")
            {
                lstAccessType.Add("Laptop", "Laptop");
                lstAccessType.Add("Desktop", "Desktop");
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }
            else if (Service == "SD_Monitor")
            {
                lstAccessType.Add("Monitor Screen Size 19", "Monitor Screen Size 19");
                lstAccessType.Add("Monitor Screen Size 22", "Monitor Screen Size 22");
                lstAccessType.Add("Others", "Others");
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }

            else if (Service == "SD_Keyboard")
            {
                lstAccessType.Add("Wired", "Wired");
                lstAccessType.Add("Wireless", "Wireless");
                
                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }

            else if (Service == "SD_Mouse")
            {
                lstAccessType.Add("Wired", "Wired");
                lstAccessType.Add("Wireless", "Wireless");

                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }
            else if (Service == "SD_USB")
            {
              
                lstAccessType.Add("USB Drive Size 8 GB", "USB Drive Size 8 GB");
                lstAccessType.Add("USB Drive Size 16 GB", "USB Drive Size 16 GB");
                lstAccessType.Add("Others", "Others");

                ViewBag.AccessType = new SelectList(lstAccessType, "key", "Value");
            }

            obj = _Services.ViewRequest<ServiceRequestModel_SD_VPN>(TransactionID);
           ViewBag.Mode = Mode;
            
          
            return View(obj);
        }

        public async Task<ActionResult> ShowRequest(string token, string Service, string Mode)
        {

            _GroupRequest = new GroupRequestService();
            _Services = new AjesServices();
            int TransactionID = await _GroupRequest.GetToken(token, "S");

            if (TransactionID > 0)
            {
                return RedirectToAction("ViewRequest", new { TransactionID= TransactionID,Service = Service, Mode = Mode });
               
            }
            else
            {
                ViewBag.Token = token;

                return View("Processed");
            }
        }
        public string GetCartId()
        {
            if (HttpContext.Session[CartSessionKey] == null)
            {
                HttpContext.Session[CartSessionKey] = Guid.NewGuid().ToString();

                //  System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
            }

            return HttpContext.Session[CartSessionKey].ToString();
        }

        private string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (System.IO.StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }
        private string ConvertViewToStringWithoutModel(string viewName, object model)
        {
            //  ViewData.Model = model;
            using (System.IO.StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }




        }



        public ActionResult MyService()
        {
            _Services = new AjesServices();

            var obj = _Services.MyService<ServicePending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return PartialView("MyService", obj);
        }

        public ActionResult AllService()
        {
            _Services = new AjesServices();

            var obj = _Services.AllService<ServicePending>();

            return PartialView("AllService", obj);
        }


        private string GenerareBody(IEnumerable<ServiceRequestModel_SD_VPN> obj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<table cellpadding='5' cellspacing='2' style='border:1px solid black'>");
            foreach (var item in obj)
            {
                sb.Append("<tr><td style='border: 1px solid black'>" + item.ServiceName + "   " + item.RefNo + " has been initiated.   </td></tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        private string GenerareBodyWithLink(IEnumerable<ServiceRequestModel_SD_VPN> obj)
        {
            StringBuilder sb = new StringBuilder();
            _GroupRequest = new GroupRequestService();
            sb.Append("<table cellpadding='5' cellspacing='2' style='border:1px solid black'>");
            foreach (var item in obj)
            {
                string mGuid = Guid.NewGuid().ToString();
                string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
                 _GroupRequest.LogEmail(item.ID, mGuid, "S");

                string Link= url + "/Services/ShowRequest?token=" + mGuid + "&Service=" + item.ServiceCode + "&Mode=E"; 

                sb.Append("<tr><td style='border: 1px solid black'>" + item.ServiceName + "  <a href=" + Link + " > " + item.RefNo + " </a> has been initiated.   </td></tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        
        public async Task<FileResult> GeneratePDF(int RecordID,string ServiceCode)
        {
            _Services = new AjesServices();

            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");

            url = url + "/content/images/logo.png";

            var obj = await _Services.GeneratePDF<ServiceRequestReport>(RecordID, ServiceCode);

            StringBuilder sb = new StringBuilder();
            sb.Append("<header class='clearfix'>");
            sb.Append("<img src='" + url + "' > ");
            sb.Append("<br>");
            sb.Append("<br>");
            
            sb.Append("<h1>Service Request -  " + obj[0].ServiceName + " </h1>");
            sb.Append("<br>");
            sb.Append("<h1>" + obj[0].RefNo + " </h1>");
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
                sb.Append("<td>Access Type</td>");
                sb.Append("<td colspan=3>" + obj[0].AccessType + "</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Justification</td>");
                sb.Append("<td colspan=3>" + obj[0].Remarks + "</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Others</td>");
                sb.Append("<td colspan=3>" + obj[0].Others + "</td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td>Path / Application Name</td>");
                sb.Append("<td colspan=3>" + obj[0].Path + "</td>");
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
                return File(bytes, "application/pdf", "ServiceRequest-" + obj[0].RefNo + ".PDF") ;
            }

        }


     

    }


}