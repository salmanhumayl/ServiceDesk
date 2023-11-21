using Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using Services.Claim.ClaimViewModel;
using System.Data;
using Services.JDOInterface;
using Services.CashRequisitionForm;
using System.Web.ModelBinding;
using Services.BaseSetup;
using Services.Helper;

using Services.SecurityRBAC;
using System.Globalization;

namespace Services.Claim
{
    public class ClaimRepository : BaseModule
    {

       
        
        public ClaimRepository()
        {

            UserId = Convert.ToInt16(System.Web.HttpContext.Current.Session["UserID"]);
            CompanyID = Convert.ToInt16(System.Web.HttpContext.Current.Session["CompanyID"]);
            UserName = System.Web.HttpContext.Current.Session["Name"].ToString();
            BusniesUnit = Convert.ToInt16(System.Web.HttpContext.Current.Session["BUID"]);
            EmpCode = System.Web.HttpContext.Current.Session["AccountNo"].ToString();
        }



        public ICashRequisition _CashRequisitionService;

        string sql = "";
        public void AddToClaim(Cart Carts, string shoppingCartId)
        {


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                if (Carts != null && Carts.ClaimDetailID == 0) //Add Item
                {
                    sql = " INSERT INTO CFM_Cart(CartID,CompanyID,BuID,CategoryID,InvoiceNo,invoiceDate,EmpCode,EmpName,PaymentType,CashPaid,TotalAmountbeforeVat,VatAmount,Claimdate,Remarks,DateCreated) " +
                            " VALUES ('" + shoppingCartId + "',@CompanyID,@BuID,@CategoryID,@InvoiceNo,@InvoiceDate,@EmpCode,@EmpName,@PaymentType,@CashPaid,@TotalAmountbeforeVat,@VatAmount,@Claimdate,@Remarks,'" + DateTime.Now + "')" +
                            " Select Cast(SCOPE_IDENTITY() AS int)";

                    int ID = connection.Query<int>(sql, new
                    {
                        Carts.CompanyID,
                        Carts.BuID,
                        Carts.CategoryID,
                        Carts.InvoiceNo,
                        Carts.InvoiceDate,
                        Carts.EmpCode,
                        Carts.EmpName,
                        Carts.LPOType,
                        Carts.CashPaid,
                        Carts.TotalAmountbeforeVat,
                        Carts.VatAmount,
                        Carts.Claimdate,
                        Carts.Remarks
                    }).SingleOrDefault();

                }
                else //Update Item 
                {
                    sql = "update CFM_Cart Set CompanyID=@CompanyID,BuID=@BuID,CategoryID=@CategoryID,InvoiceNo=@InvoiceNo,CashPaid=@CashPaid,TotalAmountbeforeVat=@TotalAmountbeforeVat,VatAmount=@VatAmount,Remarks=@Remarks Where RecordID=@RecordID";
                    connection.Execute(sql, new
                    {
                        Carts.CompanyID,
                        Carts.BuID,
                        Carts.CategoryID,
                        Carts.InvoiceNo,
                        Carts.TotalAmountbeforeVat,
                        Carts.VatAmount,
                        Carts.CashPaid,
                        Carts.Remarks,
                        Carts.ClaimDetailID
                    });
                }
            }

        }


