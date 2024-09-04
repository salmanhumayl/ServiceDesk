using AJCCFM.Core;

using Core.Domain;
using Core.Domain.EzwareRequest;
using Model;
using Model.EzwareProject;
using Services.EzwareProject;
using Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AJCCFM.Models.Extention;

namespace AJCCFM.Controllers
{
    public class EzwareFormController : Controller
    {
        EzwareProjectService _EzwareProjectServices;
        List<RightModel> rights = new List<RightModel>();

        public ActionResult Index()
        {
            List<UserDetail> lADUser;
            if (TempData["EmpCode"] != null)
            {
                List<ProjectDetail> objProject;
                EzwareModel EzwareModel = new EzwareModel();
                EmployeeDetail empDetail = new EmployeeDetail();

                ViewBag.IsEmployeeExist = false;
                rights = GetBlankUserRights();
                empDetail = Services.Helper.Common.GetEmpData<EmployeeDetail>(TempData["EmpCode"].ToString());
                objProject = Common.GetProject<ProjectDetail>().ToList();
                ViewBag.ToProject = new SelectList(objProject, "Code", "Name");

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

                    EzwareModel.EzwareRights = rights;
                    EzwareModel.empdetail = empDetail;
                    return View(EzwareModel);
               }

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


        [HttpPost]
        public ActionResult SubmitRequest(EzwareModel model, string forwardto, string forwardName)
        {
            string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.empdetail.EmpCode);
            string SubmittToAddress = AJESActiveDirectoryInterface.AJESAD.GetEmailAddress(forwardto);
            string[] Name = forwardName.Split('-');
            _EzwareProjectServices = new EzwareProjectService();
            IResponse result = _EzwareProjectServices.SubmitRequest(model, forwardto, EmpEmailAddress, SubmittToAddress);
            if (result.ErrorMessage != null)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Dashboard");

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
    }
}