using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using AJCCFM.Action_Filters;
using AJCCFM.Action_Filters.ErroHandler;

using AJCCFM.Models;

using AJCCFM.Models.Reports;
using AJCCFM.Models.Service;
using Newtonsoft.Json;
using Services.AJESServices;
using Services.EzwareProject;
using Services.GroupRequest;
using Services.Helper;
using Services.JDE;
using static AJCCFM.Models.GroupRequest.SharefolderModel;
using static AJCCFM.Models.Reports.MISReport;

namespace AJCCFM.Controllers
{
    // [RBAC]
    [AjeErrorHandler]
  //  [UserSessionFilter]
    public class DashBoardController : Controller
    {
       
     

        private IGroupRequest _GroupRequest;
        private IServices _Services;
        private IJDE _JDEService;
        private IEzwareProject _EzwareProject;

        // GET: DashBoard



        public ActionResult Index()
        {
           
            return View();

        }


        public ActionResult GetLogHistory(int RecordID, string Doc_Code)
        {

            _Services = new AjesServices();
            var ViewModel = _Services.GetLogHistory<LogHistoryModel>(RecordID, Doc_Code);
            return PartialView("_ProcessLogHistory", ViewModel);
        }







        public ActionResult CashierActivity()
        {
          


            return View();
        }


        public ActionResult ProjectFolderActivity()
        {
            return View();
        }

        public ActionResult ProjectServiceActivity()
        {
            return View();
        }

        public ActionResult EzwareActivity()
        {
            return View();
        }
        
        public ActionResult ProjectJDEActivity()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CashierActivity(string EmployeeCode)
        {
            ViewBag.EmpCode = EmployeeCode;
            return View();
        }

        public ActionResult Pendings()
        {
            return View();
        }

        

            public ActionResult ServiceProcessCount()
        {
            _Services = new AjesServices();

            var obj = _Services.ServiceProcessCount<ProcessedServiceCount>();

            return View("_ProcessedServiceCount", obj);
        }


        public ActionResult FolderProcessCount()
        {
            _GroupRequest = new GroupRequestService();

            var obj = _GroupRequest.FolderProcessCount<ProcessedServiceCount>();

            return View("_ProcessedFolderCount", obj);
        }

        public ActionResult ShareFolderProgress()
        {
             _GroupRequest = new GroupRequestService();

            var obj = _GroupRequest.ShareFolderProgress<ShareFolderProgress>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_ShareFolderProgress", obj);
        }
        public ActionResult ShareFolderPending()
        {
            _GroupRequest = new GroupRequestService();

            var obj = _GroupRequest.ShareFolderPending<ShareFolderPending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_ShareFolderPending", obj);
        }


        


        public ActionResult ServiceProgress()
        {
            _Services = new AjesServices();

            var obj = _Services.ServiceProgress<ServiceProgress>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_ServiceProgress", obj);
        }
        public ActionResult ServicePending()
        {
            _Services = new AjesServices();

            var obj = _Services.ServicePending<ServicePending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_ServicePending", obj);
        }

        public ActionResult JDEProgress()
        {
            _JDEService = new JDEService();

            var obj = _JDEService.JDEProgress<JDEProgress>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_JDEProgress", obj);
        }

   
       public ActionResult JDEPending()
        {
            _JDEService = new JDEService();
            var obj = _JDEService.JDEPending<JDEPending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_JDEPending", obj);
        }


        public ActionResult EzwareProgress()
        {
            _EzwareProject = new EzwareProjectService();

            var obj = _EzwareProject.EzwareProjectProgress<EzwareProgress>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_EzwareProgress", obj);
        }


        public ActionResult EzwarePending()
        {
            _EzwareProject = new EzwareProjectService();
            var obj = _EzwareProject.EzwareProjectPending<EzwarePending>(System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""));

            return View("_EzwarePending", obj);
        }






        public ActionResult ApprovedShareFolderRequest()
        {
            _GroupRequest = new GroupRequestService();

            var obj = _GroupRequest.ApprovedShareFolderRequest<ShareFolderPending>();

            return View("_ApprovedShareFolderRequest", obj);
        }


