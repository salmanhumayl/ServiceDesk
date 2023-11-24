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
                    string DirectoryPath = @"\\ajes-fs02\e-Attachments\" + model.RefNo;
                  
                    Directory.CreateDirectory(DirectoryPath);

                    if (Directory.Exists(DirectoryPath))
                    {
                        int Count = 1;
                        foreach (HttpPostedFileBase file in model.PostDocument)
                        {
                            if (file != null)
                            {
                                var InputFileName = "File_" + Count +  Path.GetExtension(file.FileName) ;
                                 
                                file.SaveAs(Path.Combine(DirectoryPath, InputFileName));
                                Count= Count  + 1;
                            }
                        }
                     
                    }

                }

            }
            return "";

        }
    }
}
