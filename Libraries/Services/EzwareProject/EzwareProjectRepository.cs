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
                       // if (obj.Parent == 0)
                       // {
                            sql = "INSERT INTO SD_EzwereRequestDetail(RequestId, Parent,form_name, [All],[Create],[View],Edit,[Delete],[Print]) VALUES " +
                            " (" + RequestID + "," + obj.Parent + ",'" + obj.form_name + "', " +
                           " " + (Convert.ToBoolean(obj.All) ? 1 : 0) + "," +
                           " " + (Convert.ToBoolean(obj.Create) ? 1 : 0) + "," +
                           " " + (Convert.ToBoolean(obj.View) ? 1 : 0) + "," +
                           " " + (Convert.ToBoolean(obj.Edit) ? 1 : 0) + "," +
                           " " + (Convert.ToBoolean(obj.Delete) ? 1 : 0) + "," +
                           " " + (Convert.ToBoolean(obj.Print) ? 1 : 0) + ")";


                            connection.Execute(sql);

                       // }

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


        public async Task<bool> SubmitForApproval(int ID, string remarks)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_EzwereRequest Set Status=-1  Where ID=@RecordID", new { RecordID = ID });

                    AddLogHistory(ID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), "", remarks, "EZP");
                    
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID And DocCode='EZP'", new { RecordID =ID });

                }
                catch (Exception e)
                {
                    string tes = e.Message;
                }
                return true;
            }
        }

        public async Task<bool> RejectForm(int ID, string Remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_EzwereRequest Set Status=-100  Where ID=@RecordID", new { RecordID = ID });
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID And DocCode='EZP'", new { RecordID = ID });

                    AddLogHistory(ID, "R", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), "", Remarks,"EZP");
                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }


        public async Task<int> ArchiveRecord(string AssetsNo, int RecordID)
        {

            string sql = " Update SD_EzwereRequest Set Archive=1,AssystNo='" + AssetsNo + "' where ID=" + RecordID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();

                await connection.ExecuteAsync(sql);

                return 1;
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


        public IEnumerable<T> AllEzwareRequest<T>()
        {
            string sql = " Select id, RefNo, EmpCode,Name,CreatedOn,Createdby,AssystNo,Project,status," +
                         " Status =case when Status = -1 then 'Approved' when Status = -100 then 'Rejected'" +
                         " when Status = 0 then 'Pending' when Status = 1 then 'Pending' end " +
                         " From SD_EzwereRequest Where status in (-1, -100,0,1)";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql);
                return obj.ToList();
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

        public async Task<IEnumerable<T>> GenerateServicePDF<T>(int RecordID)
        {
            string sql = " Select a.*,d.*,Users.FullName as ApprovedBy ,b.ProcessOn as ApprovedOn FROM SD_EzwereRequest  a  " +
                         " Inner join SD_EzwereRequestDetail D on a.id = d.RequestId " +
                         " Inner join SD_WorkFlowLogHistory b on a.id = b.TransactionID "+
                         " Inner Join Users on b.ProcessBy = users.LoginName " +
                         " Where a.id = " + RecordID + "  and b.Doc_Code = 'EZP' and b.Status = 'A' and a.status=-1";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = await connection.QueryAsync<T>(sql);
                return obj.ToList();

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
