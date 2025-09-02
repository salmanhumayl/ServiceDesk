using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;


namespace Services.Helper
{

    public class Common
    {




        public static T GetEmpData<T>(string EmpNo)

        {
            string sql = " SELECT ezBusDb.Employee.EmpCode,ezBusDb.Employee.EmpName, ezBusDb.Designation.Name AS Position,ezBusDb.Project.Code as ProjectCode,ezBusDb.Project.Name AS Project,ezBusDb.Employee.ForemanCode,ezBusDb.Department.Name as Department " +
                        " FROM ezBusDb.Employee INNER JOIN" +
                        " ezBusDb.Designation ON ezBusDb.Employee.CmpyCode = ezBusDb.Designation.cmpycode And" +
                        " ezBusDb.Employee.DesignationCode = ezBusDb.Designation.Code" +
                        " INNER JOIN ezBusDb.Project ON ezBusDb.Employee.CmpyCode = ezBusDb.Project.cmpycode AND " +
                        " ezBusDb.Employee.ProjectCode = ezBusDb.Project.Code" +
                        " Left outer Join ezBusDb.Department on ezBusDb.Employee.DepartmentCode=ezBusDb.Department.Code" +
                        " where ezBusDb.employee.empcode = '" + EmpNo + "' and ezbusdb.employee.cmpycode = '01'";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStrEZWARE"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;
            }

        }




