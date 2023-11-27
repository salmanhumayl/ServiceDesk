using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using Services.Helper;

namespace Services.LinkedInPost
{
    public class LinkedInPostRepository
    {
        public async Task<string> SubmitGroupRequest(Core.Domain.LinkedInPost model)
        {
            string sql = "";
            int RequestID = 0;
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                if (model.PostDocument!=null)
                {
                    model.IsAttachment = true;
                }

                    string RefNo = Common.GetDocumentNumber("MR");
                model.RefNo = RefNo;
                sql = " Insert into SD_LinkedInPost " +
                      " (RefNo,Status,Name,EmpCode,Position,ProjectCode,Project,Createdby,Email,SubmittedTo,Post,IsAttachment) " +
                      " Values (@RefNo,0,@Name,@EmpCode,@Position,@ProjectCode,@Project,@Createdby,@Email,@SubmittedTo,@Post,@IsAttachment)" +
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
                    model.SubmittedTo

                });

                if (model.PostDocument!=null)
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

            }
           
            return "";

        }


        public T ViewRequest<T>(int TransactionID)
        {
            string sql = " Select * from SD_LinkedInPost " +
                         " Where ID=" + TransactionID;



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;
            }

        }
        public IEnumerable<T> LinkInPostPending<T>(string username)
        {
            string sql = "select id,RefNo,EmpCode,Name, 'LinkedIn' as SNType from SD_LinkedInPost Where Status = 0 " +
                        " Union All " +
                        " select id,RefNo,EmpCode,Name, 'WhatsApp Post' as Type from SD_WhatsAppPost Where Status = 0";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
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

    }
}
