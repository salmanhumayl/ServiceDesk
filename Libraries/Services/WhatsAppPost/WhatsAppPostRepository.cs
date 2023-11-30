using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using Model;
using Services.Helper;

namespace Services.WhatsAppPost
{
    public class WhatsAppPostRepository
    {
       public async Task<RefNoID> SubmitGroupRequest(Core.Domain.WhatsApp model)
        {
            string sql = "";
            int RequestID = 0;
            string RefNo;
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                if (model.PostDocument[0]!=null)
                {
                    model.IsAttachment = true;
                }

                RefNo = Common.GetDocumentNumber("MR");
                model.RefNo = RefNo;
                sql = " Insert into SD_WhatsAppPost " +
                      " (RefNo,Status,Name,EmpCode,Position,ProjectCode,Project,Createdby,Email,SubmittedTo,SubmittedToEmail,Post,IsAttachment) " +
                      " Values (@RefNo,0,@Name,@EmpCode,@Position,@ProjectCode,@Project,@Createdby,@Email,@SubmittedTo,@SubmittedToEmail,@Post,@IsAttachment)" +
                      " Select Cast(SCOPE_IDENTITY() AS int)";


                RequestID = await connection.QuerySingleAsync<int>(sql, new
                {
                    RefNo,
                    model.EmpCode,
                    model.Name,
                    model.Position,
                    model.ProjectCode,
                    model.Project,
                    model.Post,
                    model.IsAttachment,
                    model.Createdby,
                    model.Email,
                    model.SubmittedTo,
                    model.SubmittedToEmail


                });

                if (model.PostDocument[0]!=null)
                {

                    string DirectoryPath = System.Web.HttpContext.Current.Server.MapPath("~/Content/images_upload/") + model.RefNo;

                  
                    Directory.CreateDirectory(DirectoryPath);
                  
                    if (Directory.Exists(DirectoryPath))
                    {
                        int Count = 1;
                        foreach (HttpPostedFileBase file in model.PostDocument)
                        {
                            if (file != null)
                            {
                                var InputFileName = "File_" + Count +  Path.GetExtension(file.FileName) ;

                                string path = Path.Combine(DirectoryPath, InputFileName);
                                using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                                {

                                    //Stream inputStream = file.InputStream.CopyTo(outputFileStream);
                                    await  file.InputStream.CopyToAsync(outputFileStream);
                                    Count = Count + 1;
                                    //inputStream.CopyTo(outputFileStream);
                                }
                               

                                //file.SaveAs(Path.Combine(DirectoryPath, InputFileName));

                            }
                        }
                     
                    }

                }
                AddLogHistory(RequestID, "S", model.Createdby.Trim(), model.SubmittedTo, "", "MW");

            }

            RefNoID result = new RefNoID();
            result.ID = RequestID;
            result.RefNo = RefNo;
            return result;

        }


        public T ViewRequest<T>(int TransactionID)
        {
            string sql = " Select * from SD_WhatsAppPost " +
                         " Where ID=" + TransactionID;



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;
            }

        }



        public async Task<bool> SubmitForApproval(int ID, int Status,string Submitedto, string remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {

                    if (Status != -1)
                    {
                        var affectedrows = await connection.ExecuteAsync("Update SD_WhatsAppPost Set Status=" + Status + ",SubmittedTo='" + Submitedto + "' Where ID=@RecordID", new { RecordID = ID });

                        AddLogHistory(ID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), Submitedto, remarks,"MW");
                    }
                    else
                    {
                        var affectedrows = await connection.ExecuteAsync("Update SD_WhatsAppPost Set Status=" + Status + "  Where ID=@RecordID", new { RecordID = ID });

                        AddLogHistory(ID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), "", remarks, "MW");
                    }
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID And DocCode='W'", new { RecordID = ID });

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
                    var affectedrows = await connection.ExecuteAsync("Update SD_WhatsAppPost Set Status=100,RejectedOn='" + DateTime.Now + "',RejectedRemarks='" + Remarks + "' Where ID=@RecordID", new { RecordID = ID });
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID And DocCode='W' ", new { RecordID = ID });

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }

      

        public async Task<bool> LogEmail(int TransactionID, string GUID, string DocCode)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {

                    string sql = " Insert into SD_ApplicationEmailLog " +
                      " (TransactionID,Guid,DocCode) " +
                      " Values (@TransactionID,@GUID,@DocCode)";

                    await connection.ExecuteAsync(sql, new
                    {
                        TransactionID,
                        GUID,
                        DocCode

                    }).ConfigureAwait(false);

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }
        public async Task<int> GetToken(string mGUID, string DocCode)
        {

            string sql = "Select  TransactionID from SD_ApplicationEmailLog Where GUID= '" + mGUID + "' And Status='P' And DocCode='" + DocCode + "'";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = await connection.QueryAsync<int>(sql).ConfigureAwait(false);
                return obj.SingleOrDefault();
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

    }
}
