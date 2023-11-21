using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AJCCFM.Controllers
{
    public class UnauthorisedController : Controller
    {
        // GET: Unauthorised
        // GET: Unauthorised
        public ActionResult Index()
        {
            //Session.Abandon();
            return View();
        }
    }
}