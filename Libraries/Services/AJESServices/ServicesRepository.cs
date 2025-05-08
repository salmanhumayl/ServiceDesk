﻿using AJCCFM.Core;
using AJCCFM.Core.Domain.SD_VPN;
using Core.Domain;
using Dapper;
using Services.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.AJESServices
{
    public class ServicesRepository
    {

        public IEnumerable<T> GetRequestByGuid<T>(string CartId)
        {
            string sql = " Select * from SD_VPN Where Cartid='" + CartId + "'";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }
        public string SaveClaim(CartService ClaimData)
        {

            string sql = "";
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();


                    sql = " Insert into SD_CartServices " +
                      " (CartId,ServiceName,ServiceCode,Remarks,Path) " +
                      " Values (@CartId,@ServiceName,@ServiceCode,@Remarks,@Path)" +
                        "Select Cast(SCOPE_IDENTITY() AS int)";

                    connection.Query(sql, new
                    {
                        ClaimData.CartId,
                        ClaimData.ServiceName,
                        ClaimData.ServiceCode,
                        ClaimData.Remarks,
                        ClaimData.Path
                        
                    }).SingleOrDefault();


                }

                catch (Exception e)
                {

                    return e.Message;


                }
            }

            return "";

        }


        public string SubmitServiceRequest(EmployeeDetail model, string CardID,string SubmittedTo, string EmpEmailAddress, string SubmittToAddress)
        {
            string[] UserDetail = new string[2];
            string sql = "";
            int RequestID;
            string RefNo = "";
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                if (model.EmpCode == null)
                {
                    return "Employee Code is missing...";
                }
                string Createdby = System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", "");
             

                try
                {
                    connection.Open();

                    sql = "Select * from SD_CartServices where CartId='" + CardID + "'";

                    var cartitems = connection.Query<CartService>(sql).ToList();

                    if (cartitems.Count > 0)

                    {
                        foreach (var items in cartitems)
                        {
                           // if (items.TableName == "SD_VPN" || items.TableName == "SD_IA" || items.TableName == "SD_USB" || items.TableName == "SD_RWA")
                            //{
                                RefNo = Common.GetDocumentNumberService("GR");
                            string Remarks = items.Remarks;
                            string Path = items.Path;
                                sql = " Insert into SD_VPN" +
                                      " (RefNo,Status,Name,EmpCode,Position,ProjectCode,Project,Createdby,Email,Cartid,SubmittedTo,ServiceName,ServiceCode,Remarks,Path,SubmittedToEmail) " +
                                      " Values (@RefNo,0,@EmpName,@EmpCode,@Position,@ProjectCode,@Project,@Createdby,@EmpEmailAddress,@CardID,@SubmittedTo,'" + items.ServiceName + "' ,'" + items.ServiceCode + "',@Remarks,@Path,'" + SubmittToAddress + "')" +
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
                                    SubmittedTo,
                                    Remarks,
                                    Path,
                                    CardID

                                }).SingleOrDefault();

                                AddLogHistory(RequestID, "S", Createdby.Trim(), SubmittedTo, "", items.ServiceCode);


                            //}
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


        public T ViewRequest<T>(int TransactionID)
        {
            string sql = " Select a.* FROM SD_VPN  a " +
                         " Where a.ID=" + TransactionID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;
            }

        }


        public async Task<bool> SubmitForApproval(ServiceRequestModel_SD_VPN model,string remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
               
                    if (model.Status !=-1)
                    {
                        var affectedrows = await connection.ExecuteAsync("Update SD_VPN Set AccessType='" + model.AccessType + "',Status=" + model.Status + " ,Others='" + model.Others + "',SubmittedTo='" + model.Submitedto + "' Where ID=@RecordID", new { RecordID = model.ID });

                        AddLogHistory(model.ID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), model.Submitedto, remarks, model.ServiceCode);
                    }
                    else
                    {
                        var affectedrows = await connection.ExecuteAsync("Update SD_VPN Set Status=" + model.Status + "  Where ID=@RecordID", new { RecordID = model.ID });

                        AddLogHistory(model.ID, "A", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""), "", remarks, model.ServiceCode);
                    }
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where TransactionID=@RecordID And DocCode='S' ", new { RecordID = model.ID });

                }
                catch (Exception e)
                {

                    string tes = e.Message;


                }
                return true;

            }
        }

        public async Task<bool> RejectForm(int ID, string Remarks,string ServiceCode)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    var affectedrows = await connection.ExecuteAsync("Update SD_VPN Set Status=-100  Where ID=@RecordID", new { RecordID = ID });
                    var affectedrows1 = await connection.ExecuteAsync("Update SD_ApplicationEmailLog Set Status='C' Where  DocCode='S' And TransactionID=@RecordID", new { RecordID = ID });

                    AddLogHistory(ID, "R", System.Web.HttpContext.Current.User.Identity.Name.Replace("AJES\\", ""),"", Remarks, ServiceCode);
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

            string sql = " Update SD_VPN Set Archive=1,AssystNo='" + AssetsNo +"' where ID=" + RecordID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();

                await connection.ExecuteAsync(sql);

                return 1;
            }

        }
        

             public IEnumerable<T> ServiceProcessCount<T>()
        {
            string sql = " Select Count(*) as Total, ServiceName,Project From SD_VPN Where Status=-1 group by Project,ServiceName order by Project";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql);
                return obj.ToList();
            }
                    }

        public IEnumerable<T> MyService<T>(string UserName)
        {
            string sql = " Select id, RefNo, ServiceName, EmpCode,Name,CreatedOn,Createdby,ServiceCode," +
                         " StatusDescription =case when Status = -1 then 'Approved' when Status = -100 then 'Rejected' end " +
                         " From SD_VPN Where Createdby = '" + UserName + "' And status in (-1, -100)";
 


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql);
                return obj.ToList();
            }

        }

        public IEnumerable<T> AllService<T>()
        {
            string sql = " Select id, RefNo, ServiceName, EmpCode,Name,CreatedOn,Createdby,ServiceCode,AssystNo,Project,status," +
                         " StatusDescription =case when Status = -1 then 'Approved' when Status = -100 then 'Rejected'" +
                         " when Status = 0 then 'Pending' when Status = 1 then 'Pending' end " +
                         " From SD_VPN Where status in (-1, -100,0,1)";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql);
                return obj.ToList();
            }

        }
        public async Task<IEnumerable> GenerateServicePDF<T>(int RecordID,string ServiceCode)
        {
                string sql = " Select a.*,Users.FullName as ApprovedBy ,b.ProcessOn as ApprovedOn FROM SD_VPN  a " +
                         " Inner join SD_WorkFlowLogHistory b on a.id = b.TransactionID " +
                         " Inner Join Users on b.ProcessBy = users.LoginName " +
                         " where a.id = "+ RecordID +" and b.Doc_Code = '" + ServiceCode +"' and b.Status = 'A'";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = await connection.QueryAsync<T>(sql);
                return obj.ToList();

            }

        }
        public async Task<int> DeleteDetailRecord(int RecordID)
        {


            string sql = " Delete from SD_CartServices where RecordID=" + RecordID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();

                await connection.ExecuteAsync(sql);

                return 1;
            }

        }
        public IEnumerable<T> ApprovedServices<T>()
        {

            string sql = "Select id, RefNo, ServiceName,ServiceCode, Name,CreatedOn,Status from SD_VPN Where  status =-1 and Archive=0";
                       

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


        public bool IsServiceAlreadySelected(string groupName, string guid)
        {

            string sql = " Select * from SD_CartServices where ServiceName='" + groupName + "' And CartId='" + guid + "'";

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


        public IEnumerable<T> GetServiceDetails<T>(string CartId)
        {
            string sql = " Select d.RecordId,ROW_NUMBER() OVER(ORDER BY  d.RecordId ASC) AS SNo," +
                         " d.ServiceName,d.Remarks,d.Path from SD_CartServices as d Where CartId='" + CartId + "'";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }



        public IEnumerable<T> ServicePending<T>(string username)
        {
            //string sql = "Select id, RefNo, 'VPN Access' as Service, Name,CreatedOn,Status from SD_VPN Where Submittedto='" + username + "' And  status >=0 " +
            //             " Union All " +
            //             " Select id, RefNo,'Internet Access' as Service, Name,CreatedOn,Status from SD_IA " +
            //             " Union All" +
            //             " Select id, RefNo,'USB Access' as Service, Name,CreatedOn,Status from SD_USB " +
            //             " Union All " +
            //             " Select id, RefNo,'Restricted website access' as Service, Name,CreatedOn,Status from SD_RWA";


            string sql = "Select id, RefNo, ServiceName, Name,CreatedOn,Status,ServiceCode from SD_VPN Where Submittedto='" + username + "' And  status >=0 ";
                     


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }


        public IEnumerable<T> ServiceProgress<T>(string username)
        {
        
            string sql = "Select id, RefNo, ServiceName, Name,CreatedOn,ServiceCode from SD_VPN Where Createdby='" + username + "' And  status >=0 ";

                        


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }
        public void EmptyCart(string shoppingCartId)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var affectedrows = connection.Execute("DELETE FROM SD_CartServices WHERE CartId = @CartID", new { CartID = shoppingCartId });


            }

        }

    }
}
