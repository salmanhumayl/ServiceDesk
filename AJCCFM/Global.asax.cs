
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AJCCFM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string connString = ConfigurationManager.ConnectionStrings
         ["ConStr"].ConnectionString;

        

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // MvcHandler.DisableMvcResponseHeader = true;

            // GlobalFilters.Filters.Add(new UserAuditFilter());//Register UserAuditLog
            //  SqlDependency.Start(connString);


            log4net.Config.XmlConfigurator.Configure();

    }

        protected void Application_PreSendRequestHeaders()
        {
          //  Response.Headers.Remove("Server"); //Remove server header 
           // Response.Headers.Remove("X-AspNet-Version"); //remove X-AspNet-Version

        }
        protected void Application_End()
        {
            //Stop SQL dependency
          //  SqlDependency.Stop(connString);
        }
    }
}
