using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Claim.ClaimViewModel
{

    public class UpdateStock
    {
        public static string PrepareStockProcessEntries(string docType, int docID, int stkType,
                                   int CompanyID, int BUID, decimal Amount, int AccountID, string DocRef, string AccountName)

        {
            string strSQL = "";


            strSQL = "INSERT into CFM_tblLedger (document_Type,document_ID,drcr,Amount,AccountID,CompanyID,BUID,Doc_Ref,AccountName) " +
                    " Values" +
                    " ('" + docType + "'," + docID + " ," + stkType + " ," + Amount + "," + AccountID + "," + CompanyID + ", " + BUID + ",'" + DocRef + "','" + AccountName + "')";




            return strSQL;
        }
    }

    public class UnPaidCashClaimViewModelService
    {
        public int Claimid { get; set; }
        public int BuID { get; set; }
        public int CompanyID { get; set; }

        public string RefNo { get; set; }

        public DateTime ClaimDate { get; set; }
        public string BU { get; set; }
        public decimal Total { get; set; }
        public Decimal ReceivedAmt { get; set; }
        public Decimal Balance { get; set; }


        public bool lReceived { get; set; }
        public bool lSettle { get; set; }

        public DateTime ReceivedOn { get; set; }
        public string BatchNo { get; set; }
    }


    public class ReceivePaymentFromTreasury
    {
        public string RefNo { get; set; }
        public decimal Total { get; set; }
        public string ChequeNo { get; set; }
        public string BankName { get; set; }
        public bool lReceived { get; set; }


    }
    public class TreasuryViewModel
    {
        public int ClaimID { get; set; }
        public string RefNo { get; set; }
        public string BatchNo { get; set; }
        public string ProcessBy { get; set; }
        public int ProcessByID { get; set; }
        public int CheckFavouringTo { get; set; }

        public string CName { get; set; }
        public decimal Total { get; set; }
        public bool lPrint { get; set; }
        public string CheckFavoring { get; set; }
    }



    public class ChequeHeader
    {
        public string RefNo { get; set; }
        public decimal Total { get; set; }
        public int CheckFavouringTo { get; set; }

    }
    public class ChequeDetail
    {
        public string RefNo { get; set; }
        public string BatchNo { get; set; }
        public decimal Total { get; set; }

    }


    public class PendingBillProcess
    {
        public string ClaimDocument { get; set; }
        public int ClaimID { get; set; }
        public string RefNo { get; set; }
        public string BatchGeneratedBy { get; set; }

        public DateTime BatchGeneratedon { get; set; }
        public string BatchNo { get; set; }
        public decimal Amount { get; set; }
        public string  BU { get; set; }
        public int level { get; set; }
        public int CompanyID { get; set; }
        public int BuID { get; set; }

    }
    public class ViewModelCart
        {
        public string SNo { get; set; }
        public string BU { get; set; }
            public string Category { get; set; }
            public string InvoiceNo { get; set; }

            public decimal CashPaid { get; set; }
            public int RecordID { get; set; }

            public string mode { get; set; }
            public string ReqNo { get; set; }

        public int LineItemStatus { get; set; } //0=NORMAL , 1 =Rejected , 2 =Revised 
    }

    public class ViewModelSearchCliam
    {

        public int ClaimID { get; set; }
        public string RefNo { get; set; }
        public int CompanyID { get; set; }

        public int BuID { get; set; }


        public String Claimstatus { get; set; }

        public int Status { get; set; }
        public DateTime Claimdate { get; set; }
    }


   
    public class UpdatVAT
    {

        public int ClaimID { get; set; }
        public string InvoiceNo { get; set; }
        public string CostCode { get; set; }
        public decimal VatAmount { get; set; }
        public int ClaimDetailID { get; set; }



    }

    public class ViewModelPendingClaims
    {
        public int ClaimPaid { get; set; } //identify the status either the claim Paid or not 
        public int Claimid { get; set; }
        public string RefNo { get; set; }

        public string BU { get; set; }
        public string Company { get; set; }
        public DateTime ClaimDate { get; set; }
        public int Status { get; set; }
        public string Claimstatus { get; set; }
        public int CashReqHeaderID { get; set; }
        public string EmpName { get; set; }
        public string EmpCode { get; set; }
        public decimal Amount { get; set; }
        public int CopyClaimID { get; set; }
        public string ClaimType { get; set; }

    }

    public class LevelInformation
    {

        public int level { get; set; }
        public int Usr_key { get; set; }
        public string  Name { get; set; }
        public string EmailAddress { get; set; }

    }

    public class PendingClaimDetail
    {
        public int SNo { get; set; }
        public int ClaimID { get; set; }
        public int RecordID { get; set; }

        public int CompanyID { get; set; }

        public int BuID { get; set; }
        public string BU { get; set; }
        public string CompanyName { get; set; }

        public string Status { get; set; }

        public string RefNo { get; set; }
        public string CreatedBy { get; set; }

        public int nCreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
      

        public DateTime Claimdate { get; set; }

        public string Category { get; set; }
        public int CategoryID { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalAmountbeforeVat { get; set; }
        public decimal VatAmount { get; set; }
        public decimal CashPaid { get; set; }
        public string Remarks { get; set; }

        public int LineItemStatus { get; set; }
        public string ReqNo { get; set; }
        public int CashReqHeaderID { get; set; }
        public string LpoType { get; set; }
        public string PO_Num { get; set; }
        public string JD_Co_No { get; set; }
        public string OP_PO_Type { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
    }

    public class PendingClaimHeader
    {
        public int ClaimID { get; set; }

        public int CompanyID { get; set; }

        public int BuID { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; }
        public string RefNo { get; set; }
        public string CreatedBy { get; set; }
        public  int  nCreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime Claimdate { get; set; }

        public string BU { get; set; }
        public string CompanyName { get; set; }
        public string ReqNo { get; set; }
        public int CashReqHeaderID { get; set; }

        public string EmpCode { get; set; }
        public string EmpName { get; set; }
    }

    public class ClaimTotal
    {
        public decimal TotalAmountbeforeVat { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalCashPaid { get; set; }

    }
    public class ViewModelPendingDetail
    {
        public ViewModelPendingDetail()
        {
           // objHeaderClaim = new PendingClaimHeader();
        }
        public PendingClaimHeader objHeaderClaim { get; set; }
        public ClaimTotal objClaimTotal { get; set; }
        public List<PendingClaimDetail> PendingClaimDetail { get; set; }


    }


    public class MyClaimList
    {
        public int ClaimID { get; set; }
        public string RefNo { get; set; }
        public DateTime Claimdate { get; set; }
        public decimal CashPaid { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string BatchNo { get; set; }
        public string Claimstatus { get; set; }
        public string Createdby { get; set; }

    }

    public class ViewModelAutoGenerateClaim
    {
        public ViewModelAutoGenerateClaim()
        {
            // objHeaderClaim = new PendingClaimHeader();
        }
        public PendingClaimHeader objHeaderClaim { get; set; }
        public ClaimTotal objClaimTotal { get; set; }
        public List<PendingClaimDetail> PendingClaimDetail { get; set; }


    }

    public class ClaimData
    {
        public int ClaimID { get; set; }
        public string BatchNo{ get; set; }
        public string BatchGeneratedBy { get; set; }
        public DateTime BatchGeneratedon { get; set; }
    }
}
