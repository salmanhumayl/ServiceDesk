
using Core.Domain;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using Model.RoleApproval;

namespace AJESActiveDirectoryInterface
{
    public static class AJESAD
    {
        public static int RoleID;
        public static List<UserDetail> GetADUsers()

        {

            Services.IWorkFlow _WorkFlowService;

            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;

            DirectoryEntry entry = new DirectoryEntry(sPath);

            DirectorySearcher mySearcher = new DirectorySearcher(entry);

            SearchResult result;

            SearchResultCollection resultCol;

            SortOption so = new SortOption();

            so.Direction = SortDirection.Ascending;

            so.PropertyName = "DisplayName";

            mySearcher.Sort = so;

            mySearcher.PropertiesToLoad.Add("samaccountname");

            mySearcher.PropertiesToLoad.Add("DisplayName");

            mySearcher.PropertiesToLoad.Add("title");

            mySearcher.PropertiesToLoad.Add("ipPhone");

            mySearcher.PropertiesToLoad.Add("mail");

            mySearcher.PropertiesToLoad.Add("description");

            //mySearcher.Filter = ("(&(objectClass=user)(objectCategory=person)(msExchUserAccountControl=0)(userAccountControl=512))");



            _WorkFlowService = new Services.WorkFlow.WorkFlowService();

            string strcriteria = "";
           
            if (RoleID > 0)

            {

                strcriteria = "(|";

                var Approver = _WorkFlowService.RoleDetail<ApprovalAuthorityDetail>(0, RoleID);



                foreach (var item in Approver)

                {

                    strcriteria += "(samaccountname=" + item.LoginID + ")";

                }

                strcriteria += ")";

            }

          





            mySearcher.Filter = ("(&(objectClass=user)(objectCategory=person)(msExchUserAccountControl=0)" + strcriteria + ")");



            //mySearcher.Filter = ("(&(objectClass = user)(objectCategory = person)(msExchUserAccountControl = 0)(!(MSExchHideFromAddressLists = TRUE)))");



            //mySearcher.Filter = ("(&(objectClass=user)(objectCategory=person))");



            mySearcher.PageSize = 1000;

            int i = 0;

            resultCol = mySearcher.FindAll();

            UserDetail obj;

            List<UserDetail> lUserDetail = new List<UserDetail>();

            while (i < resultCol.Count)

            {

                result = resultCol[i];

                if (result.Properties.Contains("DisplayName") && result.Properties.Contains("title") && result.Properties.Contains("ipPhone"))

                {

                    obj = new UserDetail();

                    obj.Name = result.Properties["DisplayName"][0].ToString();

                    obj.Title = result.Properties["title"][0].ToString();

                    if (result.Properties["ipPhone"].Count > 0)

                    {

                        obj.ipPhone = result.Properties["ipPhone"][0].ToString();

                    }



                    if (result.Properties["samaccountname"].Count > 0)

                    {

                        obj.LoginName = result.Properties["samaccountname"][0].ToString();

                    }

                    if (result.Properties["description"].Count > 0)

                    {

                        obj.Project = result.Properties["description"][0].ToString();

                    }



                    obj.DisplayText = result.Properties["DisplayName"][0].ToString() + " - " + result.Properties["title"][0].ToString() + " - " + obj.Project;

                    lUserDetail.Add(obj);

                }

                i = i + 1;

            }

            return lUserDetail;



        }

        public static string[] GetADUserDetails(string username)
        {
            string[] a = new string[5];
            a[0] = "";
            a[1] = "";
            a[2] = "";
            a[3] = "";
            a[4] = "";
            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectoryEntry entry = new DirectoryEntry(sPath);
            DirectorySearcher search = new DirectorySearcher(entry);
            search.SearchScope = SearchScope.Subtree;

            search.Filter = String.Format("(sAMAccountName={0})", username);
            search.PropertiesToLoad.Add("cn");
            search.PropertiesToLoad.Add("Department");
            search.PropertiesToLoad.Add("Title");
            search.PropertiesToLoad.Add("DisplayName");
            search.PropertiesToLoad.Add("ipPhone");

            SearchResult result = search.FindOne();

            if (result == null)
            {
                return a;
            }
            int groupCount = result.Properties["cn"].Count;

            if (groupCount > 0)
            {

                if (result.Properties["ipPhone"].Count > 0)
                {
                    string str1 = result.Properties["ipPhone"][0].ToString();
                    a[0] = str1;
                }

                if (result.Properties["DisplayName"].Count > 0)
                {
                    string str1 = result.Properties["DisplayName"][0].ToString();
                    a[1] = str1;
                }
                if (result.Properties["Department"].Count > 0)
                {
                    string str = result.Properties["Department"][0].ToString();
                    a[2] = str;
                }


                if (result.Properties["Title"].Count > 0)
                {
                    string str1 = result.Properties["Title"][0].ToString();
                    a[3] = str1;
                }
            }
            return a;
        }


        public static string GetEmailAddress(string strLoginName)
        {
            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectorySearcher search = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(sPath);
            search.SearchRoot = entry;
            search.Filter = String.Format("(sAMAccountName={0})", strLoginName);
            search.PropertiesToLoad.Add("mail");
            search.SearchScope = SearchScope.Subtree;
            SearchResult result = search.FindOne();

            int groupCount = result.Properties["mail"].Count;
            if (groupCount > 0)
            {
                if (result.Properties["mail"].Count > 0)
                {
                    return result.Properties["mail"][0].ToString();
                }
            }
            search.Dispose();
            entry.Dispose();
            return "";

        }

        public static string GetEmpEmail(string EmpCode)

