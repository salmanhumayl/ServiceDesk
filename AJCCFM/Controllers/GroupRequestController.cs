using AJCCFM.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Core.Domain;
using System.Web.Mvc;

using Services.Helper;

using System.DirectoryServices;

using Services.GroupRequest;

using AJCCFM.Core;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System.Text;
using AJESeForm.Models;
using AJCCFM.Models.GroupRequest;
using static AJCCFM.Models.GroupRequest.SharefolderModel;
using Newtonsoft.Json;
using AJCCFM.Models.Project;
using AJCCFM.Action_Filters.ErroHandler;

namespace AJCCFM.Controllers
{
   // [RBAC]
    [AjeErrorHandler]
   //[UserSessionFilter]
    public class GroupRequestController : Controller
    {
        // GET: Claim
     
       
        private IGroupRequest _GroupRequest; 
            
    
        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        private string mailcontent;
     

        public List<Folder> UserAssignGroup(string EmpCode)
        {
         
            Folder obj;
            List<Folder> lFolderDetail = new List<Folder>();


            string user = AJESActiveDirectoryInterface.AJESAD.GetEmpLogin(EmpCode);

            if (!user.Equals(""))
            {
                string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
                DirectorySearcher search = new DirectorySearcher();
                DirectoryEntry entry = new DirectoryEntry(sPath);
                search.SearchRoot = entry;
                //search.Filter = String.Format("(sAMAccountName={0})", "smazhar");

                search.SearchScope = SearchScope.Subtree;
                search.Filter = "(&(objectClass=user)(objectCategory=person)(SAMAccountName=" + user + "))"; //GROUP LIST 
                search.PropertiesToLoad.Add("memberOf"); // attributes 
                SearchResult result = search.FindOne();
                if (result != null)
                {
                    int groupCount = result.Properties["memberOf"].Count;
                    int counter = 0;
                    int sno = 1;

                    while (counter < groupCount)
                    {

                        string str = result.Properties["memberOf"][counter].ToString();


                        if (str.Length >= 6)
                        {
                            str = AJESActiveDirectoryInterface.AJESAD.Mid(str, str.IndexOf("=") + 2, str.IndexOf(",") - 3);

                        }

                        string[] grpdetails = AJESActiveDirectoryInterface.AJESAD.GetADUserGroupInfo(str); //get group description ...... 
                        if (grpdetails != null)
                        {
                            if (grpdetails[0] != "")
                            {

                                obj = new Folder();
                                obj.srno = sno;
                                obj.FolderName = grpdetails[0] + " " + ">>" + " " + grpdetails[1];
                                //  obj.FolderName = grpdetails[0];
                                // obj.FolderDetail = grpdetails[1];
                                lFolderDetail.Add(obj);
                                sno += 1;
                            }

                        }
                        counter += 1;
                    }

                }
            }
            return lFolderDetail;

        }
        public List<Folder> Groups(string ProjectCode="")
        {


            string GroupKey="";
            string description="";
            string ManagedBy = "";

         //   Folder objTest;
         //   List<Folder> lTestFolderDetail = new List<Folder>();
            Folder obj;
            List<Folder> lFolderDetail = new List<Folder>();

            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectoryEntry entry = new DirectoryEntry(sPath);
            DirectorySearcher search = new DirectorySearcher(entry);

            SortOption so = new SortOption();
            so.Direction = SortDirection.Ascending;

            so.PropertyName = "cn";
            search.Sort = so;

            if (User.IsInRole("HelpDesk"))
            {
                search.Filter = "(&(objectCategory=group))";
            }
            else if (ProjectCode == "")
            {
                search.Filter = "(&(objectCategory=group))";
            }
            else if (ProjectCode.Trim() == "SO")
            {
                search.Filter = "(&(objectCategory=group)((!extensionAttribute7=*))(!(extensionAttribute5=System-Group)))";
            }
            else
            {
                search.Filter = "(&(objectCategory=group)(extensionAttribute7=" + ProjectCode.Trim() + ")(!(extensionAttribute5=System-Group)))";

            }
            search.SearchScope = SearchScope.Subtree;

            search.PropertiesToLoad.Add("cn");
            search.PropertiesToLoad.Add("description");
            search.PropertiesToLoad.Add("ManagedBy");

            SearchResultCollection searchResults;
            search.PageSize = 1500;
            searchResults = search.FindAll();
          
            foreach (SearchResult result in searchResults)
            {
                try
                {
                     GroupKey = result.Properties["cn"][0].ToString();
                     description = result.Properties["description"][0].ToString();

                     ManagedBy = result.Properties["ManagedBy"][0].ToString();
                     ManagedBy = AJESActiveDirectoryInterface.AJESAD.Mid(ManagedBy, ManagedBy.IndexOf("=") + 2, ManagedBy.IndexOf(",") - 3);
                     obj = new Folder();
                  
                    obj.FolderName = GroupKey + " " +  ">>" +  " " +  description;
                    obj.FolderDetail = description;
                    obj.ProcessOwner = ManagedBy;
                    lFolderDetail.Add(obj);
                   
                }
                  

                catch (Exception e)
                {

                   
                   
                }
               

            }
            return lFolderDetail;
           



        }


