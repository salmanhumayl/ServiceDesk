using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM.Models.Reports
{
    public class rptCashClaimFormModel
    {
        public int ClaimID { get; set; }
        public int ClaimDetailID { get; set; }
        public int SNo { get; set; }
        public string RefNo { get; set; }
        public string CompanyName { get; set; }
        public string BU { get; set; }
        public string Category { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalAmountbeforeVat { get; set; }
        public decimal VatAmount { get; set; }
        public decimal CashPaid { get; set; }
        public DateTime Claimdate { get; set; }
        public string  CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int CategoryID { get; set; }
        public string LPOType { get; set; }

        public string Remarks { get; set; }
        public DateTime invoiceDate { get; set; }
        public string TemplateDetail { get; set; }
        public string PO_Num { get; set; }
        public string JD_Co_No { get; set; }
        public string OP_PO_Type { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }

        public string SupplierName { get; set; }
        public string CostCode { get; set; }
        public string BatchNo { get; set; }

        public string Currency { get; set; }
        public decimal ConversionRate { get; set; }

    }

    public class rptS2Summary
    {
        public string BatchNo { get; set; }
        public DateTime BatchGeneratedon { get; set; }
        public string EmpName { get; set; }
        public string AccountNo { get; set; }
        public string BU { get; set; }
        public decimal CashPaid { get; set; }
        public decimal VatAmount { get; set; }
        public string BUCode { get; set; }
        public string SummaryPreparedBy { get; set; }
        public string ChequeBef { get; set; }
        public string Company { get; set; }
        public string Section { get; set; }
        public string S2RefNo { get; set; }
        public string S3RefNo { get; set; }
        

    }

    public class rptCashClaimSummaryModel
    {
        public int ClaimID { get; set; }
        public int ClaimDetailID { get; set; }
        public int SNo { get; set; }
        public string RefNo { get; set; }
        public string CompanyName { get; set; }
        public string BU { get; set; }
        public string Category { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalAmountbeforeVat { get; set; }
        public decimal VatAmount { get; set; }
        public decimal CashPaid { get; set; }
       
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int CategoryID { get; set; }
        public string LPOType { get; set; }

        public string Remarks { get; set; }
        public DateTime invoiceDate { get; set; }
        public string TemplateDetail { get; set; }
        public string BatchNo { get; set; }
        public string chequeFavouring { get; set; }
        public string EmpCode { get; set; }
        public string BUCode { get; set; }
        public string PaidByName { get; set; }
        public string PO_Num { get; set; }
        public string JD_Co_No { get; set; }
        public string SupplierName { get; set; }
        public string CostCode { get; set; }
        public string Name { get; set; }
        public string ClaimaintCode { get; set; }
        public string SummaryPreparedBy { get; set; }
    }


    public class rptShowClaimDetail
    {
        public string Company { get; set; }
        public string BusinessUnit { get; set; }
        public string Category { get; set; }
        public string RefNo { get; set; }
        public string Claimaint { get; set; }
        public DateTime Claimdate { get; set; }
        public decimal CashPaid { get; set; }
        public string PaymentStatus { get; set; }

    }
}