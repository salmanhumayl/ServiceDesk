using Dapper;
using Services.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WorkFlow
{
    public class WorkFlowRepository
    {
        public void AddLogHistory(int TransactionID, string Status, string Processby, string SubmittedTo, string Remarks, int Doc_Code,int SeqNo)
        {
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@TransactionID", TransactionID);
                param.Add("@Status", Status);
                param.Add("@ProcessBy", Processby);
                param.Add("@SubmittedTo", SubmittedTo);
                param.Add("@ProcessOn", DateTime.Now);
                param.Add("@Remarks", Remarks);
                param.Add("@Doc_Code", Doc_Code);
                param.Add("@SeqNo", SeqNo);

                param.Add("@ReturnVal", dbType: DbType.Int64, direction: ParameterDirection.Output);

                connection.Execute("AddLogHistory", param, commandType: CommandType.StoredProcedure);

                Int64 NextLevel = param.Get<Int64>("@ReturnVal");
            }


        }
        public T GetNextLevel<T>(int Doc_Code, int CurrentLevel)
        {
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {


                DynamicParameters param = new DynamicParameters();
                param.Add("@Doc_Code", Doc_Code);
                param.Add("@CurrentLevel", CurrentLevel);
                var obj = connection.Query<T>("eForm_NextLevel", param, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();
                return obj;
            }
        }

        public IEnumerable<T> RoleDetail<T>(int Doc_Code, int RoleID)
        {
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@RoleID", RoleID);
                var obj = connection.Query<T>("eForm_RoleDetail", param, commandType: System.Data.CommandType.StoredProcedure).ToList();
                return obj;
            }
        }

        public string SubmitForApproval(int Doc_Code, int CurrentLevel, int TransactionID, string Remarks,string TableName,string SubmittedTo,int SeqNo)
        {
            string sql;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                IDbTransaction transaction;
                connection.Open();
                transaction = connection.BeginTransaction();

            

                sql = "Update " + TableName + " Set Status=" + CurrentLevel + " Where ID=" + TransactionID;

                connection.Execute(sql, null, transaction);
                if (CurrentLevel != -1)
                {
                    sql = " Delete from  eForm_PendingApproval  Where TransactionID=" + TransactionID + "  And Doc_Code=" + Doc_Code + "";

                    connection.Execute(sql, null, transaction);

                    sql = " Insert into eForm_PendingApproval(SubmittedTo, doc_Code, TransactionID)" +
                          " Values ('" + SubmittedTo + "' , " + Doc_Code + ", " + TransactionID + ")";

                    connection.Execute(sql, null, transaction);
                   
                    
                }
                transaction.Commit();
                if (CurrentLevel != -1)
                {
                    AddLogHistory(TransactionID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), SubmittedTo, "", Doc_Code, SeqNo);
                    
                }
                else
                {
                    AddLogHistory(TransactionID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), "", "", Doc_Code, SeqNo);
                    DeletPending(TransactionID,  Doc_Code);
                }
                return "";
            }
        }

        public IEnumerable<T> GetLogHistory<T>(int ID, int Doc_Code)
        {

            string sql = " Select   (Case H.Status When 'A' Then 'Approved' When 'S' Then 'Submitted' when 'F' Then 'Finished' Else 'Rejected' End) as Status, " +
                         " a.FullName as ProcessBy, H.ProcessOn, isnull(b.FullName, 'Finished') as Submittedto, " +
                         " H.Remarks" +
                         " From eForm_WorkFlowLogHistory H" +
                         " inner join USERs a on H.ProcessBy = a.loginName" +
                         " inner join USERs b on h.SubmittedTo = b.loginName" +
                         " Where Doc_Code = " + Doc_Code + " And H.TransactionID =" + ID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var ViewModel = connection.Query<T>(sql).ToList();
                return ViewModel;
            }
        }

        public IEnumerable<T> GetClearanceLogHistory<T>(int ID, int Doc_Code)
        {

            string sql = " Select   (Case H.Status When 'A' Then 'Approved' When 'P' Then 'In Process' when 'F' Then 'Finished' Else 'Rejected' End) as Status, " +
                         " a.FullName as ProcessBy, H.ProcessOn, isnull(b.FullName, 'Finished') as Submittedto, " +
                         " H.Remarks,H.Approvedon,y.StepName,h.Doc_Code" +
                         " From eForm_WorkFlowLogHistory H" +
                         " left outer join eForm_ClearanceStep y on h.CleranceStepID = y.id " +
                         " inner join USERs a on H.ProcessBy = a.loginName" +
                         " inner join USERs b on h.SubmittedTo = b.loginName" +
                         " Where Doc_Code = " + Doc_Code + " And H.TransactionID =" + ID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var ViewModel = connection.Query<T>(sql).ToList();
                return ViewModel;
            }
        }
        public IEnumerable<T> GetOptionalLevel<T>(int Doc_Code)
        {

            string sql = " Select * From eForm_WorkFlowConfiguration  Where Doc_Code = " + Doc_Code + " And IsOptional=1";
                          

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var ViewModel = connection.Query<T>(sql).ToList();
                return ViewModel;
            }
        }


        public int UpdateStatus(int ID, int Status, string TableName,int Doc_Code)
        {
            string sql = "";
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "Update " + TableName + " Set Status=" + Status + " Where ID=" + ID;
                connection.Execute(sql);
            }
            return -1;

        }


        public void DeletPending(int TransactionID,int Doc_Code)

        {
            string sql;
            sql = " Delete from  eForm_PendingApproval Where TransactionID = " + TransactionID + " And Doc_Code = " + Doc_Code + "";
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Execute(sql);
            }
        }
    }
    
}
