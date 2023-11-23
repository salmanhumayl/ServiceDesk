using AJCCFM.Core;
using Core.Domain;
using Dapper;
using Services.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.GroupRequest
{
    public class GroupRequestRepository
    {
        public string SaveClaim(Cart ClaimData)
        {
        
            string sql = "";
          using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                  try
                {
                    connection.Open();
                   
                
                        sql = " Insert into SD_Cart " +
                          " (CartId,Group_Name,RequiredAccess,Remarks,Reason,ProcessOwner,ProcessOwnerEmail,ProcessOwnerLoginID) " +
                          " Values (@CartId,@Group_Name,@RequiredAccess,@Remarks,@Reason,@ProcessOwner,@ProcessOwnerEmail,@ProcessOwnerLoginID)" +
                            "Select Cast(SCOPE_IDENTITY() AS int)";

                    connection.Query(sql, new
                    {
                        ClaimData.CartId,
                        ClaimData.Group_Name,
                        ClaimData.RequiredAccess,
                        ClaimData.Remarks,
                        ClaimData.Reason,
                        ClaimData.ProcessOwner,
                        ClaimData.ProcessOwnerEmail,
                        ClaimData.ProcessOwnerLoginID
                       
                    }).SingleOrDefault();
                    
                  
                }

                catch (Exception e)
                {
                  
                    return e.Message;


                }
            }

            return "";

        }

        public string SubmitGroupRequest(EmployeeDetail model, string CardID)
        {
            string[] UserDetail = new string[2];
            string sql = "";
            int RequestID;
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                if (model.EmpCode == null)
                {
                    return "Employee Code is missing...";
                }
                string Createdby = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
                string EmpEmailAddress = AJESActiveDirectoryInterface.AJESAD.GetEmpEmail(model.EmpCode);


                try
                {
                    connection.Open();

                    sql = "Select * from SD_Cart where CartId='" + CardID + "'";

                    var cartitems = connection.Query<Cart>(sql).ToList();

                    if (cartitems.Count > 0 )

                    {
                        foreach (var items in cartitems)
                        {
                            string RefNo = Common.GetDocumentNumber("GR");

                            sql = " Insert into SD_GroupRequest " +
                                  " (RefNo,Status,Name,EmpCode,Position,ProjectCode,Project,Createdby,Email,ProcessOwner,ProcessOwnerLoginID,ProcessOwnerEmail,Cartid,ActualProcessOwner) " +
                                  " Values (@RefNo,0,@EmpName,@EmpCode,@Position,@ProjectCode,@Project,@Createdby,@EmpEmailAddress,@ProcessOwner,@ProcessOwnerLoginID,@ProcessOwnerEmail,@CardID,'" + items.ProcessOwnerLoginID +"')" +
                                  " Select Cast(SCOPE_IDENTITY() AS int)";


                            RequestID = connection.Query<int>(sql, new
                            {
                                RefNo,
                                model.EmpCode,
                                model.EmpName,
                                model.Position,
                                model.ProjectCode,
                                model.Project,
                                Createdby,
                                EmpEmailAddress,
                                items.ProcessOwner,
                                items.ProcessOwnerLoginID,
                                items.ProcessOwnerEmail,
                                CardID
                              

                            }).SingleOrDefault();



                            sql = " Insert into SD_GroupReqDetail " +
                                  " (RequestID,Group_Name,RequiredAccess,Remarks,Reason) " +
                                  " Values (@RequestID,@Group_Name,@RequiredAccess,@Remarks,@Reason)";

                            connection.Query(sql, new
                            {

                                items.Group_Name,
                                items.RequiredAccess,
                                items.Remarks,
                                items.Reason,
                                RequestID

                            }).SingleOrDefault();

                        } 
                    }
                    else
                    {
                        return "Add group detail";
                    }
                }

                catch (Exception e)
                {

                    return e.Message;


                }
            }

            EmptyCart(CardID);
            return "";

        }
        
        public IEnumerable<T> GetClaimDetails<T>(string CartId)
        {
            string sql = " Select d.RecordId,ROW_NUMBER() OVER(ORDER BY  d.RecordId ASC) AS SNo," +
                         " d.Group_Name, RequiredAccess=case when d.RequiredAccess = 'R' then 'Read Only' " +
                         " When d.RequiredAccess='F' then 'Full Access' end,d.Remarks,d.Reason,d.ProcessOwner from SD_Cart as d Where CartId='" + CartId+ "'";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }


        
         public IEnumerable<T> GetRequestByGuid<T>(string CartId)
        {
            string sql = " Select * from SD_GroupRequest Where Cartid='" + CartId +"'";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }

        public bool IsGroupAlreadySelected(string groupName, string guid)
        {

            string sql = " Select * from SD_Cart where Group_Name='" + groupName + "' And CartId='" + guid +"'";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query(sql).ToArray();
                 if (obj.Length > 0)
                {
                    return true;
                }
                return false;
            }

        }


        public IEnumerable<T> ShareFolderProgress<T>(string username)
        {
            string sql = " Select ID,RefNo,ProcessOwner," +
                         " CreatedOn  from SD_GroupRequest where Createdby='" + username + "' and status=0";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }

        public IEnumerable<T> ShareFolderPending<T>(string username )
        {
            string sql = "Select ID,RefNo,EmpCode,Name,Department,Position,ProjectCode,Project,CreatedOn from SD_GroupRequest where ProcessOwnerLoginID='" + username +"' and status=0";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }

        public IEnumerable<T> ApprovedShareFolderRequest<T>()
        {
            string sql = "Select ID,RefNo,EmpCode,Name,Department,Position,ProjectCode,Project,CreatedOn,ApprovedOn,ProcessOwner from SD_GroupRequest where  status=-1 and Archive=0";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }
        public IEnumerable<T> MyFolders<T>(string UserName)
        {

            
            string sql = " Select ID,RefNo,EmpCode,Name,Department,Position,ProjectCode,Project,CreatedOn,Createdby,ApprovedOn,ProcessOwner,AssystNo, " +
                         " Status=case when Status=-1 then 'Approved' when Status=100 then 'Rejected' end  from SD_GroupRequest where Createdby='" + UserName + "' And Status in (-1, 100)";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj =  connection.Query<T>(sql);
                return obj.ToList();
            }

        }

        public IEnumerable<T> AllFolders<T>()
        {


            string sql = " Select ID,RefNo,EmpCode,Name,Department,Position,ProjectCode,Project,CreatedOn,Createdby,ApprovedOn,ProcessOwner,AssystNo,IsReview,ProcessOwnerLoginID, " +
                         " Status=case when Status=-1 then 'Approved' when Status=0 then 'Pending' when Status=100 then 'Rejected' end  from SD_GroupRequest where Status in (0,-1, 100)";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql);
                return obj.ToList();
            }

        }

        


        public T ViewRequest<T>(int TransactionID)
        {
            string sql = " Select a.*,b.Group_Name ,b.Remarks,b.Reason,RequiredAccess=case when  b.RequiredAccess = 'R' then 'Read Only'  when b.RequiredAccess='F' then 'Full Access' end " +
                         " FROM SD_GroupRequest  a " +
                         " Inner join SD_GroupReqDetail b on a.ID = b.RequestID " +
                         " Where a.ID=" + TransactionID;



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;
            }

        }

        public IEnumerable<T> ViewRequestDetail<T>(int TransactionID)
        {
            string sql = " Select ROW_NUMBER() OVER(ORDER BY  d.Id ASC) AS SNo," +
                         " d.Group_Name, RequiredAccess=case when   d.RequiredAccess = 'R' then 'Read Only' " +
                         " when d.RequiredAccess='F' then 'Full Access' end, " +
                         " d.Remarks,d.Reason from SD_GroupReqDetail as d where RequestID=" + TransactionID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }

        public async Task<bool> SubmitForApproval(int ID,string Remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_GroupRequest Set Status=-1,ApprovedOn='" + DateTime.Now + "',ApprovedRemarks='" + Remarks + "' Where ID=@RecordID", new { RecordID = ID });
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID", new { RecordID = ID });


                }
                catch (Exception e)
                {

                    string tes=e.Message;


                }
                return true;

            }
        }

        public async Task<bool> ConfirmReview(int ID,string Access)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_GroupReqDetail Set RequiredAccess='" + Access + "'  Where RequestID=@RecordID", new { RecordID = ID });

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }

        public async Task<bool> ConfirmReviewIT(int ID, string Access,string Folder,string ProcessOwner, string ProcessOwnerLoginID, string ProcessOwnerEmail)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_GroupRequest Set ProcessOwner='" + ProcessOwner + "',ProcessOwnerLoginID='" + ProcessOwnerLoginID + "',ProcessOwnerEmail='" + ProcessOwnerEmail + "'  Where ID=@RecordID", new { RecordID = ID });
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_GroupReqDetail Set Group_Name='" + Folder + "' Where RequestID=@RecordID", new { RecordID = ID });
                    var affectedrows2 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID", new { RecordID = ID });

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }

        


        public async Task<bool> RejectForm(int ID,string Remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_GroupRequest Set Status=100,RejectedOn='" + DateTime.Now + "',RejectedRemarks='"+ Remarks +"' Where ID=@RecordID", new { RecordID = ID });
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID", new { RecordID = ID });

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }

        public async Task<bool> LogEmail(int TransactionID, string GUID,string DocCode)
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


        
       public async Task<bool> LogEmailCancel(int TransactionID, string DocCode)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {

                    string sql = " Update SD_ApplicationEmailLog Set Status='X' Where DocCode='" + DocCode+"' And Status='P' And TransactionID =" + TransactionID;

                    await connection.ExecuteAsync(sql).ConfigureAwait(false);

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }

        public async Task<int> GetToken(string mGUID,string DocCode)
        {

            string sql = "Select  TransactionID from SD_ApplicationEmailLog Where GUID= '" + mGUID + "' And Status='P' And DocCode='" + DocCode + "'";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = await connection.QueryAsync<int>(sql).ConfigureAwait(false);
                return obj.SingleOrDefault();
            }

        }


        public async Task<int> DeleteDetailRecord(int RecordID)
        {

           
            string sql = " Delete from SD_Cart where RecordID=" + RecordID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();
              
                await connection.ExecuteAsync(sql);
               
                return 1;
            }

        }

        public async Task<int> ArchiveRecord(string AssetsNo,int RecordID)
        {

            string sql = " Update SD_GroupRequest Set Archive=1,AssystNo='" + AssetsNo +"'  where ID=" + RecordID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();

                await connection.ExecuteAsync(sql);

                return 1;
            }

        }

        public async Task<bool> PostDelegateRequest(int ID, string ProcessOwnerLoginID, string Remarks)
        {

            string sql = " Update SD_GroupRequest Set IsReview=1,ReviewRemarks='" + Remarks + "',ProcessOwnerLoginID='" + ProcessOwnerLoginID + "',ReviewedOn='" + DateTime.Now + "'  where ID=" + ID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();

                await connection.ExecuteAsync(sql);

                return true;
            }

        }



        public  IEnumerable<T> FolderProcessCount<T>()
        {
            string sql = " Select Project, Count(*) as Total From SD_GroupRequest Where Status=-1 group by Project order by Project";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj =  connection.Query<T>(sql);
                return obj.ToList();
            }
        }
        public async Task<T>GeneratePDF<T>(int RecordID)
        {


            string sql = " Select a.RefNo,a.EmpCode,a.Name,a.Position,a.ProjectCode,a.ProcessOwner,a.ApprovedOn,a.CreatedOn,a.ApprovedRemarks,IsReview,ReviewRemarks,ProcessOwnerLoginID, " +
                         " b.Group_Name,RequiredAccess=case when b.RequiredAccess = 'R' then 'Read Only' " +
                         " When b.RequiredAccess='F' then 'Full Access' end,b.Remarks,b.Reason" +
                         " From SD_GroupRequest a" +
                         " Inner join SD_GroupReqDetail b on a.id = b.RequestID " +
                         " Where a.id=" + RecordID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = await connection.QueryAsync<T>(sql);
                return obj.SingleOrDefault();
                
            }

        }

        
        public void EmptyCart(string shoppingCartId)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var affectedrows = connection.Execute("DELETE FROM SD_Cart WHERE CartId = @CartID", new { CartID = shoppingCartId });


            }

        }




    }
}