        public List<Folder> GetProcessOwner()
        {

            Folder obj;
            List<Folder> lFolderDetail = new List<Folder>();

            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectoryEntry entry = new DirectoryEntry(sPath);
            DirectorySearcher search = new DirectorySearcher(entry);

            SortOption so = new SortOption();
            so.Direction = SortDirection.Ascending;
            so.PropertyName = "cn";
            search.Sort = so;

            
                search.Filter = "(&(objectCategory=group))";
            
          
            search.SearchScope = SearchScope.Subtree;

           
            search.PropertiesToLoad.Add("ManagedBy");

            SearchResultCollection searchResults;
            searchResults = search.FindAll();

            foreach (SearchResult result in searchResults)
            {
                try
                {
                   

                    string ManagedBy = result.Properties["ManagedBy"][0].ToString();
                    ManagedBy = AJESActiveDirectoryInterface.AJESAD.Mid(ManagedBy, ManagedBy.IndexOf("=") + 2, ManagedBy.IndexOf(",") - 3);
                    obj = new Folder();

                
                    obj.ProcessOwner = ManagedBy;
                    lFolderDetail.Add(obj);
                }


                catch (Exception e)
                {

                }


            }
            return lFolderDetail;




        }

        [HttpPost]
        public async Task<ActionResult> SubmitRequest(EmployeeDetail model)
        {
             ShoppingCartId = GetCartId();
            _GroupRequest = new GroupRequestService();
            string message=_GroupRequest.SubmitGroupRequest(model,GetCartId());
            if (!message.Equals(""))
            {
                return RedirectToAction("Index");
            }
            // Send Email to Process Owners.....
            var obj = _GroupRequest.GetRequestByGuid<GroupRequest>(ShoppingCartId);
            string body;
            foreach (var item in obj)
            {
                string mGuid = Guid.NewGuid().ToString();

                await _GroupRequest.LogEmail(item.ID, mGuid,"F");

                string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
                string Link = url + "/GroupRequest/ShowRequest?token=" + mGuid;
                EmailManager VCTEmailService = new EmailManager();
               
                body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewGroupRequest-POnwer.html");
                mailcontent = body.Replace("@ReqNo", item.RefNo); //Replace Contenct...
                mailcontent = mailcontent.Replace("@pwdchangelink", Link); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("Subject");
                VCTEmailService.ReceiverAddress = item.ProcessOwnerEmail;
                VCTEmailService.ReceiverDisplayName = item.ProcessOwner;
                await VCTEmailService.SendEmail();
            }

            if (!string.IsNullOrEmpty(obj.ElementAt(0).Email))
            {
                //Send Email to requestor
                EmailManager VCTEmailServiceInit = new EmailManager();
                body = VCTEmailServiceInit.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewGroupRequest-Init.html");
                string Content = GenerareBody(obj);
                mailcontent = body.Replace("@ReqNo", Content); //Replace Contenct...
                VCTEmailServiceInit.Body = mailcontent;
                VCTEmailServiceInit.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("Subject");
                VCTEmailServiceInit.ReceiverAddress = obj.ElementAt(0).Email;
                VCTEmailServiceInit.ReceiverDisplayName = obj.ElementAt(0).Name;
                await VCTEmailServiceInit.SendEmail();
            }

            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index","Dashboard");
        }
        [HttpPost]
        public ActionResult SaveClaim(Cart ClaimData)
        {
            string Error = "";
            _GroupRequest = new GroupRequestService();
            ProcesOwnerDetail processOwnerDetail=new ProcesOwnerDetail();
            ShoppingCartId = GetCartId();

            if (ClaimData.ProcessOwner == System.Configuration.ConfigurationManager.AppSettings.Get("ProcessOwnerIT"))
            {
                processOwnerDetail.ProcessOwnerEmail = System.Configuration.ConfigurationManager.AppSettings.Get("ProcessOwnerEmailIT"); 
                processOwnerDetail.ProcessOwnerLoginID = System.Configuration.ConfigurationManager.AppSettings.Get("ProcessOwnerLoginIDIT");

            }
            else
            {
                processOwnerDetail = AJESActiveDirectoryInterface.AJESAD.ProcessOwnerDetail(ClaimData.ProcessOwner);
                if (processOwnerDetail.ProcessOwnerEmail == null)
                {
                    return Json(new { Response = "ProcessOwner Email not defined...." });
                }
                if (processOwnerDetail.ProcessOwnerLoginID == null)
                {
                    return Json(new { Response = "ProcessOwner LoginID not defined...." });
                }
            }
            ClaimData.ProcessOwnerEmail = processOwnerDetail.ProcessOwnerEmail;
            ClaimData.ProcessOwnerLoginID = processOwnerDetail.ProcessOwnerLoginID;
         

            if (_GroupRequest.IsGroupAlreadySelected(ClaimData.Group_Name, ShoppingCartId) ==true)
            {
                return Json(new { Response = "Group Already selected" });
            }

         


            ClaimData.CartId = ShoppingCartId;
            Error = _GroupRequest.SaveClaim(ClaimData);
                var ViewModel = _GroupRequest.GetClaimDetails<Cart>(ClaimData.CartId);
                string viewContent = ConvertViewToString("_ListClaimDetail", ViewModel);
                return Json(new { PartialView = viewContent, Response = Error });
            
       
        }
        public ActionResult Index()
        {
            EmployeeDetail empDetail=new EmployeeDetail();
            if (TempData["EmpCode"] != null)
            {
               empDetail = Common.GetEmpData<EmployeeDetail>(TempData["EmpCode"].ToString());
               
                ViewBag.EmpCode = TempData["EmpCode"].ToString();
                    return View(empDetail);
            }

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


        public async Task<ActionResult> ShowRequest(string token)
        {
            
            _GroupRequest = new GroupRequestService();

            int TransactionID=await _GroupRequest.GetToken(token,"F");

            if (TransactionID > 0)
            {
                var obj = _GroupRequest.ViewRequest<GroupRequestModel>(TransactionID);
                return View("ViewRequest", obj);
            }
            else
            {
                ViewBag.Token = token;
                
                return View("Processed");
            }
        }

        public ActionResult ViewRequest(int TransactionID,string mode)
        {
            ViewBag.Mode = mode;
            _GroupRequest = new GroupRequestService();
            var obj = _GroupRequest.ViewRequest<GroupRequestModel>(TransactionID);
            return View(obj);
        }


        private string GenerareBody(IEnumerable<GroupRequest> obj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<table cellpadding='5' cellspacing='2' style='border:1px solid black'>");
            foreach (var item in obj)
            {
                sb.Append("<tr><td style='border: 1px solid black'>" + item.RefNo + " has been initiated and Sent to  " + item.ProcessOwner +" </td></tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }
      
        


        public ActionResult GetClaimDetails()
        {
            _GroupRequest = new GroupRequestService();
            ShoppingCartId = GetCartId();

            var obj = _GroupRequest.GetClaimDetails<Cart>(ShoppingCartId);
            return PartialView("_ListClaimDetail", obj);
        }
       [HttpPost]
        public ActionResult NewClaimLineItem(string EmpCode)
        {
         
            if (EmpCode =="")
            {
                return Json(new { Response = "Select Employee No." });
            }
            string File = Server.MapPath("~/") + "\\App_Data\\Project.Json";

            using (StreamReader r = new StreamReader(File))
            {
                string json = r.ReadToEnd();
                var project = JsonConvert.DeserializeObject<List<Project>>(json);
                ViewBag.Projects = new SelectList(project, "Code", "Name");
            }
          
            string viewContent = ConvertViewToStringWithoutModel("_NewClaim", ViewBag.EmpID);
            return Json(new { PartialView = viewContent, Response="" });
           // return PartialView("_NewClaim");
        }


        public ActionResult DelegateRequest(int RecordID,string RefNo)
        {
            _GroupRequest = new GroupRequestService();
            var detail = _GroupRequest.ViewRequest<GroupRequestModel>(RecordID);
            if (detail.Group_Name== "OTHERS")
            {
                   return Json(new { Response = "Delegate Can only be done once Folder Selection is complete" });
            }

            var lstGroup = GetProcessOwner();
            var obj = lstGroup.Select(p => p.ProcessOwner).Distinct();
            List<SelectListItem> mySkills= new List<SelectListItem>();
            ViewBag.RecordID = RecordID;
            ViewBag.RefNo = RefNo;
           foreach (var item in obj)
            {

                mySkills.Add(new SelectListItem
                {
                    Text = item
                });

            }
            ViewBag.ProcessOwner = mySkills;
            string viewContent = ConvertViewToStringWithoutModel("_DelegateRequest", 1);
            return Json(new { PartialView = viewContent, Response = "" });
            
        }
        [HttpPost]
        public async Task<ActionResult> PostDelegateRequest(int ID,string ProcessOwner,string Remarks,string RefNo)
        {
            string body = "";
            ProcesOwnerDetail processOwnerDetail = new ProcesOwnerDetail();
            _GroupRequest = new GroupRequestService();
            processOwnerDetail = AJESActiveDirectoryInterface.AJESAD.ProcessOwnerDetail(ProcessOwner);

            //if (processOwnerDetail.ProcessOwnerEmail == null)
            //{
            //    return Json(new { Response = "ProcessOwner Email not defined...." });
            //}
            //if (processOwnerDetail.ProcessOwnerLoginID == null)
            //{
            //    return Json(new { Response = "ProcessOwner LoginID not defined...." });
            //}


            await _GroupRequest.PostDelegateRequest(ID, processOwnerDetail.ProcessOwnerLoginID, Remarks);


            string mGuid = Guid.NewGuid().ToString();

            await _GroupRequest.LogEmailCancel(ID, "F");
            await _GroupRequest.LogEmail(ID, mGuid, "F");

            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            string Link = url + "/GroupRequest/ShowRequest?token=" + mGuid;
            EmailManager VCTEmailService = new EmailManager();

            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewGroupRequest-POnwer.html");
            mailcontent = body.Replace("@ReqNo", RefNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@pwdchangelink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("Subject");
            VCTEmailService.ReceiverAddress = processOwnerDetail.ProcessOwnerEmail;
            VCTEmailService.ReceiverDisplayName = ProcessOwner;
            await VCTEmailService.SendEmail();


            return Json(new { Result = Url.Action("ProjectFolderActivity", "Dashboard") });
        }

            [HttpGet]
        public ActionResult GetFolders(string ProjectID,string EmpCode)
        {
           
             var  lstUserGroup = UserAssignGroup(EmpCode);
             var lstGroup = Groups(ProjectID);

               var FilterList = lstGroup.Except(lstUserGroup);
        
                IEnumerable<Folder> ItemNotProcess =
                 lstGroup.Except(lstUserGroup, new FolderComparer());

                Folder obj = new Folder();
                obj.FolderName = "OTHERS";
                obj.ProcessOwner = System.Configuration.ConfigurationManager.AppSettings.Get("ProcessOwnerIT");
                var newlist=ItemNotProcess.ToList();

               newlist.Add(obj);

            //    ViewBag.Groups = new SelectList(newlist, "ProcessOwner", "FolderName");

         //   StringBuilder sb = new StringBuilder();

         //   foreach (var item in newlist)
         //   {
         //       sb.Append(item.FolderName);
           //     sb.AppendLine();

          //  }

            return new JsonResult
            {
                Data = newlist,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
          
        }
        
        public ActionResult ReviewRequest(int ID,string ISOthers,string RequiredAccess,string Remarks)
        {
            _GroupRequest = new GroupRequestService();
            if (ISOthers == "OTHERS")
            {

                var lstGroup = Groups();
                ViewBag.Groups = new SelectList(lstGroup, "ProcessOwner", "FolderName");
              
            }
            ViewBag.ISOthers = ISOthers;
            ViewBag.RequiredAccess = RequiredAccess;
            ViewBag.Remarks = Remarks;
            return PartialView("_ReviewRequest");
        }

        public async Task<ActionResult> DeleteLineItemReturnPartialView(int RecordID)
        {

           
            _GroupRequest = new GroupRequestService();

            var response = await _GroupRequest.DeleteDetailRecord(RecordID);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };





        }

        public async Task<ActionResult> ArchiveFolderReturnPartialView(string AssetsNo,int RecordID)
        {


            _GroupRequest = new GroupRequestService();

            var response = await _GroupRequest.ArchiveRecord(AssetsNo,RecordID);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };





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


        public string GetCartId()
        {
            if (HttpContext.Session[CartSessionKey] == null)
            {
               HttpContext.Session[CartSessionKey] =Guid.NewGuid().ToString();

              //  System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
            }

            return HttpContext.Session[CartSessionKey].ToString();
        }


        public async Task<ActionResult> SubmitForApproval(int ID,string Remarks)
        {
            string returnURL = "";
            string body;
            _GroupRequest = new GroupRequestService();

            var obj=_GroupRequest.ViewRequest<GroupRequest>(ID);
            if (ID > 0 )
            {
                var affectedRows = await _GroupRequest.SubmitForApproval(ID, Remarks);
                if (!string.IsNullOrEmpty(obj.Email))
                {
                    string PName = AJESActiveDirectoryInterface.AJESAD.GetName(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));
                    EmailManager VCTEmailService = new EmailManager();
                    body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\RequestStatusUpdate-Approved.html");
                    mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                    mailcontent = mailcontent.Replace("@ProcessOwner", PName); //Replace Contenct...
                    VCTEmailService.Body = mailcontent;
                    VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectApproved");
                    VCTEmailService.ReceiverAddress = obj.Email;
                    VCTEmailService.ReceiverDisplayName = obj.Name;
                    await VCTEmailService.SendEmail();
                }

                EmailManager VCTEmailServiceIT = new EmailManager();
                body = VCTEmailServiceIT.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\RequestStatus-Approved(IT).html");
                mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                VCTEmailServiceIT.Body = mailcontent;
                VCTEmailServiceIT.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectApproved") + "-" + obj.RefNo; ;
                VCTEmailServiceIT.ReceiverAddress = System.Configuration.ConfigurationManager.AppSettings.Get("groupdistribution");
                VCTEmailServiceIT.ReceiverDisplayName = obj.Name;
                await VCTEmailServiceIT.SendEmail();

                returnURL = Url.Action("Index", "Dashboard");
              
                
            }
             return Json(new { Result = returnURL});

        }

        
       public async Task<ActionResult> ConfirmReview(int ID,string Access)
             {
                string returnURL = "";
                _GroupRequest = new GroupRequestService();
                var result = await _GroupRequest.ConfirmReview(ID,Access);
                 returnURL = Url.Action("ViewRequest",new { TransactionID =ID});
                 return Json(new { Result = returnURL });
            }

        public async Task<ActionResult> ConfirmReviewIT(int ID, string Access,string Folder,string ProcessOwner)
        {
            string returnURL = "";
            string body;
            _GroupRequest = new GroupRequestService();
            var processOwnerDetail = AJESActiveDirectoryInterface.AJESAD.ProcessOwnerDetail(ProcessOwner);
            var result = await _GroupRequest.ConfirmReviewIT(ID, Access,Folder,ProcessOwner, processOwnerDetail.ProcessOwnerLoginID, processOwnerDetail.ProcessOwnerEmail);

            var obj = _GroupRequest.ViewRequest<GroupRequestModel>(ID);
            string mGuid = Guid.NewGuid().ToString();

            await _GroupRequest.LogEmail(ID, mGuid, "F");

            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");
            string Link = url + "/GroupRequest/ShowRequest?token=" + mGuid;
            EmailManager VCTEmailService = new EmailManager();

            body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\NewGroupRequest-POnwer.html");
            mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
            mailcontent = mailcontent.Replace("@pwdchangelink", Link); //Replace Contenct...
            VCTEmailService.Body = mailcontent;
            VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("Subject");
            VCTEmailService.ReceiverAddress = processOwnerDetail.ProcessOwnerEmail;
            VCTEmailService.ReceiverDisplayName = ProcessOwner;
            await VCTEmailService.SendEmail();


            returnURL = Url.Action("Index","Dashboard");

            return Json(new { Result = returnURL });
        }


        public async Task<ActionResult> RejectForm(int ID,string Remarks)
        {
            string returnURL = "";

            _GroupRequest = new GroupRequestService();
            var affectedRows = await _GroupRequest.RejectForm(ID, Remarks);

            var obj = _GroupRequest.ViewRequest<GroupRequest>(ID);
            if (ID > 0)
            {
                string PName = AJESActiveDirectoryInterface.AJESAD.GetName(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));
               
                EmailManager VCTEmailService = new EmailManager();
                string body = VCTEmailService.GetBody(Server.MapPath("~/") + "\\App_Data\\Templates\\RequestStatusUpdate-Rejected.html");
                mailcontent = body.Replace("@ReqNo", obj.RefNo); //Replace Contenct...
                mailcontent = mailcontent.Replace("@ProcessOwner", PName); //Replace Contenct...
                mailcontent = mailcontent.Replace("@Reason", Remarks); //Replace Contenct...
                VCTEmailService.Body = mailcontent;
                VCTEmailService.Subject = System.Configuration.ConfigurationManager.AppSettings.Get("SubjectReject");
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

        [Obsolete]
        public async Task<FileResult> GeneratePDF(int RecordID)
        {


            _GroupRequest = new GroupRequestService();

            string url = System.Configuration.ConfigurationManager.AppSettings.Get("Url");

            url = url + "/content/images/logo.png";

            var obj =await _GroupRequest.GeneratePDF<SharefolderModel.GroupRequestModel>(RecordID);

            StringBuilder sb = new StringBuilder();
            sb.Append("<header class='clearfix'>");
            sb.Append("<img src='" + url + "' > ");
            sb.Append("<br>");
            sb.Append("<br>");
            sb.Append("<h1>Folder Request - " + obj.RefNo + " </h1>");
            sb.Append("<br>");
            sb.Append("<br>");
            sb.Append("<table border='1'>");
            sb.Append("<tr>");
            sb.Append("<td>Employee No</td>");
            sb.Append("<td> " + obj .EmpCode + " </td>");
            sb.Append("<td>Name</td>");
            sb.Append("<td>" + obj.Name + "</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Position</td>");
            sb.Append("<td>" + obj.Position + "</td>");
            sb.Append("<td>Project</td>");
            sb.Append("<td>" + obj.ProjectCode + "</td>");
            sb.Append("<td>Process Owner</td>");
            sb.Append("<td>" + obj.ProcessOwner + "</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</header>");
            sb.Append("<br>");
            sb.Append("<main>");
            sb.Append("<table border='1'>");
          
            sb.Append("<tr>");
       
            sb.Append("<td >Folder Name</th>");
            sb.Append("<td>" + obj.Group_Name + "</th>");
            sb.Append("<tr>");

            sb.Append("<td >Access Type</th>");
            sb.Append("<td>" + obj.RequiredAccess + "</th>");
            sb.Append("<tr>");

            sb.Append("<td >Reason</th>");
            sb.Append("<td>" + obj.Reason + "</th>");
            sb.Append("<tr>");

            sb.Append("<td >Remark</th>");
            sb.Append("<td>" + obj.Remarks + "</th>");
            sb.Append("<tr>");




            sb.Append("</table>");
            sb.Append("<br>");
            sb.Append("<div id='notices'>");
            sb.Append("<div>Requested On</div>");
            sb.Append("<div class='notice'>" + obj.CreatedOn.ToString("dd/MM/yyyy hh:mm:ss") + "</div>");
            sb.Append("</div>");

            sb.Append("<div id='proc'>");
            sb.Append("<div><b>Process Owner</b></div>");
            sb.Append("<div class='notice'>" + obj.ProcessOwner + "</div>");
            sb.Append("</div>");
            sb.Append("<div id='created'>");
            sb.Append("<div><b>Approved On</b></div>");
            sb.Append("<div class='notice'>" + obj.ApprovedOn.ToString("dd/MM/yyyy hh:mm:ss") + "</div>");
            sb.Append("</div>");
            sb.Append("<div id='RemarksApp'>");
            sb.Append("<div><b>Approved Remarks</b></div>");
            sb.Append("<div class='notice'>" + obj.ApprovedRemarks + "</div>");

            if (obj.IsReview)
            {
                sb.Append("<div id='ISReview'>");
                sb.Append("<div><b>Approved on Behalf of </b></div>");
                sb.Append("<div style='color: red'>" + obj.ProcessOwnerLoginID + "</div>");

                sb.Append("<div id='Remarks'>");
                sb.Append("<div><b>Delegation Remarks</b></div>");
                sb.Append("<div class='notice'>" + obj.ReviewRemarks + "</div>");
            }
            sb.Append("</div>");
            sb.Append("</main>");
            sb.Append("<footer>");
            sb.Append("<br>");
            sb.Append("Document was created on a computer and is valid without the signature and seal.");
            sb.Append("</footer>");

            string key= "b14ca5898a4e4133bbce2ea2315a1916";

            StringReader sr = new StringReader(sb.ToString());
            Document pdfDoc = new Document(PageSize.EXECUTIVE);
            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
             //   writer.SetEncryption(ASCIIEncoding.Default.GetBytes(key), ASCIIEncoding.Default.GetBytes(key), 0, PdfWriter.ENCRYPTION_AES_256);

                pdfDoc.Open();

                htmlparser.Parse(sr);
                pdfDoc.Close();

                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();
                return File(bytes,"application/pdf","GroupRequest - "+ obj.RefNo + ".PDF");
            }

        }

       public ActionResult MyFolders()
        {
            _GroupRequest = new GroupRequestService();

            var obj = _GroupRequest.MyFolders<ShareFolderPending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return PartialView("MyFolders",obj);
        }

        public ActionResult AllFolders()
        {
            _GroupRequest = new GroupRequestService();

            var obj = _GroupRequest.AllFolders<ShareFolderPending>();

            return PartialView("AllFolders", obj);
        }

        public ActionResult Check()
        {
            List<int> list1 = new List<int> { 1, 1, 2, 3, 4 };
            List<int> list2 = new List<int> { 3, 4, 5, 6,1 };
            List<int> firstNotSecond = list1.Except(list2).ToList();
            var secondNotFirst = list2.Except(list1).ToList();
            Console.WriteLine("Present in List1 But not in List2");
            foreach (var x in firstNotSecond)
            {
              //  Console.WriteLine(x);
            }
            Console.WriteLine("Present in List2 But not in List1");
            foreach (var y in secondNotFirst)
            {
              //  Console.WriteLine(y);
            }
            return View();
        }


    }
}