        public static int UpdateSQL(string sql)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                int RowEffected = connection.Execute(sql);
                return RowEffected;
            }
        }


        public static int ExecuteQuery(string sql)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                int RowEffected = connection.Execute(sql);
                return RowEffected;
            }
        }


        public static IEnumerable<T> GetDataByID<T>(string sql, int nCashierID)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = db.Query<T>(sql, new { CashierID = nCashierID }).ToList();
                db.Close();
                return obj;

            }

        }

        public static IEnumerable<T> GetData<T>(string sql)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = db.Query<T>(sql).ToList();
                db.Close();
                return obj;

            }

        }

        public static IEnumerable<T> GetTemplate<T>(int CategoryID)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                String sql = "Select id,FieldName,FieldHeading,Width,CategoryID from CFM_CategoryTemplate Where CategoryID=" + CategoryID;
                var obj = db.Query<T>(sql).ToList();
                db.Close();
                return obj;

            }

        }


        public static IEnumerable<T> GetTemplateJDECode<T>(int CategoryID)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                String sql = " select a.id, d.Code as CatCode, C.Code + ' - ' + c.Name + c.Name as Company,b.Code + ' - ' + b.Name as BU,a.JDEObject,a.JDESub from CFM_tblCompany c,CFM_tblCompany b, CFM_tblCategory d," +
                             " CFM_Category_JDECode a " +
                             " where a.CompanyID = c.CompanyID and a.buID = B.CompanyID And d.id=a.categoryID";

                var obj = db.Query<T>(sql).ToList();
                db.Close();
                return obj;

            }

        }


        public static T JDECode<T>(int CompanyID, int BUid, int CategoryID)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                String sql = " select JDEObject,JDESub from CFM_Category_JDECode where CompanyID=" + CompanyID + " AND buid=" + BUid + " and categoryid=" + CategoryID;

                var obj = db.Query<T>(sql).SingleOrDefault();
                db.Close();
                return obj;

            }

        }


        public static T GetClaimCompany<T>(int ClaimID)
        {
            string sql = " Select b.CompanyID,b.BuID From CFM_Claims b Where b.ClaimID =@ID";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql, new { ID = ClaimID }).SingleOrDefault();
                return obj;
            }
        }

        public static void AddTemplate()
        {



        }



        public static List<string> IsTemplateDefine(int CategoryID)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                return connection.Query<String>(string.Format("Select CategoryID from CFM_CategoryTemplate Where CategoryID= {0}", CategoryID)).ToList();

            }

        }

        public static dynamic GetTemplateClaimValues(string tableName, string ColumnName, int? ClaimID)
        {

            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                dynamic obj;
                string col = ColumnName;
                String sql = "Select " + ColumnName + "  from " + tableName + " Where __HClaimID=" + ClaimID;
                System.Data.IDataReader Ireader = db.ExecuteReader(sql);
                Ireader.Read();


                try
                {
                    obj = Ireader[ColumnName];


                }
                catch (Exception)
                {
                    return 0;

                }

                return obj;
            }

        }

        public static decimal ExecuteReader(string sql)
        {

            decimal Qty = 0;

            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {


                System.Data.IDataReader Ireader = db.ExecuteReader(sql);

                Ireader.Read();
                //var Qty = reader.GetValue(0).ToString();
                //reader.GetInt32(reader.GetOrdinal("Kind")))

                try
                {
                    decimal obj = Convert.ToDecimal(Ireader["Amount"]);
                    Qty = (decimal)obj;

                }
                catch (Exception)
                {
                    return 0;

                }
            }
            return Qty;





        }


        public static dynamic GetTableSchema(string TableName, string loadType)
        {
            string DBName = ConfigurationManager.AppSettings["DBName"].ToString();
            string sql;
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                if (loadType == "R")
                {
                    sql = " SELECT COLUMN_NAME FROM " + DBName + ".INFORMATION_SCHEMA.COLUMNS" +
                                " WHERE TABLE_NAME = N'" + TableName + "' AND COLUMN_NAME NOT IN ('ID') ";
                }
                else
                {
                    sql = " SELECT COLUMN_NAME FROM " + DBName + ".INFORMATION_SCHEMA.COLUMNS" +
                               " WHERE TABLE_NAME = N'" + TableName + "' AND COLUMN_NAME NOT IN ('ID','Type') ";
                }

                var obj = db.Query(sql).ToList();
                db.Close();
                return obj;

            }

        }


        public static bool ShowEmployeeFetchButton(int CategoryID)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                var obj = connection.Query(string.Format("Select EmployeeFetch from CFM_tblCategory Where ID= {0}", CategoryID)).SingleOrDefault();

                if (obj != null)
                {
                    return (bool)obj.EmployeeFetch;
                }
                return false;
            }

        }
        public static String GetTableName(int CategorID)
        {

            String TableName = "";

            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {


                System.Data.IDataReader Ireader = db.ExecuteReader("Select TableName from CFM_CategoryTemplate where CategoryID=" + CategorID);

                Ireader.Read();
                //var Qty = reader.GetValue(0).ToString();
                //reader.GetInt32(reader.GetOrdinal("Kind")))

                try
                {
                    var obj = Ireader["TableName"];
                    TableName = (String)obj;

                }
                catch (Exception)
                {
                    return "";

                }
            }
            return TableName;





        }



        public static string GetDocumentNumber(string DocumentCode)
        {

            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    int LastNumber;

                    string NewLastNumber;
                    string sql;


                    sql = "Select LastNumber from SD_docinfo where Code='" + DocumentCode + "'";

                    var obj = db.Query<AutoGenerateNumber>(sql).SingleOrDefault();

                    if (obj != null)
                    {

                        LastNumber = obj.LastNumber + 1;

                        if (DocumentCode == "EZ")
                        {
                            NewLastNumber = "EZ" + "-" + LastNumber.ToString("000000");
                        }
                        else if (DocumentCode == "JR")
                        {
                            NewLastNumber = "JR" + "-" + LastNumber.ToString("000000");
                        }
                        else if (DocumentCode == "I")
                        {
                            NewLastNumber = "I" + "-" + LastNumber.ToString("000000");
                        }
                        else
                        {
                            NewLastNumber = "SF" + "-" + LastNumber.ToString("000000");
                        }


                        //Update last generated Number 
                        sql = "Update SD_docinfo Set LastNumber='" + LastNumber.ToString("000000") + "' where Code='" + DocumentCode + "'";
                        db.Execute(sql);
                        return NewLastNumber;

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return "";
        }


        public static string GetDocumentNumberService(string DocumentCode)
        {

            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    int LastNumber;

                    string NewLastNumber;
                    string sql;


                    sql = "Select LastNumber from SD_docinfo where Code='" + DocumentCode + "'";

                    var obj = db.Query<AutoGenerateNumber>(sql).SingleOrDefault();

                    if (obj != null)
                    {

                        LastNumber = obj.LastNumber + 1;


                        NewLastNumber = "SR" + "-" + LastNumber.ToString("000000");


                        //Update last generated Number 
                        sql = "Update SD_docinfo Set LastNumber='" + LastNumber.ToString("000000") + "' where Code='" + DocumentCode + "'";
                        db.Execute(sql);
                        return NewLastNumber;

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return "";
        }



        public static decimal GetConsolidatePaidByPocket(int CompanyID)
        {
            string sql = " Select SUM(b.CashPaid) as Amount   from cfm_claims A ,CFM_ClaimDetails  B " +
                         " WHERE a.ClaimID = b.ClaimID " +
                         " and a.PaymentStatus = 'T' and a.status<>-99 And a.CompanyID =" + CompanyID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query(sql).ToArray();
                if (obj.Count() > 0)
                {
                    return Convert.ToDecimal(obj[0].Amount);
                }
            }
            return 0;
        }

        public static string GetEmailAddress(int UserID)
        {
            string sql = "Select EmailAddress from CFM_USER WHERE Usr_Key=" + UserID;


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                string obj = connection.ExecuteScalar<string>(sql);
                return obj;
            }

        }

        public static int IsPaidFromAnotherCompany(int ClaimID)
        {
            string sql = "Select PaidFromCompanyID from CFM_Claims WHERE ClaimID=" + ClaimID;


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                int obj = connection.ExecuteScalar<int>(sql);
                return obj;
            }

        }

        public static IList<T> GetProject<T>(string mproject)
        {
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                string sql = " select Code,Name  from AjePayroll.ezBusDb.Project where ActiveYN='Y' AND cmpycode='01' and Code not in ('8000') " +
                             " Union ALL Select '1' as Code, 'Please Select Project' " +
                             " ORDER BY Code,Name";

                if (mproject == "S")
                {
                    sql = " select Code,Name  from AjePayroll.ezBusDb.Project where Code='8000' AND cmpycode='01'";

                }

                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }

        public class AutoGenerateNumber
        {
            public int LastNumber { get; set; }
            public int nYear { get; set; }
        }
    }
}