        public ActionResult ApprovedServices()
        {
            _Services = new AjesServices();

            var obj = _Services.ApprovedServices<ServicePending>();

            return View("_ApprovedServices", obj);
        }

        
             public ActionResult ApprovedJDERequest()
        {
            _JDEService = new JDEService();

            var obj = _JDEService.ApprovedJDERequest<ServicePending>();

            return View("_ApprovedJDERequest", obj);
        }
        public ActionResult GetLedger(int CompanyID = 0)
        {

            return View();
        }
        public ActionResult GetPersonnelLedger(int CompanyID = 0)
        {

                return View();
            }


        [HttpPost]
        public ActionResult RefreshDashboard(int cmbCompany)
        {
            return View();

        }

     
        public ActionResult GraphCategoryWise(int CompanyID = 0)
        {

                return View();

            }
        public ActionResult GraphBUWiseConsumptionHighChart(int CompanyID = 0)
        {

                return View();

            }

        public ActionResult GraphMonthlyPaid(int CompanyID = 0)
        {
                return View();

            }



        public ActionResult GraphProjectManagement()
        {


            DateTime dt = DateTime.Now;

            

            DateTimeOffset now = DateTimeOffset.UtcNow;
            long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();

          

            ViewBag.start = 3;
            ViewBag.end = 15;

            ViewBag.today = unixTimeMilliseconds;

            var day = 1000 * 60 * 60 * 24;


            return View("_GraphProjectManagement");

        }

        public class Product{

            public string Name { get; set; }
            public DateTime Expiry { get; set; }
            public decimal Price { get; set; }
            public string[] Sizes { get; set; }

        }

        public class ganttChart
        {

          
            public string Name { get; set; }
            public string id { get; set; }
            public string parent { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public complete complete { get; set; }

            public ganttChart()
            {
                this.complete = new complete();
            }

        }
        public class complete
        {
            public decimal amount { get; set; }
        }




        public ActionResult GraphBUWiseConsumption(int CompanyID = 0)
        {
            return View();
        }

        public ActionResult BUCategoryWiseConsumption(int CompanyID = 0)
        {
            return View();

        }
        public ActionResult ConsolidateLedger(int CompanyID=0) // //wallet 
        {
            return View();
        }

        public ActionResult GetCashierConsolidateLedger(int CompanyID = 0)
        {
            return View();
        }

        public ActionResult GetStaffConsolidateLedger(int CompanyID = 0)
        {
            return View();
        }

        public ActionResult GetConsolidateBalance(int CompanyID = 0)
        {
            return View();
        }

        public ActionResult GetConsolidatePaidByPocket(int CompanyID = 0)
        {
            return View();
        }

        public ActionResult GetConsolidatePaidCashierByPocket(int CompanyID = 0)
        {
            return View();
        }

            public ActionResult ChartComparision()
        {


            return View();




        }
        public ActionResult Detail(int documentid, string documenttype)
        {
            //From here based on documenttype get table 

            return PartialView("Details");

        }

        public ActionResult DayEndSummary()
        {
            return View(new List<rptCashClaimFormModel>());
        }

        [HttpPost]
        public ActionResult DayEndSummary(DateTime  FromDate, DateTime  ToDate)
        {
            return View("Index");


          
        }



        public ActionResult DayEndSummaryBatchWise()
        {
            return View(new List<rptCashClaimFormModel>());
        }

        [HttpPost]
        public ActionResult DayEndSummaryBatchWise(string BatchNo)
        {
           


 


            return View();
        }



        public ActionResult VIPData()
        {
           

            return View();
        }


        public ActionResult ShowClaimDetail()
        {
            return View();

         
        }

        public ActionResult ExportToExcel(ShowClaimDetail model)
        {
       

            return View("Index");

        }


        public ActionResult ShowDetail(string id )
        {
            
            return View();
        }


       


    }
}