        public int AddClaim(Cart Carts)
        {


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                string RefNo = "";
                int ClaimID = 0;
                string Paid = "CB";
                if (Carts.EmpCode != null)
                {
                    Paid = "SB";
                }
                if (Paid == "CB")
                {
                    Carts.EmpName = UserName;
                    Carts.EmpCode = EmpCode;
                }
                if (Carts.ClaimType == "P")
                {
                    RefNo = Common.GetDocumentNumber(Carts.CompanyID, "PP");
                }
                else
                {
                    RefNo = Common.GetDocumentNumber(Carts.CompanyID, "CC");
                }

                //   Random objRandom = new Random();
                // string RefNo1 = "CCF-01" + objRandom.Next();

                if (Carts != null && Carts.ClaimID == 0) //Add Item
                {
                    sql = " INSERT INTO CFM_Claims(RefNo,CompanyID,BuID,Claimdate,EmpCode,EmpName,Paid,CreatedBy,ClaimType) " +
                            " VALUES (@RefNo,@CompanyID,@BuID,@Claimdate,@EmpCode,@EmpName,'" + Paid + "'," + UserId + ",@ClaimType)" +
                            " Select Cast(SCOPE_IDENTITY() AS int)";

                    ClaimID = connection.Query<int>(sql, new
                    {
                        RefNo,
                        Carts.CompanyID,
                        Carts.BuID,
                        Carts.Claimdate,
                        Carts.EmpCode,
                        Carts.EmpName,
                        Carts.ClaimType

                    }).SingleOrDefault();

                }
                else //Update Item 
                {
                    sql = "update CFM_Cart Set CompanyID=@CompanyID,BuID=@BuID,CategoryID=@CategoryID,InvoiceNo=@InvoiceNo,CashPaid=@CashPaid,TotalAmountbeforeVat=@TotalAmountbeforeVat,VatAmount=@VatAmount,Remarks=@Remarks Where RecordID=@RecordID";
                    connection.Execute(sql, new
                    {
                        Carts.CompanyID,
                        Carts.BuID,
                        Carts.CategoryID,
                        Carts.InvoiceNo,
                        Carts.TotalAmountbeforeVat,
                        Carts.VatAmount,
                        Carts.CashPaid,
                        Carts.Remarks,
                        Carts.ClaimDetailID
                    });
                }

                return ClaimID;
            }

        }


        public List<ViewModelCart> GetCartItemsDetail(string shoppingCartId)
        {
            sql = " Select  ROW_NUMBER() OVER(ORDER BY a.Name ASC) AS SNo,a.Name as BU ,c.Name as Category ,b.InvoiceNo, b.RecordID,'cart' as mode,b.CashPaid  From CFM_tblCompany a , CFM_cart b ,CFM_tblCategory c  " +
                        " where a.CompanyID = b.BUID " +
                        " And b.categoryid = c.id " +
                        " And b.CartID = @CardID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<ViewModelCart>(sql, new { CardID = shoppingCartId }).ToList();
                return cartItems;
            }
        }


        public Cart GetCartItemsByID(int RecordID)
        {
            sql = " Select * from CFM_Cart where RecordId = @ID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.QuerySingleOrDefault<Cart>(sql, new { ID = RecordID });
                return cartItems;
            }

        }
        public Cart GetClaimItemsByID(int RecordID)
        {
            sql = " Select * from CFM_Claims a,CFM_ClaimDetails b  " +
                        " Where  a.ClaimID = b.ClaimID and b.ClaimDetailID =@ID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.QuerySingleOrDefault<Cart>(sql, new { ID = RecordID });
                return cartItems;
            }

        }



        public string AutoGenerateClaim(int ID, string Reqno, int CompanyID, int BuID)
        {
            _CashRequisitionService = new CashRequisitionService();
            int ClaimID = 0;
            string sql;
            //  Random objRandom = new Random();

            // string RefNo = "CCF-01" + objRandom.Next();

            string RefNo = Common.GetDocumentNumber(CompanyID, "CC");
            // get Requestor Name, Code 
           var objReq=_CashRequisitionService.GetRequestorName<RequestorDetail>(ID);

            Double nBalance;
            decimal mSumCashPaid = 0;
            decimal CashPaid = 0;
            IDbConnection connection;
            string Message = "";
            //GetBalance 
            try
            {
                sql = "Select IsNull(Sum(Balance),0) as Balance from CFM_TH_CashRequisitionForm where ID=" + ID + " AND Balance > 0";
                using (var mconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {
                    System.Data.IDataReader Ireader = mconnection.ExecuteReader(sql);
                    Ireader.Read();

                    var obj = Ireader["Balance"];
                    nBalance = (Double)obj;
                }

                if (nBalance > 0)
                {
                    sql = " Insert into CFM_Claims(CompanyID, BuID, Refno,EmpCode,EmpName, ClaimDate,ReqNo,lClaimstatusForReq,CashReqHeaderID,status,Paid,CreatedBy) " +
                     " Values("+ CompanyID + ", " + BuID + ", '" + RefNo + "','" + objReq.AccountNo + "','" + objReq.Name + "',getdate(),'" + Reqno + "',2," + ID + ",0,'CG'," + UserId + ")" +
                     " Select Cast(SCOPE_IDENTITY() AS int)";


                    using (connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                    {
                        connection.Open();
                        IDbTransaction transaction = connection.BeginTransaction();


                        ClaimID = connection.Query<int>(sql, null, transaction).SingleOrDefault();


                        var REQLPOItems = _CashRequisitionService.GetRequisitionLPOItem<RequisitionLPOItem>(ID);


                        if (REQLPOItems != null)

                        {

                            foreach (var reqitems in REQLPOItems)
                            {
                                if (reqitems.LPOType == "P")
                                {
                                    var JDInvoices = JDInterface.JDAPInfo<JDAPViewModel>(reqitems.Purchase_Order, reqitems.Doc_Co, "OP");   //Read invoice from JDE in case of LPO
                                    var processInvoices = JDInterface.GetProcessedInvoice<JDAPViewModel>(reqitems.Purchase_Order, reqitems.Doc_Co,"OP"); //Process Invocies

                                    IEnumerable<JDAPViewModel> InvoiceNotProcess = JDInvoices.Except(processInvoices, new InvoiceComparer());
                                    if (InvoiceNotProcess.Count() != 0)
                                    {
                                        CashPaid = JDInvoices.Select(x => x.GrossAmount).Sum();

                                        sql = " Insert into CFM_ClaimDetails(ClaimID,CategoryID,LPOType,JD_Co_No,OP_PO_Type,PO_Num,CashPaid,LPOAmount,TDCashRequisitionFormID) VALUES " +
                                              " (" + ClaimID + ", " + reqitems.CategoryID + ",'" + reqitems.LPOType + "','" + reqitems.Doc_Co + "','OP','" + reqitems.Purchase_Order + "', " + CashPaid + "," + reqitems.Balance + "," + reqitems.id + " ) " +
                                              " Select Cast(SCOPE_IDENTITY() AS int)";

                                        int ClaimDetailID = connection.Query<int>(sql, null, transaction).SingleOrDefault();

                                        //Insert into CFM_ClaimInvoice_Detail

                                        foreach (var JDEItem in InvoiceNotProcess)
                                        {
                                            string sqlInvoiceDetail = " Insert into CFM_ClaimInvoice_Detail " +
                                                " (ClaimDetailID,Calimid,Purchase_Order,Doc_Co,PO_Do,SupplierName,InvoiceNumber,InvoiceDate,GrossAmount) " +
                                                " Values (" + ClaimDetailID + " , " + ClaimID + ",@Purchase_Order,@Doc_Co,'OP',@SupplierName,@InvoiceNumber,@InvoiceDate,@GrossAmount)";


                                            connection.Execute(sqlInvoiceDetail, new
                                            {
                                                JDEItem.Purchase_Order,
                                                JDEItem.Doc_Co,
                                                //JDEItem.PO_Do,
                                                JDEItem.SupplierName,
                                                JDEItem.InvoiceNumber,
                                                JDEItem.InvoiceDate,
                                                JDEItem.GrossAmount
                                            }, transaction);
                                        }

                                        mSumCashPaid = mSumCashPaid + CashPaid;
                                    }
                                    else
                                    {
                                        Message = Message +  "Invoice Not Find In JD.Purchase Order #. " + reqitems.Purchase_Order + " Doc No:" + reqitems.Doc_Co;
                                    }
                                }
                                else
                                {
                                    sql = " Insert into CFM_ClaimDetails(ClaimID,CategoryID,LPOType,CashPaid,LPOAmount,TDCashRequisitionFormID) VALUES " +
                                    " (" + ClaimID + ", " + reqitems.CategoryID + ",'" + reqitems.LPOType + "', " + reqitems.Balance + "," + reqitems.Balance + "," + reqitems.id + " ) " +
                                    " Select Cast(SCOPE_IDENTITY() AS int)";

                                    int ClaimDetailID = connection.Query<int>(sql, null, transaction).SingleOrDefault();


                                   // connection.Execute(sql, null, transaction);
                                    //Get Table Name 
                                     string tableName = GetTableName(reqitems.CategoryID);
                                    // var obj = GetTableSchema(tableName);
                                     var objFectchColumn = GetTableColumn(tableName);
                                 
                                    string[] fetchColumn = new string[objFectchColumn.Count ];
                                 
                                  
                                    if (objFectchColumn.Count > 0)
                                    {
                                        for (var i = 0; i <= objFectchColumn.Count - 1; i++)
                                        {
                                            fetchColumn[i] = objFectchColumn[i].COLUMN_NAME;
                                        }
                                        string getPRData = string.Format("INSERT INTO  " + tableName + " SELECT  " + ClaimDetailID + ",'C',{0} from  " + tableName + " Where __HClaimID=" + reqitems.id + " And Type='R'", string.Join(",", fetchColumn));
                                        connection.Execute(getPRData, null, transaction);
                                    }
                                 
                                    mSumCashPaid += reqitems.Balance;
                                }

                            }
                        }

                        if (mSumCashPaid > 0)
                        {
                            sql = " Insert into CFM_CashRequistionClaims(CashReqHeaderID,ClaimID,ClaimAmount) " +
                                   " Values(" + ID + "," + ClaimID + "," + mSumCashPaid + ")";

                            connection.Execute(sql, null, transaction);

                            //Update TH Requisition to record latest Status of claim and its status
                            sql = " update CFM_TH_CashRequisitionForm  " +
                                 " Set CurrentStatus=2 ,CurrentClaimID=" + ClaimID + " where id =" + ID + " ";

                            connection.Execute(sql, null, transaction);
                            transaction.Commit();
                            connection.Close();
                        }
                        else
                        {
                            transaction.Rollback();
                            connection.Close();
                        }


                    }
                }

            }
            catch (Exception e)
            {

                return e.Message;


            }
            return Message;

        }



        public string SettleInAccount(int ID, string Reqno, int CompanyID, int BuID)
        {
            _CashRequisitionService = new CashRequisitionService();
            int ClaimID = 0;
            string sql;
          //  Random objRandom = new Random();
            string RefNo = Common.GetDocumentNumber(CompanyID, "CC");
            Double nBalance;
            IDbConnection connection;
            string Message = "";
            //GetBalance 
            try
            {
                sql = "Select IsNull(Sum(Balance),0) as Balance from CFM_TD_CashRequisitionForm where  CashReqHeaderID=" + ID + " AND Balance > 0";
                using (var mconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {
                    System.Data.IDataReader Ireader = mconnection.ExecuteReader(sql);
                    Ireader.Read();

                    var obj = Ireader["Balance"];
                    nBalance = (Double)obj;
                }

                if (nBalance > 0)
                {
                    sql = " Insert into CFM_Claims(CompanyID, BuID, Refno, ClaimDate,ReqNo,lClaimstatusForReq,CashReqHeaderID,status,Paid,CreatedBy,lSettleInAccount) " +
                     " Values(" + CompanyID + ", " + BuID + ", '" + RefNo + "',getdate(),'" + Reqno + "',2," + ID + ",0,'CG'," + UserId + ",1)" +
                     " Select Cast(SCOPE_IDENTITY() AS int)";


                    using (connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                    {
                        connection.Open();
                        IDbTransaction transaction = connection.BeginTransaction();


                        ClaimID = connection.Query<int>(sql, null, transaction).SingleOrDefault();

                        sql = " Insert into CFM_ClaimDetails(ClaimID,CategoryID,LPOType,CashPaid) VALUES " +
                        " (" + ClaimID + ",13,'P', " + nBalance + ") " +
                        " Select Cast(SCOPE_IDENTITY() AS int)";

                        int ClaimDetailID = connection.Query<int>(sql, null, transaction).SingleOrDefault();


                        sql = " Insert into CFM_CashRequistionClaims(CashReqHeaderID,ClaimID,ClaimAmount) " +
                               " Values(" + ID + "," + ClaimID + "," + nBalance + ")";

                        connection.Execute(sql, null, transaction);

                        //Update TH Requisition to record latest Status of claim and its status
                        sql = " update CFM_TH_CashRequisitionForm  " +
                             " Set CurrentStatus=2 ,CurrentClaimID=" + ClaimID + " where id =" + ID + " ";

                        connection.Execute(sql, null, transaction);
                        transaction.Commit();
                        connection.Close();
                    }

                }

            }
            catch (Exception e)
            {
                return e.Message;
             }
            return Message;
        }

        public dynamic GetTableColumn(string TableName)
        {
            string DBName = ConfigurationManager.AppSettings["DBName"].ToString();
            string sql;
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

             
                    sql = " SELECT COLUMN_NAME FROM "+ DBName +".INFORMATION_SCHEMA.COLUMNS" +
                               " WHERE TABLE_NAME = N'" + TableName + "' AND COLUMN_NAME NOT IN ('ID','Type','__HClaimID') ";
               

                var obj = db.Query(sql).ToList();
                db.Close();
                return obj;

            }

        }

        public dynamic GetTableSchema(string TableName)
        {
            string DBName = ConfigurationManager.AppSettings["DBName"].ToString();
            string sql;
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

              
                    sql = " SELECT COLUMN_NAME FROM " + DBName + ".INFORMATION_SCHEMA.COLUMNS" +
                               " WHERE TABLE_NAME = N'" + TableName + "' AND COLUMN_NAME NOT IN ('ID','Type') ";
                

                var obj = db.Query(sql).ToList();
                db.Close();
                return obj;

            }

        }

        public  String GetTableName(int CategorID)
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


        public List<Cart> GetCartItems(string shoppingCartId)
        {
            string sql = " Select * from Cart where CartID = @CartID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<Cart>(sql, new { CartID = shoppingCartId }).ToList();
                return cartItems;
            }

        }


        public string SaveClaim(string InsertSql, Cart ClaimData, List<JDAPViewModel> LPODetail)
        {
            decimal ConversionRate = 1;
            int mClaimDetailID = 0;
            string sqlInvoiceDetail = "";
            string JD_Co_No;
            IDbTransaction transaction = null;
            DateTime InvoiceDate;

            if (ClaimData.Currency != "AED")
            {
                ConversionRate = JDInterface.GetConversionRate(ClaimData.Currency);
            }


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    if (ClaimData.LPOType == "P")
                    {
                        JD_Co_No = string.Format("{0,5:00000}", Convert.ToInt16(ClaimData.JD_Co_No));
                    }
                    else
                    {
                        JD_Co_No = ClaimData.JD_Co_No;
                    }

                    if (ClaimData.ClaimDetailID == 0) //Add Item
                    {
                        ClaimData.CashPaid = ClaimData.CashPaid * ConversionRate;

                        InvoiceDate = DateTime.ParseExact(ClaimData.InvoiceDate, "dd/MM/yyyy",CultureInfo.InvariantCulture);

                        sql = " Insert into CFM_ClaimDetails " +
                          " (ClaimID,CategoryID,LPOType,InvoiceNo,InvoiceDate,Vendor,JD_Co_No,OP_PO_Type,PO_Num,CashPaid,TotalAmountbeforeVat,VatAmount,Remarks,LPOAmount,TDCashRequisitionFormID,JDEObject,JDESub,SupplierName,CostCode,Currency,ConversionRate) " +
                          " Values (@ClaimID,@CategoryID,@LPOType,@InvoiceNo,@InvoiceDate,@Vendor,@JD_Co_No,@OP_PO_Type,@PO_Num,@CashPaid,@TotalAmountbeforeVat,@VatAmount,@Remarks,@LPOAmount,@TDCashRequisitionID,@JDEObject,@JDESub,@SupplierName,@CostCode,@Currency," + ConversionRate + ")" +
                            "Select Cast(SCOPE_IDENTITY() AS int)";

                        mClaimDetailID = connection.Query<int>(sql, new
                        {
                            ClaimData.ClaimID,
                            ClaimData.CategoryID,
                            ClaimData.LPOType,
                            ClaimData.InvoiceNo,
                            InvoiceDate,
                            ClaimData.Vendor,
                            JD_Co_No,
                            ClaimData.OP_PO_Type,
                            ClaimData.PO_Num,
                            ClaimData.CashPaid,
                            ClaimData.TotalAmountbeforeVat,
                            ClaimData.VatAmount,
                            ClaimData.Remarks,
                            ClaimData.LPOAmount,
                            ClaimData.TDCashRequisitionID,
                            ClaimData.JDEObject,
                            ClaimData.JDESub,
                            ClaimData.SupplierName,
                            ClaimData.CostCode,
                            ClaimData.Currency
                        }, transaction).SingleOrDefault();
                    }
                    else //Edit 
                    {
                        if (ClaimData.LineItemStatus == 1)
                        {
                            ClaimData.LineItemStatus = 2;
                        }
                        InvoiceDate = DateTime.ParseExact(ClaimData.InvoiceDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        mClaimDetailID = ClaimData.ClaimDetailID;

                        ClaimData.CashPaid = ClaimData.CashPaid * ConversionRate;


                        sql = " update CFM_ClaimDetails Set CategoryID=@CategoryID,LPOType=@LPOType,InvoiceNo=@InvoiceNo, " +
                              " InvoiceDate=@InvoiceDate,Vendor=@Vendor,JD_Co_No=@JD_Co_No,OP_PO_Type=@OP_PO_Type," +
                              " PO_Num=@PO_Num,CashPaid=@CashPaid,TotalAmountbeforeVat=@TotalAmountbeforeVat,SupplierName=@SupplierName,CostCode=@CostCode," +
                              " VatAmount=@VatAmount,Remarks=@Remarks,LineItemStatus=@LineItemStatus,Currency=@Currency,ConversionRate=" + ConversionRate + " Where ClaimDetailID=@ClaimDetailID";
                        connection.Execute(sql, new
                        {

                            ClaimData.CategoryID,
                            ClaimData.LPOType,
                            ClaimData.InvoiceNo,
                            InvoiceDate,
                            ClaimData.Vendor,
                            ClaimData.JD_Co_No,
                            ClaimData.OP_PO_Type,
                            ClaimData.PO_Num,
                            ClaimData.CashPaid,
                            ClaimData.TotalAmountbeforeVat,
                            ClaimData.VatAmount,
                            ClaimData.Remarks,
                            ClaimData.LineItemStatus,
                            ClaimData.SupplierName,
                            ClaimData.CostCode,
                            ClaimData.Currency,
                            ClaimData.ClaimDetailID
                        }, transaction);
                    }




                    if (ClaimData.LPOType == "N" && !string.IsNullOrEmpty(InsertSql))
                    {
                        InsertSql = InsertSql.Replace("99XX", mClaimDetailID.ToString());

                        connection.Execute(InsertSql, null, transaction);
                    }

                    if (ClaimData.LPOType == "P")
                    {
                        //Add invoices
                        foreach (var item in LPODetail)
                        {

                            var processInvoices = JDInterface.GetProcessedInvoiceByid<JDAPViewModel>(item.ID); //Process Invocies
                            if (processInvoices == null)
                            {
                                sqlInvoiceDetail = " Insert into CFM_ClaimInvoice_Detail " +
                                      " (ClaimDetailID,Calimid,Purchase_Order,Doc_Co,PO_Do,SupplierName,InvoiceNumber,InvoiceDate,GrossAmount) " +
                                      " Values (" + mClaimDetailID + " , " + ClaimData.ClaimID + ",@Purchase_Order,@Doc_Co,'OP',@SupplierName,@InvoiceNumber,@InvoiceDate,@GrossAmount)";

                                connection.Execute(sqlInvoiceDetail, new
                                {
                                    item.Purchase_Order,
                                    item.Doc_Co,
                                    //item.PO_Do,
                                    item.SupplierName,
                                    item.InvoiceNumber,
                                    item.InvoiceDate,
                                    item.GrossAmount
                                }, transaction);
                            }
                        }

                    }

                  
                    transaction.Commit();
                }

                catch (Exception e)
                {
                    transaction.Rollback();
                    return e.Message;


                }
            }

            return "";

        }



        public string CreateClaim(List<Cart> cartItems, string shoppingCartId)
        {
            Int32 PaidToStaff = 2;
            if (cartItems[0].EmpCode != null)
            {
                PaidToStaff = 0;
            }

            //First Add Header 
            int ClaimID = 0;
            Random objRandom = new Random();
            string RefNo = "CCF-01" + objRandom.Next();
            sql = " INSERT INTO CFM_Claims(CompanyID,BuID,PaymentType,EmpCode,EmpName,RefNo,Claimdate,CreatedBy,lPaidToStaff)VALUES (@CompanyID,@BuID,@PaymentType,@EmpCode,@EmpName,@RefNo,@Claimdate,1,@PaidToStaff) " +
                                "Select Cast(SCOPE_IDENTITY() AS int)";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();
                IDbTransaction transaction = connection.BeginTransaction();

                ClaimID = connection.Query<int>(sql, new
                {
                    cartItems[0].CompanyID,
                    cartItems[0].BuID,
                    RefNo,
                    cartItems[0].LPOType,
                    cartItems[0].EmpCode,
                    cartItems[0].EmpName,
                    PaidToStaff,
                    cartItems[0].Claimdate
                }, transaction).SingleOrDefault();




                foreach (var cartItem in cartItems)
                {
                    sql = " Insert into CFM_ClaimDetails (ClaimID,CategoryID,InvoiceNo,InvoiceDate,CashPaid,TotalAmountbeforeVat,VatAmount,Remarks) " +
                             " Values (" + ClaimID + ",@CategoryID,@InvoiceNo,@InvoiceDate,@CashPaid,@TotalAmountbeforeVat,@VatAmount,@Remarks)";


                    connection.Execute(sql, new
                    {
                        cartItem.CategoryID,
                        cartItem.InvoiceNo,
                        cartItem.InvoiceDate,
                        cartItem.TotalAmountbeforeVat,
                        cartItem.VatAmount,
                        cartItem.CashPaid,
                        cartItem.Remarks
                    }, transaction);
                }
                transaction.Commit();
            }
            EmptyCart(shoppingCartId);
            return RefNo + "," + ClaimID;

        }


        public void EmptyCart(string shoppingCartId)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var affectedrows = connection.Execute("DELETE FROM Cart WHERE CartID = @CartID", new { CartID = shoppingCartId });


            }

        }
        
        public ViewModelSearchCliam TrackClaimPost(string TrackRefNo)
        {

            sql = " Select a.*, Claimstatus=case when status=0 then 'Not Submited' " +
                  " When status = -1 then 'Approved'" +
                  " When status = -2 then 'Rejected' " +
                  " When status = -3 then 'Paid' " +
                  " When Status= -4 then 'Batching' " +
                  " When Status= -5 then 'Batching In Aprroval' " +
                  " When Status > 0 then 'Approval Process'" +
                  " End From CFM_Claims a where a.CreatedBy=" + UserId + " And a.RefNo = @TrackRefNo";
          
            ViewModelSearchCliam obj;
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                obj = connection.QuerySingleOrDefault<ViewModelSearchCliam>(sql, new { TrackRefNo = TrackRefNo.Trim() });
          
               
            }
            return obj;
        }

        
            public List<UpdatVAT> EditVAT(string ClaimRefNo)
        {
            string sql = "Select a.ClaimID,a.ClaimDetailID,a.InvoiceNo,a.CostCode,a.VatAmount from CFM_ClaimDetails a , CFM_Claims b  " +
                         " WHERE a.ClaimID= b.ClaimID And b.RefNo = '" + ClaimRefNo + "'";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<UpdatVAT>(sql).ToList();
                return cartItems;
            }

        }
        public void UpdateVAT(List<UpdatVAT> model)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                foreach (var item in model)
                {
                    sql = "update CFM_ClaimDetails Set VatAmount=@VatAmount,CostCode=@CostCode,VATUpdateBy=" + UserId + ",VATUpdateOn='" + DateTime.Now.ToString("dd/MMM/yyyy") + "' where ClaimDetailID=@ClaimDetailID";

                    connection.Execute(sql, new
                    {
                        item.VatAmount,
                        item.CostCode,
                        item.ClaimDetailID

                    });
               }
            }
        }

       

      public List<MyClaimList> SiteStatusClaimsList()
        {
            string sql = " Select a.ClaimID,a.RefNo,a.Claimdate,status,a.EmpCode,a.EmpName,BatchNo,x.Name as Createdby, " +
                         " Claimstatus =case when status = 0 then 'Not Submited'  When status = -2 then 'Rejected'  When status = -1 then 'Approved' When status = -3 then 'Paid' When status = -5 then 'Batch Approval wf' When Status = -99 then 'Cancelled' When Status = -6 then 'Cash Summary Approved' End, " +
                         " (select sum(b.CashPaid)  from CFM_ClaimDetails b  where a.ClaimID = b.ClaimID) as CashPaid  from CFM_Claims a, CFM_USER x" +
                         " Where a.CreatedBy = x.Usr_key" +
                         " And a.CompanyID In (" + CompanyAccess + ") And a.BUID in ( " + BUAccess + ")";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<MyClaimList>(sql).ToList();
                return cartItems;
            }

        }

        public List<MyClaimList> MyClaimsList()
        {
            string sql = "select a.ClaimID,a.RefNo,a.Claimdate,status,a.EmpCode,a.EmpName,BatchNo," +
                        " Claimstatus =case when status = 0 then 'Not Submited' " +
                        " When status = -2 then 'Rejected' " +
                        " When status = -1 then 'Approved'" +
                        " When status = -3 then 'Paid'" +
                        " When status = -5 then 'Batch Approval wf'" +
                        " When Status =-99 then 'Cancelled' When Status = -6 then 'Cash Summary Approved' End, " + 
                         " (select sum(b.CashPaid)  from CFM_ClaimDetails b  where a.ClaimID = b.ClaimID) as CashPaid " +
                         " from CFM_Claims a  where a.CreatedBy = " + UserId + " order by Claimdate";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<MyClaimList>(sql).ToList();
                return cartItems;
            }

        }
        public List<ViewModelCart> GetClaims(int ClaimID)
        {
            string sql = "Select ROW_NUMBER() OVER(ORDER BY a.Name ASC) AS SNo,b.ReqNo," +
                         "a.Name as BU ,c.Name as Category ,d.InvoiceNo, d.ClaimDetailID as RecordID,'upclaim' as mode,CashPaid,d.LineItemStatus " +
                        " From CFM_tblCompany a , CFM_Claims b, CFM_tblCategory c ,CFM_ClaimDetails d  " +
                        " Where b.ClaimID = d.ClaimID  " +
                        " And a.CompanyID = b.BUID  " +
                        " And d.categoryid = c.id  " +
                        " And b.ClaimID = @ID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<ViewModelCart>(sql, new { ID = ClaimID }).ToList();
                return cartItems;
            }

        }

        private ClaimData GetData(int ClaimID)
        {
            string sql = "Select ClaimID,BatchNo,BatchGeneratedBy,BatchGeneratedon from CFM_Claims where  CFM_Claims.ClaimID = @ID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var cartItems = connection.Query<ClaimData>(sql, new { ID = ClaimID }).SingleOrDefault();
                return cartItems;
            }

        }
        public void UpdateClaim(int ClaimID, int ClaimDetailID, Cart Carts)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                if (Carts != null && ClaimDetailID == 0) //Add Item
                {
                    sql = " Insert into CFM_ClaimDetails (ClaimID,CategoryID,InvoiceNo,InvoiceDate,CashPaid,Remarks) " +
                         " Values (" + ClaimID + ",@CategoryID,@InvoiceNo,@InvoiceDate,@CashPaid,@Remarks)";


                    connection.Execute(sql, new
                    {

                        Carts.CategoryID,
                        Carts.InvoiceNo,
                        Carts.InvoiceDate,
                        Carts.CashPaid,
                        Carts.Remarks
                    });
                }
                else //Update Item 
                {
                    sql = "update CFM_ClaimDetails Set CategoryID=@CategoryID,InvoiceNo=@InvoiceNo,CashPaid=@CashPaid,Remarks=@Remarks Where ClaimDetailID=@ClaimDetailID";
                    connection.Execute(sql, new
                    {

                        Carts.CategoryID,
                        Carts.InvoiceNo,
                        Carts.CashPaid,
                        Carts.Remarks,
                        ClaimDetailID
                    });
                }
            }

        }

         public string LogUnPaidCashPaidClaim(int ClaimID, decimal Amount, int ProcessBy, string LedgerQuery, string Status, int CompanyID)
        {
            
            int nPaidClaimLogID = 0;
            IDbTransaction transaction = null;
            int Ownby = ProcessBy; //
            
            var claim = GetData(ClaimID);
            string RefNo = Common.GetDocumentNumber(CompanyID, "CP");


            int OnwerID = UserServies.GetOwner(ProcessBy); // get Owner from CFM_User , if not found then Owner=ProcessBy
            if (OnwerID == 0)
            {
                OnwerID = ProcessBy;
            }
            sql = " INSERT INTO CFM_tblPaidClaimPayment(ClaimID,Amount,ProcessBy,Ownby,RefNo)VALUES (@ClaimID,@Amount,@ProcessBy,@OnwerID,@RefNo) " +
                               "Select Cast(SCOPE_IDENTITY() AS int)";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    nPaidClaimLogID = connection.Query<int>(sql, new
                    {
                        ClaimID,
                        Amount,
                        ProcessBy,
                        OnwerID,
                        RefNo


                    }, transaction).SingleOrDefault();

                    sql = "update CFM_Claims Set Paid='" + Status + "',Status=-3,PaymentStatus='P',BatchNo=null,BatchGeneratedBy=null Where ClaimID=@ClaimID"; //-3 mean Pay 
                    connection.Execute(sql, new
                    {
                        ClaimID

                    }, transaction);

                    connection.Execute(LedgerQuery, null, transaction);

                    sql = " INSERT INTO CFM_tblClaimHistory(ClaimID,BatchNo,BatchGeneratedBy,BatchGeneratedon)VALUES " +
                          " (@ClaimID,@BatchNo,@BatchGeneratedBy,@BatchGeneratedon) ";

                    connection.Query(sql, new
                    {
                        claim.ClaimID,
                        claim.BatchNo,
                        claim.BatchGeneratedBy,
                        claim.BatchGeneratedon


                    }, transaction).SingleOrDefault();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return e.Message;


                }
            }
            return "";
        }
        public string LogCashPaidClaim(int ClaimID, decimal Amount, int ProcessBy, string LedgerQuery,string Status, int CompanyID,int BUID)
        {
           
           int nPaidClaimLogID = 0;
            IDbTransaction transaction = null;
            int Ownby = ProcessBy; //

           

            string RefNo = Common.GetDocumentNumber(CompanyID, "CP");


            int OnwerID= UserServies.GetOwner(ProcessBy); // get Owner from CFM_User , if not found then Owner=ProcessBy ( remove later on )
            if (OnwerID == 0)
            {
                OnwerID = ProcessBy;
            }
            sql = " INSERT INTO CFM_tblPaidClaimPayment(ClaimID,Amount,ProcessBy,Ownby,RefNo)VALUES (@ClaimID,@Amount,@ProcessBy,@OnwerID,@RefNo) " +
                               "Select Cast(SCOPE_IDENTITY() AS int)";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    nPaidClaimLogID = connection.Query<int>(sql, new
                    {
                        ClaimID,
                        Amount,
                        ProcessBy,
                        OnwerID,
                        RefNo


                    }, transaction).SingleOrDefault();

                    if (BUID == -99) // paid from another company 
                    {
                        sql = "update CFM_Claims Set Paid='" + Status + "',Status=-3,PaymentStatus='P',PaidFromCompanyID=" + CompanyID + " Where ClaimID=@ClaimID"; //-3 mean Pay 
                    }
                    else
                    {
                        sql = "update CFM_Claims Set Paid='" + Status + "',Status=-3,PaymentStatus='P' Where ClaimID=@ClaimID"; //-3 mean Pay 
                    }
                    connection.Execute(sql, new
                    {
                        ClaimID

                    }, transaction);

                    connection.Execute(LedgerQuery, null, transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return e.Message;


                }
            }
            return "";
        }



        public string LogRequisitionPaidClaim(int ClaimID, int ReqID, decimal Amount, int ProcessBy, string LedgerQuery)
        {
            int nPaidReqClaimLogID = 0;
            IDbTransaction transaction = null;

            sql = " INSERT INTO CFM_tblRequistionClaimPayment(ClaimID,Req_ID,Amount,ProcessBy)VALUES (@ClaimID,@ReqID,@Amount,@ProcessBy) " +
                               "Select Cast(SCOPE_IDENTITY() AS int)";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    nPaidReqClaimLogID = connection.Query<int>(sql, new
                    {
                        ClaimID,
                        Amount,
                        ReqID,
                        ProcessBy


                    }, transaction).SingleOrDefault();

                    sql = "update CFM_Claims Set Paid='RP' Where ClaimID=@ClaimID";
                    connection.Execute(sql, new
                    {
                        ClaimID

                    }, transaction);

                    connection.Execute(LedgerQuery, null, transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return e.Message;


                }
            }
            return "";
        }

        public string UpdatePaidStatus(int ClaimID, string Status, int ProcessBy, string LedgerQuery)
        {

            IDbTransaction transaction = null;


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    sql = "update CFM_Claims Set Paid='" + Status + "' Where ClaimID=@ClaimID";
                    connection.Execute(sql, new
                    {
                        ClaimID

                    }, transaction);

                    connection.Execute(LedgerQuery, null, transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return e.Message;


                }
            }
            return "";
        }


        public void UpdateBatchNo(int ClaimID, string BatchNo, int ApproverID)
        {
            int ChequeBeneficiaryID = UserServies.GetOwner(UserId); // get Owner from CFM_User 
            

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "update CFM_Claims Set NextApprover=" + ApproverID + ",Status=-4,BatchNo='" + BatchNo + "',OLDBatchNo='" + BatchNo + "',Paid='BG',BatchGeneratedBy=" + UserId + ",BatchGeneratedon='" + DateTime.Now.ToString("dd/MMM/yyyy") + "',ChequeBeneficiaryID=" + ChequeBeneficiaryID + "  Where ClaimID=@ClaimID";
                connection.Execute(sql, new
                {
                    ClaimID

                });
            }
        }
        public void RejectLineItem(int ClaimDetailID, string Remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "update CFM_ClaimDetails Set LineItemStatus=1,Remarks=@Remarks Where ClaimDetailID=@ClaimDetailID";
                connection.Execute(sql, new
                {
                    ClaimDetailID,
                    Remarks

                });
            }

        }


        public void RejectClaim(int ClaimID, string Remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    sql = "Delete From CFM_ClamBatchPending  Where ClaimID=" + ClaimID + " And SubmitedTo=" + UserId;
                    connection.Execute(sql, null, transaction);

                    sql = "update CFM_Claims Set Status=100,BatchNo=null,Reverse=0,RejectedRemarks='" + Remarks + "' Where ClaimID=" + ClaimID;
                    connection.Execute(sql, null, transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
        }


        public void RejectCompleteBatch(string BatchNo, string Remarks)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    sql = "Delete From CFM_ClamBatchPending  Where BatchNo='" + BatchNo + "'";
                    connection.Execute(sql, null, transaction);

                    sql = "Update CFM_Claims Set Status=100,BatchNo=null,Reverse=0,RejectedRemarks='" + Remarks + "' Where BatchNo='" + BatchNo +"'";
                    connection.Execute(sql, null, transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }


        }
        public void ReverseClaim(int ClaimID, decimal Amount, int nCompanyID, int nBUID, string UpdateLedger)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    //sql = "Insert into CFM_tblPaidClaimPaymentHistory select * from CFM_tblPaidClaimPayment where ClaimID=" + ClaimID;
                    //connection.Execute(sql, null, transaction);

                    //sql = "delete from CFM_tblPaidClaimPayment where ClaimID=" + ClaimID;
                    //connection.Execute(sql, null, transaction);

                    sql = "Update CFM_Claims Set Status=-99,Reverse=1 Where ClaimID=" + ClaimID; 
                    connection.Execute(sql, null, transaction);

                    if (!string.IsNullOrEmpty(UpdateLedger))
                    {
                      connection.Execute(UpdateLedger, null, transaction);
                    }

                    sql = "Delete CFM_ClaimInvoice_Detail Where Calimid=" + ClaimID;
                    connection.Execute(sql, null, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
        }

        public void ReverseClaimAndCopy(int ClaimID, decimal Amount, int nCompanyID, int nBUID, string UpdateLedger)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                int nClaimID;
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                  //  sql = "Insert into CFM_tblPaidClaimPaymentHistory select * from CFM_tblPaidClaimPayment where ClaimID=" + ClaimID;
                   // connection.Execute(sql, null, transaction);

                   // sql = "delete from CFM_tblPaidClaimPayment where ClaimID=" + ClaimID;
                   // connection.Execute(sql, null, transaction);

                    sql = "Update CFM_Claims Set Status=-99,Reverse=1 Where ClaimID=" + ClaimID;
                    connection.Execute(sql, null, transaction);

                    if (!string.IsNullOrEmpty(UpdateLedger))
                    {
                        connection.Execute(UpdateLedger, null, transaction);
                    }
                    
                    string RefNo = Common.GetDocumentNumber(nCompanyID, "CC");
                    //Copy 
                    sql = " Insert into CFM_Claims(CompanyID, BUID, RefNo, Claimdate, CreatedBy, CreatedOn, EmpCode, EmpName,CopyClaimID) " +
                          " (Select CompanyID, BUID, '" + RefNo +"', GETDATE(), " + UserId + ", GETDATE(), EmpCode, EmpName," + ClaimID + " from CFM_Claims where ClaimID = " + ClaimID + ") " +
                          " Select Cast(SCOPE_IDENTITY() AS int)";


                    nClaimID = connection.Query<int>(sql, null, transaction).SingleOrDefault();

                    sql = " Insert into CFM_ClaimDetails(Claimid, CategoryID, LPOType, InvoiceNo, invoiceDate, VatAmount, CashPaid, Remarks, SupplierName, CostCode) " +
                        " (Select " + nClaimID + ", CategoryID, LPOType, InvoiceNo, invoiceDate, VatAmount, CashPaid, Remarks, SupplierName, CostCode from CFM_ClaimDetails where ClaimID =  " + ClaimID + ") ";

                     connection.Query(sql, null, transaction);



                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
        }
        

        public void NotReverseClaim(int ClaimID) // Send for batching 
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    
                    sql = "Update CFM_Claims Set Status=205 Where ClaimID=" + ClaimID;
                    connection.Execute(sql, null, transaction);

                    
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
        }

        public List<ViewModelPendingClaims> GetClaimsIndex()
        {
            string sql = " Select a.Claimid,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU,a.Status,isnull(a.CashReqHeaderID,0) as CashReqHeaderID,a.Status," +
                        "  a.EmpCode,a.EmpName,a.CopyClaimID," +
                        "  (Select Sum(d.cashPaid)  from CFM_ClaimDetails d where a.ClaimID = d.ClaimID) as Amount," +
                        "  Claimstatus=case when status=0 then 'Not Submited' " +
                        "  When status = -2 then 'Rejected' " +
                         " When status = -1 then 'Approved'" +
                         " When status = -3 then 'Paid'" +
                         " When Status > 0 then 'Approval Process'" +
                         " End " +
                         " From CFM_Claims a inner join CFM_tblCompany c  on a.BuID=c.CompanyID  inner join CFM_tblCompany d  on a.CompanyID=d.CompanyID " +
                         " Where  a.Status in (0,1,-1,-3) And a.CreatedBy=" + UserId + "  Union All" +
                         " Select a.Claimid,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU,a.Status,isnull(a.CashReqHeaderID,0) as CashReqHeaderID,a.Status, a.EmpCode," +
                         " a.EmpName,a.CopyClaimID," +
                         " (Select Sum(d.cashPaid)  from CFM_ClaimDetails d where a.ClaimID = d.ClaimID) as Amount," +
                         " Claimstatus=case when status=0 then 'Not Submited'  When status = -2 then 'Rejected'  When status = -1 then 'Approved' When status = -3 then 'Paid' When Status > 0 then 'Approval Process' End " +
                         " From CFM_Claims a   inner join CFM_tblCompany c  on a.BuID = c.CompanyID inner join CFM_tblCompany d  on a.CompanyID=d.CompanyID  Where a.Status in (0, 1, -1, -3) " +
                         " And a.CreatedBy = (Select Cashierid from CFM_BachGenerationConfigiration y where y.CordinatorID = " + UserId + ") order by d.Name,a.ClaimID ";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<ViewModelPendingClaims>(sql).ToList();
                return obj;
            }
        }

        public List<ViewModelPendingClaims> GetPendingClaims(int UserID)
        {
            string sql = " Select a.Claimid,a.RefNo,a.ClaimDate,c.Name as BU, " +
                         " ClaimType =case when a.ClaimType = 'B' then 'Business Operation' when a.ClaimType = 'A' then 'Admin' when a.ClaimType = 'V' then 'VIP' when a.ClaimType = 'P' then 'Private' end " +
                         " From CFM_Claims a , CFM_PendingClaims as b,CFM_tblCompany c    " +
                         " Where a.Claimid = b.Claimid" +
                         " And a.BuID=c.CompanyID" +
                         " And B.Doc_Code='01'" +
                         " And B.SubmittedTo =" + UserID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<ViewModelPendingClaims>(sql).ToList();
                return obj;
            }
        }


        

              public IEnumerable<T> GetBatchToGerateS2Summary<T>(int UserID)
        {
            // string sql = " select a.BatchNo ,d.Name as Company,c.Code + ' - ' + c.Name as BU,a.ClaimID, " +
            //            " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total" +
            //          " from cfm_claims a, CFM_tblCompany c, CFM_tblCompany d " +
            //        " where a.BuID = c.CompanyID and a.CompanyID = d.CompanyID " +
            //      " And NextApprover = " + UserID + " and a.Status in (-6) And a.SummaryS2S3=0 And d.GenerateSummary=1";

            string sql = " select a.BatchNo,sum(d.CashPaid) As Total " +
                        " from CFM_ClaimDetails d, cfm_claims a, CFM_tblCompany x " +
                        " where a.Claimid = d.ClaimID" +
                        " and a.CompanyID = x.CompanyID" +
                        " and a.NextApprover = " + UserID + " and a.Status in (-6) And a.SummaryS2S3 = 0 And x.GenerateSummary = 1" +
                        " group by a.BatchNo";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }


        public void S2S3Summary(string Batchno, string S2RefNo, string S3RefNo, int SummaryGeneratedBy, int SummaryChequeBef)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                IDbTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();


                    sql = " Update CFM_Claims Set S2RefNo='" + S2RefNo + "',S3RefNo='" + S3RefNo + "',SummaryGeneratedBy=" + SummaryGeneratedBy + ",SummaryChequeBef=" + SummaryChequeBef + ",SummaryS2S3=1" +
                          " Where BatchNo='" + Batchno +"'";

                    connection.Execute(sql, null, transaction);


                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
        }
        public IEnumerable<T> GetUnPaidClaims<T>(int ClaimID)
        {
            
            bool bFound = false;
            string sql="";
            bFound = new RBACUser(System.Web.HttpContext.Current.Session["UserName"].ToString()).HasRole("Cashier");// direct call rbac , not extended function

            if (bFound)
            {
                sql = " Select a.Claimid, a.BuID,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU ,X.ProcessBy,X.ProcessOn,a.CompanyID," +
                              " isnull(dbo.Get_CFM_SummaryApprovalLevel(a.CompanyID ,a.BuID),dbo.Get_CFM_SummaryApprovalLevelCashierWise(" + UserId + ")) as Approver," +
                              " isnull(dbo.Get_CFM_SummaryApprovalLevelID(a.CompanyID,a.BuID),dbo.Get_CFM_SummaryApprovalLevelIDCashierWise(" + UserId + ")) as ApproverID," +
                             " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total,'Paid Cash Document' as Type  " +
                             " From CFM_Claims a,CFM_tblPaidClaimPayment X, CFM_tblCompany c,CFM_tblCompany d " +
                             " Where a.Claimid = X.ClaimID " +
                             " and a.BuID = c.CompanyID and a.CompanyID=d.CompanyID " +
                             " And a.Status in (-3,205) And a.BatchNo is null" +
                             " And x.processby = " + UserId + " " +
                             " Union All"  +
                             " Select a.Claimid, a.BuID,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU ,X.ProcessBy,X.ProcessOn,a.CompanyID," +
                             " isnull(dbo.Get_CFM_SummaryApprovalLevel(a.CompanyID ,a.BuID),dbo.Get_CFM_SummaryApprovalLevelCashierWise(" + UserId + ")) as Approver," +
                             " isnull(dbo.Get_CFM_SummaryApprovalLevelID(a.CompanyID,a.BuID),dbo.Get_CFM_SummaryApprovalLevelIDCashierWise(" + UserId + ")) as ApproverID," +
                            " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total,'Paid Cash Document' as Type  " +
                            " From CFM_Claims a,CFM_tblPaidClaimPayment X, CFM_tblCompany c,CFM_tblCompany d " +
                            " Where a.Claimid = X.ClaimID " +
                            " and a.BuID = c.CompanyID  and a.CompanyID=d.CompanyID" +
                            " And a.Status in (-3,205) And a.BatchNo is null" +
                            " And x.processby in (Select subCashierid from CFM_BachGenerationConfigiration y where y.CashierId=" + UserId + ") " +
                            " Union All" +
                            " Select a.Claimid, a.BuID,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU ,a.TopUpBy as ProcessBy,a.TopupOn as ProcessOn,a.CompanyID, " +
                            " isnull(dbo.Get_CFM_SummaryApprovalLevel(a.CompanyID ,a.BuID),dbo.Get_CFM_SummaryApprovalLevelCashierWise(" + UserId + ")) as Approver, " +
                            " isnull(dbo.Get_CFM_SummaryApprovalLevelID(a.CompanyID,a.BuID),dbo.Get_CFM_SummaryApprovalLevelIDCashierWise(" + UserId + ")) as ApproverID, " +
                            " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total,'UnPaid Cash Document' as Type" +
                           "  From CFM_Claims a" +
                           "  inner join CFM_tblCompany c on a.BuID = c.CompanyID inner join CFM_tblCompany d on a.CompanyID  = d.CompanyID  " +
                           "  Where a.Status in(-3,205)  And a.PaymentStatus = 'T' And a.BatchNo is null And a.TopUpBy =" + UserId;

            }
            else  // IF CORDINATOR
            {
                sql = " Select a.Claimid, a.BuID,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU ,X.ProcessBy,X.ProcessOn,a.CompanyID," +
                             " isnull(dbo.Get_CFM_SummaryApprovalLevel(a.CompanyID ,a.BuID),dbo.Get_CFM_SummaryApprovalLevelCashierWise(" + UserId + ")) as Approver," +
                             " isnull(dbo.Get_CFM_SummaryApprovalLevelID(a.CompanyID,a.BuID),dbo.Get_CFM_SummaryApprovalLevelIDCashierWise(" + UserId + ")) as ApproverID," +
                            " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total,'Paid Cash Document' as Type  " +
                            " From CFM_Claims a,CFM_tblPaidClaimPayment X, CFM_tblCompany c,CFM_tblCompany d " +
                            " Where a.Claimid = X.ClaimID " +
                            " and a.BuID = c.CompanyID  and a.CompanyID=d.CompanyID" +
                            " And a.Status in (-3,205) And a.BatchNo is null" +
                            " And x.processby in (Select Cashierid from CFM_BachGenerationConfigiration y where y.cORDINATORID="+ UserId + ") " +
                           " Union All" +
                           " Select a.Claimid, a.BuID,a.RefNo,a.ClaimDate,d.Name as Company,c.Code + ' - ' + c.Name as BU ,a.TopUpBy as ProcessBy,a.TopUpon as ProcessOn,a.CompanyID, " +
                           " isnull(dbo.Get_CFM_SummaryApprovalLevel(a.CompanyID ,a.BuID),dbo.Get_CFM_SummaryApprovalLevelCashierWise(" + UserId + ")) as Approver, " +
                           " isnull(dbo.Get_CFM_SummaryApprovalLevelID(a.CompanyID,a.BuID),dbo.Get_CFM_SummaryApprovalLevelIDCashierWise(" + UserId + ")) as ApproverID, " +
                           " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total,'UnPaid Cash Document' as Type" +
                          "  From CFM_Claims a" +
                          "  inner join CFM_tblCompany c on a.BuID = c.CompanyID  inner join CFM_tblCompany d on a.CompanyID  = d.CompanyID  " +
                          "  Where a.Status in(-3,205)  And a.PaymentStatus = 'T' And a.BatchNo is null And a.TopUpBy in (Select Cashierid from CFM_BachGenerationConfigiration y where y.cORDINATORID ="+ UserId + ")";

            }




            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }



        public IEnumerable<T> UnPaidCashPersonnelClaims<T>()
        {

            string sql = " SELECT a.Claimid, a.RefNo, a.ClaimDate, c.Name AS BU, a.EmpCode, a.EmpName, 'Un Paid Cash Claim' ReqNo, a.paid, a.CompanyID, a.BUID, isnull(a.CashReqHeaderID, 0) AS CashReqHeaderID, " +
                    " (SELECT sum(d.CashPaid) FROM  CFM_ClaimDetails d WHERE  a.Claimid = d.ClaimID) AS Total, 0 AS Balance, x.name AS ClaimGeneratedBy, a.PaymentStatus" +
                    " FROM CFM_Claims a INNER JOIN CFM_tblCompany c ON a.BuID = c.CompanyID " +
                    " INNER JOIN CFM_USER x ON a.BatchGeneratedBy = x.Usr_key " +
                    " Inner join CFM_ClamBatchPending on a.ClaimID=CFM_ClamBatchPending.claimid" +
                    " WHERE a.status=-5 and a.PaymentStatus = 'T' And CFM_ClamBatchPending.submitedTo=" + UserId;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }

        public IEnumerable<T> RejectedClaims<T>()
        {
            string SQLQuery = "";
            SQLQuery = " SELECT a.RejectedRemarks,a.OLDBatchNo AS BatchNo,a.Claimid, a.RefNo, a.ClaimDate, a.EmpCode, a.EmpName,a.Reverse,a.PaymentStatus,a.CompanyID,a.BUID,c.Name as Company, " +
                       " (SELECT sum(d.CashPaid) FROM  CFM_ClaimDetails d WHERE  a.Claimid = d.ClaimID) AS Amount" +
                       " FROM CFM_Claims a,CFM_tblCompany c " +
                       " WHERE  a.CompanyID=C.CompanyID And a.status in (100) and a.BatchGeneratedBy =" + UserId + " "+
                       " Union All" +
                       " SELECT a.RejectedRemarks,a.OLDBatchNo AS BatchNo,a.Claimid, a.RefNo, a.ClaimDate, a.EmpCode, a.EmpName,a.Reverse,a.PaymentStatus,a.CompanyID,a.BUID,c.Name as Company,(SELECT sum(d.CashPaid) FROM  CFM_ClaimDetails d WHERE  a.Claimid = d.ClaimID) AS Amount " +
                       " FROM CFM_Claims a,CFM_tblCompany c,CFM_tblPaidClaimPayment p   WHERE a.CompanyID=C.CompanyID and a.ClaimID=p.ClaimID And a.status in (100) and p.ProcessBy in (Select Cashierid from CFM_BachGenerationConfigiration y where y.Cashierid = " + UserId + ")";


            string sql;
            bool bFound;
            bFound = new RBACUser(System.Web.HttpContext.Current.Session["UserName"].ToString()).HasRole("Cashier");

            if (bFound)
            {
                sql = " SELECT a.RejectedRemarks,a.OLDBatchNo AS BatchNo,a.Claimid, a.RefNo, a.ClaimDate, a.EmpCode, a.EmpName,a.Reverse,a.PaymentStatus,a.CompanyID,a.BUID,c.Name as Company, " +
                        " (SELECT sum(d.CashPaid) FROM  CFM_ClaimDetails d WHERE  a.Claimid = d.ClaimID) AS Amount" +
                        " FROM CFM_Claims a,CFM_tblCompany c " +
                        " WHERE  a.CompanyID=C.CompanyID And a.status in (100) and a.BatchGeneratedBy =" + UserId + " " +
                        " Union All" +
                        " SELECT a.RejectedRemarks,a.OLDBatchNo AS BatchNo,a.Claimid, a.RefNo, a.ClaimDate, a.EmpCode, a.EmpName,a.Reverse,a.PaymentStatus,a.CompanyID,a.BUID,c.Name as Company,(SELECT sum(d.CashPaid) FROM  CFM_ClaimDetails d WHERE  a.Claimid = d.ClaimID) AS Amount  " +
                        " FROM CFM_Claims a,CFM_tblCompany c" +
                        " WHERE a.CompanyID = C.CompanyID " +
                        " And a.status in (100) and a.PaymentStatus = 'T' AND A.TopUpBy =" + UserId + "";

            }
            else  // IF CORDINATOR //salman
            {
                sql = " SELECT a.RejectedRemarks,a.OLDBatchNo AS BatchNo,a.Claimid, a.RefNo, a.ClaimDate, a.EmpCode, a.EmpName,a.Reverse,a.PaymentStatus,a.CompanyID,a.BUID,c.Name as Company,(SELECT sum(d.CashPaid) FROM  CFM_ClaimDetails d WHERE  a.Claimid = d.ClaimID) AS Amount " +
                      " FROM CFM_Claims a,CFM_tblCompany c,CFM_tblPaidClaimPayment p   WHERE a.CompanyID=C.CompanyID and a.ClaimID=p.ClaimID And a.status in (100) and p.ProcessBy in (Select Cashierid from CFM_BachGenerationConfigiration y where y.cORDINATORID = " + UserId + ")";
            }
            
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                if (obj.Count == 0)
                {
                    var objNew = connection.Query<T>(SQLQuery).ToList();
                    return objNew;
                }
                else
                {
                    return obj;
                }

                
            }

        }

        public IEnumerable<T> ReceivePaymentFromTreasury<T>(int UserID)
        {
            string sql = "  Select * from CFM_TH_CheckProcessing Where Status='S' And CheckFavouringTo=" + UserID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }

        }
        public IEnumerable<T> GetClaimsBatchWise<T>()
        {
            string sql = "  Select a.Claimid, a.CompanyID,a.BuID,a.RefNo,a.ClaimDate,c.Name as BU ,a.BatchNo,isnull(Sum(p.Amount),0) as ReceivedAmt,0 as lSettle,p.ReceivedOn," +
                          " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total," +
                          " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) -isnull(sum(p.Amount),0) As Balance" +
                          " From CFM_Claims a" +
                          " inner join CFM_tblCompany c on a.BuID = c.CompanyID" +
                          " left outer join CFM_tblReceivedClaimPayment p on a.ClaimID = p.claimid" +
                          " Left outer join CFM_tblPaidClaimPayment pp on a.ClaimID = pp.ClaimID " +
                          " Where a.BatchNo is Not null" +
                          " And a.Status = -1" +
                          " And a.paid = 'BG' and a.BatchStatus='F' " +
                          " And pp.OWNBY = "+ UserId + " " +
                          " Group by a.Claimid, a.CompanyID,a.BuID,a.RefNo,a.ClaimDate,c.Name ,a.BatchNo,p.ReceivedOn";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }

        

      public IEnumerable<T> GetPendingChequesToBeProcess<T>(int UserID)
        {
            sql = " Select a.BatchNo,Sum(b.CashPaid)  as Total,ch.Name as CheckFavoring,ch.Usr_key as CheckFavouringTo  from CFM_Claims a " +
                  " inner join CFM_ClaimDetails b on a.ClaimID = B.ClaimID " +
                  " inner join CFM_tblCompany c on a.CompanyID = c.CompanyID " +
                  " inner join CFM_tblPaidClaimPayment pcp  on a.ClaimID=pcp.claimID" +
                  " INNER JOIN CFM_USER Y on y.Usr_key = pcp.processby" +
                  " INNER JOIN CFM_USER ch on y.Owner = ch.Usr_key" +
                  " where A.BatchStatus = 'C' " +
                  " group by  BatchNo,ch.Name,ch.Usr_key";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }
        public IEnumerable<T> GetTreasuryClaims<T>()
        {
            //string sql = " Select a.Claimid, a.CompanyID,a.BuID,a.RefNo,a.ClaimDate,c.Name as BU ,a.BatchNo,x.Name as ProcessBy, ow.Name as OwnBy, " +
            //             " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total  From CFM_Claims a " +
            //             " inner join CFM_tblCompany c on a.BuID = c.CompanyID" +
            //             " inner join CFM_tblPaidClaimPayment cp on a.ClaimID = cp.ClaimID " +
            //             " inner join CFM_USER x on cp.ProcessBy = x.Usr_key " +
            //             " inner join CFM_USER ow on cp.ownBy = ow.Usr_key Where  a.BatchNo is Not null" +
            //             " And a.Status = -1 And a.ForwardToStatus='T'";


            // sql = " Select a.ClaimID,a.BatchNo,Sum(b.CashPaid)  as Total,c.Name as CName,x.Name as ProcessBy,a.BatchForwardTo as ProcessByID from CFM_Claims a " +
            //        " inner join CFM_ClaimDetails b on a.ClaimID = B.ClaimID " +
            //        " inner join CFM_tblCompany c on a.CompanyID = c.CompanyID " +
            //       " inner join CFM_USER x on x.Usr_key=a.BatchForwardTo" + 
            //       " where A.BatchStatus = 'T' " +
            //       " group by BatchNo,c.Name,x.Name,a.BatchForwardTo,a.ClaimID ";


            sql = " Select a.ClaimID,a.Refno,a.BatchNo,Sum(b.CashPaid)  as Total,c.Name as CName, " +
                 " x.Name as ProcessBy,a.BatchForwardTo as ProcessByID," +
                 " ch.Name as CheckFavoring " +
                 " from CFM_Claims a  inner join CFM_ClaimDetails b on a.ClaimID = B.ClaimID" +
                 " inner join CFM_tblCompany c on a.CompanyID = c.CompanyID" +
                 " inner join CFM_USER x on x.Usr_key = a.BatchForwardTo " +
                 " inner join CFM_tblPaidClaimPayment pcp  on a.ClaimID = pcp.claimID " +
                 " INNER JOIN CFM_USER Y on y.Usr_key = pcp.processby " +
                 " INNER JOIN CFM_USER ch on y.Owner = ch.Usr_key " +
                 " where A.BatchStatus = 'T' " +
                "  group by BatchNo,a.Refno,c.Name,x.Name,a.BatchForwardTo,a.ClaimID , ch.name  order by  ch.name ";




            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }
        public IEnumerable<T> ListofBatches<T>(int ProcessBy, bool? showAllBatches)
        {
            //  string sql = "  Select distinct (a.BatchNo),a.CompanyID   from CFM_Claims a  where Status=-4 And a.BatchGeneratedBy=" + ProcessBy;
            string sql;
            if (showAllBatches == true)
            {
                sql = "select DISTINCT (a.BATCHNO),a.BatchGeneratedon from CFM_Claims a where a.BatchGeneratedBy = " + UserId + " AND BatchNo IS NOT NULL order by a.BatchGeneratedon desc";
            }
            else
            {
                sql = " Select distinct (a.BatchNo),a.CompanyID,a.nextApprover as ApproverID,x.Name as ApproverName,a.BatchGeneratedon   from CFM_Claims a   " +
                           " Inner join CFM_USER x on a.nextApprover = x.Usr_key " +
                           " Where Status = -4 And a.BatchGeneratedBy = " + ProcessBy;
            }

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }

        
        public IEnumerable<T> SitesListofBatches<T>(int ProcessBy)
        {
            sql = "select DISTINCT (a.BATCHNO),a.BatchGeneratedon from CFM_Claims a where a.CompanyID In (" + CompanyAccess + ") And a.BUID in ( " + BUAccess + ") And a.BatchNo IS NOT NULL and a.Buid <> 386  order by a.BatchNo ";
            
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }


        public IEnumerable<T> ListofS2S3Summary<T>()
        {
            
            string sql;
            
                sql = " Select DISTINCT(a.S2Refno),a.S3Refno from CFM_Claims a where a.SummaryGeneratedBy = " + UserId + " AND S2Refno IS NOT NULL";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }


        public IEnumerable<T> TrackSummary<T>(string TrackRefNo)
        {
            string sql;

            sql = " Select b.BatchNo,b.RefNo as ClaimRef,CFM_tblCompany.Code + ' - ' + CFM_tblCompany.Name as BU,b.PaymentStatus " +
                  " From CFM_Claims b " +
                  " INNER Join CFM_tblCompany on CFM_tblCompany.CompanyID = b.BuID" +
                  " Where b.S2RefNo ='"+ TrackRefNo +"' And b.Status = -6 " +
                  " group by b.BatchNo,b.RefNo,CFM_tblCompany.Code,CFM_tblCompany.Name,b.PaymentStatus";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }

        public IEnumerable<T> EditSummaryClaim<T>(string TrackRefNo)
        {
            string sql;

            sql = " Select a.ClaimID,a.ClaimDetailID,a.InvoiceNo,a.CostCode,a.CashPaid,a.VatAmount, " +
                  " a.CashPaid as AdjustCashPaid, a.VatAmount as AdjustVatAmount,'' as Remarks" +
                  " from CFM_ClaimDetails a , CFM_Claims b" +
                  " WHERE a.ClaimID = b.ClaimID And b.RefNo = '" + TrackRefNo + "' "; 

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }
        public void EditSummaryProcess(decimal CashPaid, decimal ApprovedCashPaid, decimal AdjustCashPaid, decimal VatAmount, decimal AdjustVatAmount, int claimID, int detailID,string PaymentStatus,int PaidBy,int CompanyID,string RefNo,string Remarks)
        {

            IDbTransaction transaction = null;
            string sqlDetail = "";

            sql = " Update CFM_ClaimDetails Set CashPaid=" + ApprovedCashPaid + ",Previous_CashPaid=" + CashPaid + ", " +
                  " VATAmount=" + AdjustVatAmount + ",AdjustedBy=" + UserId + ",AdjustedOn= '" + DateTime.Now + "' , AdjustRemarks='" + Remarks + "',AdjustedAmount= " + AdjustCashPaid +" " +
                  " Where ClaimDetailID=" + detailID;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    connection.Query(sql,null, transaction).SingleOrDefault();


                    if (PaymentStatus == "P")
                    {
                        sqlDetail = " INSERT INTO CFM_tblledger(TDate,document_ID,document_Type,drcr,Amount,AccountID,CompanyID,Doc_Ref)VALUES " +
                                   " ( '" + DateTime.Now + "'," + claimID + ",'PSAD',1," + Math.Abs(AdjustCashPaid) + " , " + PaidBy + "," + CompanyID + ",'" + RefNo + "' )";


                        connection.Execute(sqlDetail,null, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }

        }


        public T GeRecrod<T>(int ClaimID)
        {
            string sql = "Select * from CFM_Claims where  CFM_Claims.ClaimID = @ID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var OBJ = connection.Query<T>(sql, new { ID = ClaimID }).SingleOrDefault();
                return OBJ;
            }

        }
        public int GetPaidBy(int ClaimID)
        {
            string sql = "Select ProcessBy from CFM_tblPaidClaimPayment where ClaimID = @ID";


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var OBJ = connection.Query<int>(sql, new { ID = ClaimID }).SingleOrDefault();
                return OBJ;
            }

        }
        

        public IEnumerable<T> PendingBillProcess<T>(int UserID)
        {

            string sql = "";
           
                
                   sql = " Select a.Refno,a.ClaimID,a.BatchNo,x.Name as BatchGeneratedBy,Sum(b.CashPaid) as Amount,c.Code + ' - ' + c.Name  as BU,cb.level,a.CompanyID,a.BuID," +
                         " ClaimDocument= case when a.PaymentStatus='P' THEN 'Paid Cash Document' "+ 
                         " when a.PaymentStatus = 'T' THEN 'UnPaid Cash Document' End "+ 
                         " from CFM_Claims a " +
                              " inner join CFM_ClaimDetails b on a.ClaimID=b.ClaimID" +
                              " inner join CFM_tblCompany c on c.CompanyID=a.BuID " +
                              " inner join CFM_USER x on a.BatchGeneratedBy = x.Usr_key" +
                              " inner join CFM_ClamBatchPending cb on cb.claimid=a.claimID " +
                              "  where a.status = -5 And  cb.Submitedto  =" + UserID + " " +
                             "  Group by a.BatchNo,a.Refno,x.Name,a.ClaimID,c.Code,c.Name,cb.level,a.CompanyID,a.BuID,a.PaymentStatus ";

              
           
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }



        public IEnumerable<T> PendingCashPayment<T>()
        {
            string sql = "  Select a.CreatedBy, a.Claimid, a.BuID,a.RefNo,a.ClaimDate,c.Name as BU ,a.EmpCode,a.EmpName,isnull(Sum(p.Amount),0) as ReceivedAmt,0 as lSettle,p.PaidOn," +
                         "  (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total, " +
                         "  (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) -isnull(sum(p.Amount), 0) As Balance " +
                         "  From CFM_Claims a " +
                         "  inner join CFM_tblCompany c on a.BuID = c.CompanyID " +
                         "  left outer join cfm_tblPaidClaimPayment p on a.ClaimID = p.claimid " +
                         "  Where a.Empcode is not null " +
                         "  And a.BatchNo is not null " +
                         "  And a.Status = -6 " +
                         "  Group by a.Claimid, a.BuID,a.RefNo,a.ClaimDate,c.Name ,a.Empcode,a.EmpName , a.CreatedBy,p.PaidOn ";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }

        public IEnumerable<T> PaymentDetailViewModel<T>(int ID)
        {
            string sql = " Select u.Name as ProcessBy, ProcessOn ,a.Amount as AmountReceived, " +
                         " (Select sum(d.CashPaid) from CFM_ClaimDetails d where a.Claimid = d.ClaimID) As Total  " +
                         " from CFM_tblReceivedClaimPayment a ,CFM_USER u " +
                         " Where a.processby = u.Usr_key And a.ClaimID=" + @ID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql, new { ClaimID = @ID }).ToList();
                return obj;
            }
        }


        public T GetEmployeeFromJDE<T>(string empcode)
        {
            string sql = " SELECT [LTRIM(RTRIM(ALTERNATIVENO))] AS ALTERNATIVENO, [LTRIM(ADDRESSNO)] AS ADDRESSNO, [LTRIM(RTRIM(NAME))] AS NAME, " +
                         " [LTRIM(RTRIM(COMPANY))] AS COMPANY," +
                         " [LTRIM(RTRIM(COMPANYDESC))] AS COMPANYNAME," +
                         " [LTRIM(RTRIM(BUSINESSUNIT))] AS BU, [LTRIM(RTRIM(BUDESC))] AS BUNAME" +
                         " FROM  OPENQUERY(JDEPROD, " +
                         " 'SELECT LTRIM(RTRIM(ALTERNATIVENO)),LTRIM(ADDRESSNO),ltrim(rtrim(NAME)),LTRIM(RTRIM(COMPANY)),LTRIM(RTRIM(COMPANYDESC)), " +
                         " LTRIM(RTrim(BUSINESSUNIT)), LTRIM(RTrim(BUDESC))" +
                         " FROM PRODDTA.EFORM_EMPLOYEE_MASTER_INFO WHERE ALTERNATIVENO =''" + empcode + "''') ";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).SingleOrDefault();
                return obj;


            }
        }
        public ViewModelPendingDetail PendingClaimDetail(int ID)
        {
            string sql = " Select ROW_NUMBER() OVER(ORDER BY b.ClaimID ASC) AS SNo,d.LpoType,d.PO_Num,d.JD_Co_No,d.OP_PO_Type, " +
                         " d.CategoryID,b.ClaimID,b.CompanyID,b.BuID,b.Status,b.RefNo ,'' as CreatedBy,b.CreatedBy as nCreatedBy,b.CreatedOn,b.Claimdate ,company.Name as CompanyName,b.CashReqHeaderID,b.EmpCode,b.EmpName, " +
                         " a.Name as BU ,c.Name as Category ,d.InvoiceNo, d.ClaimDetailID as RecordID,TotalAmountbeforeVat,VatAmount,CashPaid,d.Remarks,LineItemStatus" +
                         " From CFM_tblCompany a, CFM_tblCompany as company,CFM_Claims b, CFM_tblCategory c, CFM_ClaimDetails d" +
                         " Where b.ClaimID = d.ClaimID" +
                         " And a.CompanyID = b.BUID" +
                         " And company.CompanyID=b.companyid" +
                         " And d.categoryid = c.id" +
                         " And b.ClaimID = " + ID;



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<PendingClaimDetail>(sql).ToList();

                PendingClaimHeader cliamHeader = new PendingClaimHeader();
                cliamHeader.RefNo = obj[0].RefNo;
                cliamHeader.ClaimID = obj[0].ClaimID;
                cliamHeader.CompanyID = obj[0].CompanyID;
                cliamHeader.BuID = obj[0].BuID;
                cliamHeader.Status = obj[0].Status;
                cliamHeader.CreatedOn = obj[0].CreatedOn;
                cliamHeader.Claimdate = obj[0].Claimdate;
                cliamHeader.BU = obj[0].BU;
                cliamHeader.CompanyName = obj[0].CompanyName;
                cliamHeader.Amount = obj[0].CashPaid;
                cliamHeader.nCreatedBy = obj[0].nCreatedBy;
                cliamHeader.CashReqHeaderID = obj[0].CashReqHeaderID;
                cliamHeader.EmpCode = obj[0].EmpCode;
                cliamHeader.EmpName = obj[0].EmpName;
                var ViewModel = new ViewModelPendingDetail()
                {
                    objClaimTotal = GetPendingClaimsTotal(ID),
                    PendingClaimDetail = obj,
                    objHeaderClaim = cliamHeader

                };
                return ViewModel;
            }
        }

        public ViewModelAutoGenerateClaim AutoGenerateClaimDetail(int ID)
        {
            string sql = " Select ROW_NUMBER() OVER(ORDER BY b.ClaimID ASC) AS SNo,b.ReqNo,b.CashReqHeaderID," +
                         " b.ClaimID,b.CompanyID,b.BuID,b.Status,b.RefNo ,'' as CreatedBy,b.CreatedBy as nCreatedBy,b.CreatedOn,b.Claimdate ,company.Name as CompanyName, " +
                         " a.Name as BU ,c.Name as Category ,d.InvoiceNo, d.ClaimDetailID as RecordID,TotalAmountbeforeVat,VatAmount,CashPaid,d.Remarks,LineItemStatus" +
                         " From CFM_tblCompany a, CFM_tblCompany as company,CFM_Claims b, CFM_tblCategory c, CFM_ClaimDetails d" +
                         " Where b.ClaimID = d.ClaimID" +
                         " And a.CompanyID = b.BUID" +
                         " And company.CompanyID=b.companyid" +
                         " And d.categoryid = c.id" +
                         " And b.ClaimID = " + ID;



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<PendingClaimDetail>(sql).ToList();

                PendingClaimHeader cliamHeader = new PendingClaimHeader();
                cliamHeader.RefNo = obj[0].RefNo;
                cliamHeader.ReqNo = obj[0].ReqNo;
                cliamHeader.ClaimID = obj[0].ClaimID;
                cliamHeader.CashReqHeaderID = obj[0].CashReqHeaderID;
                cliamHeader.CompanyID = obj[0].CompanyID;
                cliamHeader.BuID = obj[0].BuID;
                cliamHeader.Status = obj[0].Status;
                cliamHeader.CreatedOn = obj[0].CreatedOn;
                cliamHeader.BU = obj[0].BU;
                cliamHeader.CompanyName = obj[0].CompanyName;
                cliamHeader.Amount = obj[0].CashPaid;
                cliamHeader.nCreatedBy = obj[0].nCreatedBy;


                var ViewModel = new ViewModelAutoGenerateClaim()
                {
                    objClaimTotal = GetPendingClaimsTotal(ID),
                    PendingClaimDetail = obj,
                    objHeaderClaim = cliamHeader

                };
                return ViewModel;
            }
        }

        public ClaimTotal GetPendingClaimsTotal(int ClaimID)
        {
            string sql = " Select Sum(a.TotalAmountbeforeVat) as TotalAmountbeforeVat,Sum(a.VatAmount) as TotalVatAmount," +
                         " Sum(a.CashPaid ) as TotalCashPaid from CFM_ClaimDetails a where a.ClaimID=@ID";

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<ClaimTotal>(sql, new { ID = ClaimID }).SingleOrDefault();
                return obj;
            }
        }


        public IEnumerable<T> OutStandingClaims<T>(string EmpCode)
        {
            string sql = "  Select * from V_OutStandingClaims Where CompanyID in (" + CompanyAccess + ") And BUID in ( " + BUAccess + ") And ClaimType <>'P'";

            if (!string.IsNullOrEmpty(EmpCode))
            {
                sql = sql +  " And EMPCODE= '" + EmpCode + "'";
            }
            sql = sql + " order by V_OutStandingClaims.Company";
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }

        public IEnumerable<T> OutStandingPersonnalClaims<T>(string EmpCode)
        {
            string sql = "  Select Code + ' - ' + Company as CompanyNameCode,* from V_OutStandingPersonnalClaims Where CompanyID in (" + CompanyAccess + ") And BUID in ( " + BUAccess + ")";

            if (!string.IsNullOrEmpty(EmpCode))
            {
                sql = sql + " And EMPCODE= '" + EmpCode + "'";
            }
            sql = sql + " order by V_OutStandingPersonnalClaims.Company";
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }

        public void DeleteClaimItemDetail(int id)
        {
            
            string sql = " Delete from CFM_ClaimDetails where ClaimDetailID=" + id;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                
                connection.Execute(sql);


            }
        }

        //GetTotal Refund 

        private decimal GetTotalRefund(int CashReqHeaderID)
        {
            sql = "select isnull(Sum(ClaimAmount),0) TotalRefund from CFM_CashRequistionClaims where CashReqHeaderID = " + CashReqHeaderID + " and ReFundRequestID  is not null";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query(sql).SingleOrDefault();
                
                if (obj != null)
                {
                    return (decimal)obj.TotalRefund;
                }
                return 0;
            }
        }
        private decimal GetTotalClaimPaid(int CashReqHeaderID)
        {
            sql = "select isnull(Sum(ClaimAmount),0) TotalClaim from CFM_CashRequistionClaims where CashReqHeaderID = " + CashReqHeaderID + " and ReFundRequestID  is null";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query(sql).SingleOrDefault();

                if (obj != null)
                {
                    return (decimal)obj.TotalClaim;
                }
                return 0;

            }
        }

        private decimal GetClaimAmount(int ClaimID)
        {
            sql = "select Sum(b.CashPaid) as CashPaid from CFM_ClaimDetails b where ReqAdjustment='X' And b.ClaimID =" + ClaimID; 


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query(sql).SingleOrDefault();

                if (obj.CashPaid != null)
                {
                    return (decimal)obj.CashPaid;
                }
                return 0;

            }
        }
        public void AddjustCashRequisitionBalance(int ClaimID, string ntype, int CashReqHeaderID)
        {
            double nBalance;
            decimal nClaimAmount = 0;
           // decimal nTotalRefund= GetTotalRefund(CashReqHeaderID);
          //  decimal nPreviousClaimPaid = GetTotalClaimPaid(CashReqHeaderID);
            nClaimAmount =  GetClaimAmount(ClaimID);


            decimal TotalClaimPaid = nClaimAmount;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();
                IDbTransaction transaction = connection.BeginTransaction();


               sql = " update a set a.Balance =a.Balance - " + TotalClaimPaid +  " from CFM_Th_CashRequisitionForm a where a.id=" + CashReqHeaderID;

                connection.Execute(sql, null, transaction);

                sql = " update a set a.ReqAdjustment ='A' from CFM_ClaimDetails a where a.ClaimID=" + ClaimID;


                connection.Execute(sql, null, transaction);

                sql = " update CFM_CashRequistionClaims " +
                      " Set ClaimAmount=(select isnull(Sum(b.CashPaid),0) from CFM_ClaimDetails b where b.ClaimID =" + ClaimID + " )" +
                      " Where CFM_CashRequistionClaims.ClaimID=" + ClaimID;

                connection.Execute(sql, null, transaction);

                if (ntype == "F")
                {
                    sql = " update CFM_Claims " +
                          " Set lClaimstatusForReq=3,Paid='RF' where ClaimID =" + ClaimID + " ";

                    connection.Execute(sql, null, transaction);


                    sql = " update CFM_TH_CashRequisitionForm  " +
                       " Set CurrentStatus=3  where id =" + CashReqHeaderID + " ";


                    connection.Execute(sql, null, transaction);

                    // check if balance is 0 then Close  
                    //GetBalance 
                    sql = "Select isnull(sum(Balance),0)  as Balance from CFM_TH_CashRequisitionForm where  ID=" + CashReqHeaderID + " AND Balance > 0";

                    System.Data.IDataReader Ireader = connection.ExecuteReader(sql, null, transaction);
                    Ireader.Read();

                    var obj = Ireader["Balance"];

                    nBalance = (Double)obj;
                    Ireader.Close();
                    if (nBalance == 0)
                    {
                        sql = " update CFM_TH_CashRequisitionForm  " +
                              " Set Closed=1  where id =" + CashReqHeaderID + " ";


                        connection.Execute(sql, null, transaction);

                    }
                }

                transaction.Commit();
            }



        }

        public T GetClaimMasterDetail<T>(int ClaimID, int nClaimDetailID)
        {
            string sql = " select a.ClaimID,b.LPOType,b.CategoryID,b.InvoiceNo,b.InvoiceDate,b.CashPaid,b.JD_Co_No,b.OP_PO_Type,b.PO_Num,isnull(a.CashReqHeaderID,0) as CashReqHeaderID,b.VatAmount,b.Remarks,b.LineItemStatus,b.SupplierName, b.CostCode,b.ReqAdjustment,b.Currency,b.ConversionRate " +
                         " from CFM_Claims a ,CFM_ClaimDetails b " +
                         " Where a.ClaimID = b.ClaimID " +
                         " and a.ClaimID = @ID And b.ClaimDetailID=@ClaimDetailID";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql, new { ID = ClaimID, ClaimDetailID = nClaimDetailID }).SingleOrDefault();
                return obj;
            }
        }

        public IEnumerable<T> GetClaimInvoiceDetail<T>(int ClaimDetailID)
        {
            string sql = " Select * from CFM_ClaimInvoice_Detail where ClaimDetailID=" + ClaimDetailID;


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql).ToList();
                return obj;
            }
        }
        public T GetClaimMaster<T>(int ClaimID)
        {
            string sql = " Select top 1 " +
                         " b.lSettleInAccount,b.ClaimID,b.CompanyID,b.BuID,b.Status,b.RefNo ,x.Name as CreatedBy,b.CreatedBy as nCreatedBy,b.ReqNo,b.Paid," +
                         " isnull(b.CashReqHeaderID,0) as CashReqHeaderID," +
                         " b.CreatedOn,b.Claimdate ,company.Name as CompanyName," +
                         " a.Code + ' - ' + a.Name as BU,isnull(CFM_ClaimDetails.ClaimDetailID,0) as  ClaimDetailID,b.EmpCode,b.EmpName,b.CopyClaimID, " +
                         " (Select isnull(Total,0) as Amount from CFM_TH_CashRequisitionForm Where id= b.CashReqHeaderID) as LPOAmount," +
                         " ClaimType=case when b.ClaimType='B' then 'Business Operation' when b.ClaimType='A' then 'Admin' when b.ClaimType='V' then 'VIP'  when b.ClaimType='P' then 'Private'end," +
                         " (Select isnull(Balance,0) as Amount from CFM_TH_CashRequisitionForm Where id= b.CashReqHeaderID) as LPOBalance" + 
                         " From CFM_Claims b " +
                         " inner join   CFM_tblCompany as company on company.CompanyID = b.companyid" +
                         " inner join   CFM_tblCompany as a on a.CompanyID = b.BUID" +
                         " inner join  CFM_USER as X on x.Usr_key = b.CreatedBy" +
                         " left outer join CFM_ClaimDetails on b.ClaimID = CFM_ClaimDetails.ClaimID" +
                         " Where b.ClaimID =@ID";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql, new { ID = ClaimID }).SingleOrDefault();
                return obj;
            }
        }

        public IEnumerable<T> GetClaimDetails<T>(int ClaimID)
        {
            string sql = " Select ROW_NUMBER() OVER(ORDER BY  d.claimdetailid ASC) AS SNo,d.ClaimID,CFM_Claims.CashReqHeaderID,CFM_Claims.Status,CFM_Claims.CopyClaimID, " +
                         " c.Name as Category ,d.CategoryID,d.InvoiceNo,d.invoiceDate, " +
                         " d.JD_Co_No,d.OP_PO_Type,d.PO_Num,d.LPOType," +
                         " d.ClaimDetailID ," +
                         " TotalAmountbeforeVat,VatAmount,CashPaid,d.Remarks,LineItemStatus" +
                         " From  CFM_tblCategory c, CFM_ClaimDetails d, CFM_Claims" +
                         " Where d.categoryid = c.id" +
                         " And d.ClaimID=CFM_Claims.ClaimID" + 
                         " And d.ClaimID =@ID";


            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query<T>(sql, new { ID = ClaimID }).ToList();
                return obj;
            }

        }


        public decimal CalculateReqItemBalance(int ClaimID)
        {
            string sql = " Select isnull(Sum(CashPaid),0) as TotalCashPaid from CFM_ClaimDetails " +
                         " Where ReqAdjustment='X' And CFM_ClaimDetails.ClaimID=@ID";



            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                var obj = connection.Query(sql, new { ID = ClaimID}).SingleOrDefault();
                if (obj != null)
                {
                    return (decimal)obj.TotalCashPaid;
                }
            }
            return 0;
        }


        public string ProcessForCheckProcessing(ChequeHeader chequeHeader ,List<ChequeDetail> model)
        {

            IDbTransaction transaction = null;
            string sqlDetail = "";

            sql = " INSERT INTO CFM_TH_CheckProcessing(RefNo,Total,CheckFavouringTo)VALUES " +
                                " (@RefNo,@Total,@CheckFavouringTo)";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                     connection.Query(sql, new
                    {
                         chequeHeader.RefNo,
                         chequeHeader.Total,
                         chequeHeader.CheckFavouringTo
                    }, transaction).SingleOrDefault();

                    foreach (var item in model)
                    {
                        sqlDetail = " INSERT INTO CFM_TD_CheckProcessing(RefNo,BatchNo,Total)VALUES " +
                               " (@RefNo,@BatchNo,@Total)";


                        connection.Execute(sqlDetail, new
                        {
                            item.BatchNo,
                            item.Total,
                            item.RefNo
                        }, transaction);
                    }
                    foreach (var item in model)
                    {
                        sql = "update CFM_Claims Set BatchStatus='D' Where BatchNo='" + item.BatchNo + "'";
                        connection.Execute(sql,null,transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return e.Message;


                }
            }



          
            return "";
        }

        public string ProcessPayment(List<UnPaidCashClaimViewModelService> model)
        {
            
            IDbTransaction transaction = null;
           

                foreach (var item in model)
            {
                if (item.lReceived == true)
                {
                   
                        int nPaidClaimLogID = 0;
                        sql = " INSERT INTO CFM_tblReceivedClaimPayment(ClaimID,Amount,ProcessBy,lSettle)VALUES " +
                              " (@ClaimID,@Balance," + UserId + ", @lSettle) " + 
                              " Select Cast(SCOPE_IDENTITY() AS int)";

                        using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                        {
                            try
                            {
                                connection.Open();
                                transaction = connection.BeginTransaction();
                                nPaidClaimLogID = connection.Query<int>(sql, new
                                {
                                    item.Claimid,
                                    item.Balance,
                                    item.lSettle
                                }, transaction).SingleOrDefault();
                            sql = null;
                                if (item.lSettle == true)
                                {
                                    sql = "update CFM_Claims Set Paid='PD' Where ClaimID=@ClaimID";
                                }
                                if (item.Total == (item.ReceivedAmt + item.Balance)) // Close Payment Case by marking paid =1 , if balance =0 then 
                                {
                                    sql = "update CFM_Claims Set Paid='PD' Where ClaimID=@ClaimID";
                                }
                                if (sql!=null)
                                 { 
                                    connection.Execute(sql, new
                                    {
                                        item.Claimid

                                    }, transaction);
                                 }
                            //Update Ledger 
                            string LedgerQuery = UpdateStock.PrepareStockProcessEntries("PCC", nPaidClaimLogID, 1, item.CompanyID, BusniesUnit, item.Balance, UserId, item.RefNo, UserName);

                                connection.Execute(LedgerQuery, null, transaction);
                                transaction.Commit();
                                 connection.Close();
                            }

                            catch (Exception e)
                            {
                                connection.Close();
                                transaction.Rollback();
                                return e.Message;
                            }
                        }
                   
                }
            }

            return "";
        }

            public int LogCashReceivedClaim(int ClaimID, decimal Amount, int ProcessBy, bool lSettle)
            {
                int nPaidClaimLogID = 0;
                sql = " INSERT INTO CFM_tblReceivedClaimPayment(ClaimID,Amount,ProcessBy,lSettle)VALUES (@ClaimID,@Amount,@ProcessBy,@lSettle) " +
                                   "Select Cast(SCOPE_IDENTITY() AS int)";

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {

                    try
                    {
                        nPaidClaimLogID = connection.Query<int>(sql, new
                        {
                            ClaimID,
                            Amount,
                            ProcessBy,
                            lSettle



                        }).SingleOrDefault();
                    }
                    catch (Exception e)
                    {
                        if (e.Message.IndexOf("") > 0)
                        {

                        }
                    }
                }
                return nPaidClaimLogID;
            }

        public void RemoveClaimInvoice(int id, int ClaimDetailID)
        {
           
            IDbTransaction transaction = null;

            sql = " Delete from  CFM_ClaimInvoice_Detail Where id =" + id;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    connection.Execute(sql, null, transaction);


                    sql = " update a set a.CashPaid=X.GrossAmount  from CFM_ClaimDetails a " +
                          " inner join(select Sum(GrossAmount) AS GrossAmount, ClaimDetailID from CFM_ClaimInvoice_Detail WHERE ClaimDetailID=" + ClaimDetailID + "  GROUP BY ClaimDetailID) as X " +
                          " on a.ClaimDetailID = X.ClaimDetailID  where a.ClaimDetailID =" + ClaimDetailID;

                        connection.Execute(sql, null, transaction);

                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    connection.Close();
                }
            }
        }

        public void TopupSettle(int ClaimID)
        {


            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "update CFM_Claims Set lTopupSettle=1  Where ClaimID=@ClaimID";
                connection.Execute(sql, new
                {
                    ClaimID

                });
            }
        }
        public bool IsInvoiceExist(int ClaimID)
        {

        
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "Select isNull(InvoiceNo,'X') AS InvoiceNo from CFM_ClaimDetails where claimid=" + ClaimID + " And  LPOType='N' And InvoiceNo is null";
                var obj = connection.Query(sql).ToList();
                if (obj.Count == 0)
                {
                    return false; 
                }
               
                return true;
            }
        }
        public int DeleteDetailRecord(int ID,int Reqid, decimal Amount)
        {

            decimal nClaimAmount = 0;
            //get Amount 
            if (Reqid > 0)
            {
                nClaimAmount = Amount;
            }


            string sql = " Delete from CFM_ClaimDetails where ClaimDetailID=" + ID;
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Open();
                IDbTransaction transaction = connection.BeginTransaction();
                connection.Execute(sql, null, transaction);

                sql = "Delete CFM_ClaimInvoice_Detail Where ClaimDetailID=" + ID;
                connection.Execute(sql, null, transaction);

                if (Reqid > 0)
                {
                    sql = " update a set a.Balance =a.Balance + " + nClaimAmount + " from CFM_Th_CashRequisitionForm a where a.id=" + Reqid;
                    connection.Execute(sql, null, transaction);
                    transaction.Commit();
                }
                else
                {
                    transaction.Commit();
                }
                return 1;
            }

            //using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            //{
            //    sql = "Delete from CFM_ClaimDetails where ClaimDetailID=" + ID ;
            //    connection.Execute(sql);
             

            //    return 1;
            //}
        }
        public int CancelClaim(int ID)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "Update CFM_Claims Set Status=-99 where ClaimID=" + ID;
                connection.Execute(sql);


                return 1;
            }
        }

        public int RevertClaimAfterApproved(int ID)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "Update CFM_Claims Set Status=0 where ClaimID=" + ID;
                connection.Execute(sql);
                return 1;
            }
        }

        public void RevertBatch(string nBatchNo)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "Update CFM_Claims Set Status=-3,BatchNo=null,BatchGeneratedBy=null,BatchGeneratedon=null,OLDBatchNo=null where BatchNo='" + nBatchNo + "'";
                connection.Execute(sql);
            }
        }

        public void UpdateChequeSignature(string nBatchNo,int ChequeBeneficiaryID)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = "Update CFM_Claims Set ChequeBeneficiaryID= " + ChequeBeneficiaryID + " where BatchNo='" + nBatchNo + "'";
                connection.Execute(sql);
            }
        }

        public int ProcessClaimSummary(string BatchNo, int CompanyID,int SubmitTo,int Level)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                sql = " Insert into CFM_ClamBatchPending (BatchNo,claimid,SubmitedTo,Level)" +
                      " (Select Batchno, ClaimID, " + SubmitTo + ", " + Level +" FROM CFM_Claims where BatchNo = '" + BatchNo + "')";
                      connection.Execute(sql);


                return 1;
            }
        }

        public int ProcessSummary(int ClaimID, int CompanyID, int SubmitTo, int Level, string BatchNo)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                //before update maintain History
                if (ClaimID > 0)
                {
                    sql = " update  CFM_ClamBatchPending  Set SubmitedTo=" + SubmitTo + ",Level=" + Level + " Where ClaimID=" + ClaimID;
                }
                else
                {
                    sql = " update  CFM_ClamBatchPending  Set SubmitedTo=" + SubmitTo + ",Level=" + Level + " Where BatchNo='" + BatchNo + "'";
                }
                connection.Execute(sql);


                return 1;
            }
        }

        
         public string GetClamaintEmailAddress(int ClaimID)
        {
            string strSql;
            string EmailAddress="";
            strSql = " select a.EmailAddress from CFM_USER a , CFM_Claims b  " +
                     " where a.Usr_key = b.CreatedBy and b.ClaimID = " + ClaimID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                 EmailAddress = connection.QueryFirstOrDefault<string>(strSql);
                 
            }
            return EmailAddress;
        }
            
        
        public string ProcessFundAdjustment(int ClaimID)
        {
            string strSql;

            strSql = "Update CFM_Claims  Set PaymentStatus='T',Status=-3,TopUpBy=" + UserId + " Where ClaimID =" + ClaimID;

            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Execute(strSql);
            }
            return "";
          }

        public void DeleteSummaryPending(int ClaimID,string BatchNo)
        {
            string strSql;

            if (ClaimID > 0)
            {
                strSql = " Delete from  CFM_ClamBatchPending  Where ClaimID=" + ClaimID;
            }
            else {
                strSql = " Delete from  CFM_ClamBatchPending  Where BatchNo='" + BatchNo + "'";
            }
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                connection.Execute(strSql);
            }
        }

        #region ReceiveCashFromTreasury

        public string ReceiveCashFromTreasury(List<ReceivePaymentFromTreasury> model)
        {

            IDbTransaction transaction = null;
            foreach (var item in model)
            {
                if (item.lReceived == true)
                {
                    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                    {
                        try
                        {
                            string LedgerQuery = UpdateStock.PrepareStockProcessEntries("PCC", 0, 1, CompanyID, BusniesUnit, item.Total, UserId, item.RefNo, UserName);
                            connection.Open();
                            transaction = connection.BeginTransaction();
                            connection.Execute(LedgerQuery, null, transaction);
                            sql = "update CFM_TH_CheckProcessing Set Status='P' Where RefNo='" + item.RefNo +"'";
                            connection.Execute(sql, null, transaction);
                            transaction.Commit();
                            connection.Close();
                        }

                        catch (Exception e)
                        {
                            connection.Close();
                            transaction.Rollback();
                            return e.Message;
                        }
                    }
                    
                }
            }

            return "";
        }
#endregion


    }
}


   
    
    



