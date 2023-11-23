using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


                string RefNo = Common.GetDocumentNumber("MR");
                model.RefNo = RefNo;
                sql = " Insert into SD_LinkedInPost " +
                      " (RefNo,Status,Name,EmpCode,Position,ProjectCode,Project,Createdby,Email,SubmittedTo,Post) " +
                      " Values (@RefNo,0,@Name,@EmpCode,@Position,@ProjectCode,@Project,@Createdby,@Email,@SubmittedTo,@Post)" +
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
                    model.Createdby,
                    model.Email,
                    model.SubmittedTo

                });

                if (model.PostDocument !=null)
                {
                    string DirectoryPath = @"\\ajes-fs02\e-Attachments\" + model.RefNo;
                  
                    Directory.CreateDirectory(DirectoryPath);

                    if (Directory.Exists(DirectoryPath))
                    {
                        model.PostDocument[0].SaveAs(Path.Combine(DirectoryPath, "dsadasa"));
                    }

                }

            }
            return "";

        }
    }
}
