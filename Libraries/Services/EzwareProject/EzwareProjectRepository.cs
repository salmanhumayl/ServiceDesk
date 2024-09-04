using Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using Model.EzwareProject;
using Services.Helper;
using System.Data;
using Core.Domain.EzwareRequest;

namespace Services.EzwareProject
{
    public class EzwareProjectRepository
    {
        public IResponse SubmitRequest(EzwareModel model, string SubmittedTo, string EmpEmailAddress, string SubmittToAddress)
        {
            string[] UserDetail = new string[2];
            string sql = "";
            int RequestID;
            string RefNo = "";
            IResponse response = new IResponse();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                if (model.empdetail.EmpCode == null)
                {
                    response.ErrorMessage = "Employee Code is missing...";
                    return response;
                }
                string Createdby = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");


                try
                {
                    connection.Open();
                    RefNo = Common.GetDocumentNumber("EZ");

                    sql = " Insert into SD_EzwereRequest" +
                          " (RefNo,Status,Name,EmpCode,Position,ProjectCode,Project,Createdby,Email,SubmittedTo,SubmittedToEmail,AssignedProject) " +
                          " Values (@RefNo,0,@EmpName,@EmpCode,@Position,@ProjectCode,@Project,@Createdby,@EmpEmailAddress,@SubmittedTo,'" + SubmittToAddress + "',@ToProject)" +
                          " Select Cast(SCOPE_IDENTITY() AS int)";


                    RequestID = connection.Query<int>(sql, new
                    {
                        RefNo,
                        model.empdetail.EmpCode,
                        model.empdetail.EmpName,
                        model.empdetail.Position,
                        model.empdetail.ProjectCode,
                        model.empdetail.Project,
                        model.ToProject,
                        Createdby,
                        EmpEmailAddress,
                        SubmittedTo

                    }).SingleOrDefault();

                    foreach (var obj in model.EzwareRights)
                    {

                        sql = "INSERT INTO SD_EzwereRequestDetail(RequestId, form_name, [View], [Delete], [Create], [Print], [Edit], [All]) VALUES " +
                        " (" + RequestID + ",'" + obj.form_name + "', " +
                       " " + (Convert.ToBoolean(obj.View) ? 1 : 0) + "," +
                       " " + (Convert.ToBoolean(obj.Delete) ? 1 : 0) + "," +
                       " " + (Convert.ToBoolean(obj.Create) ? 1 : 0) + "," +
                       " " + (Convert.ToBoolean(obj.Print) ? 1 : 0) + "," +
                       " " + (Convert.ToBoolean(obj.Edit) ? 1 : 0) + "," +
                       " " + (Convert.ToBoolean(obj.All) ? 1 : 0) + ")";


                        connection.Execute(sql);

                        

                    }
                    AddLogHistory(RequestID, "S", Createdby.Trim(), SubmittedTo, "", "EZP");

                }

                catch (Exception e)
                {
                    response.ErrorMessage = e.Message;
                    return response;


                }
            }

            response.RequestNo = RefNo;
            response.RecordID = RequestID;

            return response;

        }

        public T ViewRequest<T>(int TransactionID)
        {

            string sql = " Select * FROM SD_EzwereRequest  a " +
                         " Where a.ID=" + TransactionID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;
            }
        }

        public List<T> ViewRequestDetail<T>(int TransactionID)
        {

            string sql = " Select * FROM SD_EzwereRequestDetail  a " +
                         " Where a.RequestId=" + TransactionID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }


     
            public IEnumerable<T> EzwareProjectPending<T>(string username)
        {
         

            string sql = "Select id, RefNo,  Name,CreatedOn,Status from SD_EzwereRequest Where Submittedto='" + username + "' And  status =0 ";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }


        public IEnumerable<T> EzwareProjectProgress<T>(string username)
        {

            string sql = "Select id, RefNo,  Name,CreatedOn from SD_EzwereRequest Where Createdby='" + username + "' And  status >=0 ";




            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }
        public void AddLogHistory(int TransactionID, string Status, string Processby, string SubmittedTo, string Remarks, string Doc_Code)
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


                param.Add("@ReturnVal", dbType: DbType.Int64, direction: ParameterDirection.Output);

                connection.Execute("SD_AddLogHistory", param, commandType: CommandType.StoredProcedure);

                Int64 NextLevel = param.Get<Int64>("@ReturnVal");
            }
        }
        public IEnumerable<T> GetLogHistory<T>(int ID, string Doc_Code)
        {

            string sql = " Select   (Case H.Status When 'A' Then 'Approved' When 'S' Then 'Submitted' when 'F' Then 'Finished' Else 'Rejected' End) as Status, " +
                         " a.FullName as ProcessBy, H.ProcessOn, isnull(b.FullName, 'Finished') as Submittedto, " +
                         " H.Remarks" +
                         " From SD_WorkFlowLogHistory H" +
                         " inner join USERs a on H.ProcessBy = a.loginName" +
                         " inner join USERs b on h.SubmittedTo = b.loginName" +
                         " Where Doc_Code = '" + Doc_Code + "' And H.TransactionID =" + ID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var ViewModel = connection.Query<T>(sql).ToList();
                return ViewModel;
            }
        }

        

     
    }
}