        {

            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;

            DirectorySearcher search = new DirectorySearcher();

            DirectoryEntry entry = new DirectoryEntry(sPath);

            search.SearchRoot = entry;

            search.Filter = String.Format("(ipPhone={0})", EmpCode);

            search.PropertiesToLoad.Add("mail");

            search.SearchScope = SearchScope.Subtree;

            SearchResult result = search.FindOne();

            if (result == null)

            {

                return "";

            }





            int groupCount = result.Properties["mail"].Count;

            if (groupCount > 0)

            {

                if (result.Properties["mail"].Count > 0)

                {

                    return result.Properties["mail"][0].ToString();

                }

            }

            search.Dispose();

            entry.Dispose();

            return "";



        }

        public static string GetEmpLogin(string EmpCode)

        {

            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;

            DirectorySearcher search = new DirectorySearcher();

            DirectoryEntry entry = new DirectoryEntry(sPath);

            search.SearchRoot = entry;

            search.Filter = String.Format("(ipPhone={0})", EmpCode);

            search.PropertiesToLoad.Add("sAMAccountName");

            search.SearchScope = SearchScope.Subtree;

            SearchResult result = search.FindOne();

            if (result == null)

            {

                return "";

            }





            int groupCount = result.Properties["sAMAccountName"].Count;

            if (groupCount > 0)

            {

                if (result.Properties["sAMAccountName"].Count > 0)

                {

                    return result.Properties["sAMAccountName"][0].ToString();

                }

            }

            search.Dispose();

            entry.Dispose();

            return "";



        }

        public static string GetEmpNo(string strLoginName)
        {
            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectorySearcher search = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(sPath);
            search.SearchRoot = entry;
            search.Filter = String.Format("(sAMAccountName={0})", strLoginName);
            search.PropertiesToLoad.Add("ipPhone");
            search.SearchScope = SearchScope.Subtree;
            SearchResult result = search.FindOne();
            if (result != null)
            {
                int groupCount = result.Properties["ipPhone"].Count;
                if (groupCount > 0)
                {
                    if (result.Properties["ipPhone"].Count > 0)
                    {
                        return result.Properties["ipPhone"][0].ToString();
                    }
                }
            }
            search.Dispose();
            entry.Dispose();
            return "";

        }

        public static string Mid(string s, int a, int b)

        {

            string temp = s.Substring(a - 1, b);

            return temp;

        }

        public static string GetDisplayName()
        {
            string strLoginName = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectorySearcher search = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(sPath);
            search.SearchRoot = entry;
            search.Filter = String.Format("(sAMAccountName={0})", strLoginName);
            search.PropertiesToLoad.Add("DisplayName");
            search.SearchScope = SearchScope.Subtree;
            SearchResult result = search.FindOne();

            int groupCount = result.Properties["DisplayName"].Count;
            if (groupCount > 0)
            {
                if (result.Properties["DisplayName"].Count > 0)
                {
                    return result.Properties["DisplayName"][0].ToString();
                }
            }
            search.Dispose();
            entry.Dispose();
            return "";

        }

        public static string GetName(string strLoginName)
        {
            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectorySearcher search = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(sPath);
            search.SearchRoot = entry;
            search.Filter = String.Format("(sAMAccountName={0})", strLoginName);
            search.PropertiesToLoad.Add("DisplayName");
            search.SearchScope = SearchScope.Subtree;
            SearchResult result = search.FindOne();

            int groupCount = result.Properties["DisplayName"].Count;
            if (groupCount > 0)
            {
                if (result.Properties["DisplayName"].Count > 0)
                {
                    return result.Properties["DisplayName"][0].ToString();
                }
            }
            search.Dispose();
            entry.Dispose();
            return "";

        }
        public static ProcesOwnerDetail ProcessOwnerDetail(string Name)
        {
            ProcesOwnerDetail processownerdetail = new ProcesOwnerDetail();

            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectorySearcher search = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(sPath);
            search.SearchRoot = entry;
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("sAMAccountName");

            search.SearchScope = SearchScope.Subtree;
            search.Filter = ("(&(objectClass=user)(objectCategory=person)(displayName=" + Name + "))");

            SearchResult searchResults;
            searchResults = search.FindOne();

            if (searchResults != null)
            {

                if (searchResults.Properties["mail"].Count > 0)
                {
                    processownerdetail.ProcessOwnerEmail = searchResults.Properties["mail"][0].ToString();
                }

                if (searchResults.Properties["sAMAccountName"].Count > 0)
                {
                    processownerdetail.ProcessOwnerLoginID = searchResults.Properties["sAMAccountName"][0].ToString();
                }
            }
             return processownerdetail;
        }


        public  static string[] GetADUserGroupInfo(string GroupName)
        {
            string[] a = new string[2];
            a[0] = "";
            a[1] = "";
            string sPath = System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString;
            DirectorySearcher search = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(sPath);
            search.SearchRoot = entry;
            //  search.Filter = String.Format("(cn={0})", GroupName);


            search.Filter = "(&(objectCategory=group)(cn=" + GroupName + ")(!(extensionAttribute5=System-Group)))";


            search.PropertiesToLoad.Add("Description");
            search.PropertiesToLoad.Add("cn");
            search.SearchScope = SearchScope.Subtree;
            SearchResult result = search.FindOne();
            if (result == null)
            {
                return a;
            }
            int groupCount = result.Properties["Description"].Count;


            string str = result.Properties["cn"][0].ToString();
            a[0] = str;


            if (groupCount > 0)
            {
                string str1 = result.Properties["Description"][0].ToString();
                a[1] = str1;
            }


            return a;
        }


      
    }
}