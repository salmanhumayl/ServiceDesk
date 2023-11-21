using Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Services.Claim;
using Services.Claim.ClaimViewModel;
using Services.JDOInterface;

namespace Services.Claim
{
    public interface IClaim
    {
        string GetClamaintEmailAddress(int ClaimID);
        void RevertBatch(string nBatchNo);
        void UpdateChequeSignature(string nBatchNo, int ChequeBeneficiaryID);

        int ProcessClaimSummary(string BatchNo, int CompanyID,int SubmitTo, int Level); // Start ClaimSummary WorkFlow 
        int ProcessSummary(int ClaimID, int CompanyID, int SubmitTo, int Level, string BatchNo); // ClaimSummary in Approval Process
        string ProcessFundAdjustment(int ClaimID);

        void DeleteSummaryPending(int ClaimID,string BatchNo);

        int CancelClaim(int ID);
        int RevertClaimAfterApproved(int ID);
        int DeleteDetailRecord(int ID, int Reqid, decimal Amount);
        void RemoveClaimInvoice(int id, int ClaimDetailID);
        void TopupSettle(int id);

        void Update(int id, int ClaimDetailID);
        void AddjustCashRequisitionBalance(int ClaimID, string ntype,int CashReqHeaderID);

        void DeleteClaimItemDetail(int id);

        string AutoGenerateClaim(int ID, string Reqno,int CompanyID, int BuID);
        string SettleInAccount(int ID, string Reqno, int CompanyID, int BuID);
        void AddToClaim(Cart Carts,string shoppingCartId);

        int AddClaim(Cart Carts);
        
        List<ViewModelCart> GetCartItemsDetail(string shoppingCartId);
        Cart GetCartItemsByID(int RecordID);

        T GetRecord<T>(int ClaimID);

        List<Cart> GetCartItems(string shoppingCartId);
        string CreateClaim(List<Cart> Carts, string shoppingCartId);

        ViewModelSearchCliam TrackClaimPost(string TrackRefNo);

        IEnumerable<T> TrackSummary<T>(string TrackRefNo);

        void EditSummaryProcess(decimal CashPaid, decimal ApprovedCashPaid, decimal AdjustCashPaid, decimal VatAmount, decimal AdjustVatAmount, int claimID, int detailID,string PaymentStatus,int PaidBy,int CompanyID,string RefNo,string Remarks);

        int GetPaidBy(int ClaimID);

        List<UpdatVAT> EditVAT(string ClaimRefNo);

        IEnumerable<T> EditSummaryClaim<T>(string ClaimRefNo);

        void UpdateVAT(List<Services.Claim.ClaimViewModel.UpdatVAT> model);



        List<MyClaimList> MyClaimsList();
        List<MyClaimList> SiteStatusClaimsList();


        List<ViewModelCart> GetClaims(int ClaimID);

        void UpdateClaim(int ClaimID, int ClaimDetailID, Cart Carts);

        void RejectLineItem(int ClaimDetailID, string Remarks);
        void RejectClaim(int ClaimID, string Remarks);
        void RejectCompleteBatch(string BatchNo,string Remarks);
        void ReverseClaim(int nClaimID, decimal Amount, int nCompanyID, int nBUID, string UpdateLedger);

        void ReverseClaimAndCopy(int nClaimID, decimal Amount, int nCompanyID, int nBUID, string UpdateLedger);

        void NotReverseClaim(int nClaimID);


        List<ViewModelPendingClaims> GetPendingClaims(int UserID);
        List<ViewModelPendingClaims> GetClaimsIndex();

        ViewModelPendingDetail PendingClaimDetail(int ID);
        ViewModelAutoGenerateClaim AutoGenerateClaimDetail(int ID);


        IEnumerable<T> GetUnPaidClaims<T>(int ClaimID);

        IEnumerable<T> GetBatchToGerateS2Summary<T>(int UserID);

        IEnumerable<T> UnPaidCashPersonnelClaims<T>(); //Paid from personnal Pocket 

        IEnumerable<T> RejectedClaims<T>(); //Rejecte claim from Batch 
        
         IEnumerable<T> GetClaimsBatchWise<T>();
        IEnumerable<T> GetTreasuryClaims<T>();
        IEnumerable<T> ReceivePaymentFromTreasury<T>(int UserID);


        IEnumerable<T> PendingCashPayment<T>();
        IEnumerable<T> ListofBatches<T>(int ProcessBy, bool? showAllBatches);
        IEnumerable<T> SitesListofBatches<T>(int ProcessBy);

        IEnumerable<T> ListofS2S3Summary<T>();

        IEnumerable<T> PendingBillProcess<T>(int UserID);
        
        IEnumerable<T> OutStandingClaims <T>(string  EmpCode);
        IEnumerable<T> OutStandingPersonnalClaims<T>(string EmpCode);

        IEnumerable<T> PaymentDetailViewModel<T>(int ClaimID);

        void UpdateBatchNo(int ClaimID, string BatchNo,int ApproverID);
        void S2S3Summary(string Batchno, string S2RefNo, string S3RefNo,int SummarygeneratedBy,int SummaryChequeBef);


        int LogCashReceivedClaim(int ClaimID, decimal Amount, int ProcessBy,bool lsettle);

        string LogCashPaidClaim(int ClaimID, decimal Amount, int ProcessBy,string LedgerQuery,string Status,int CompanyID,int BUID);

        string LogUnPaidCashPaidClaim(int ClaimID, decimal Amount, int ProcessBy, string LedgerQuery, string Status, int CompanyID);

        string LogRequisitionPaidClaim(int ClaimID,int ReqID, decimal Amount, int ProcessBy, string LedgerQuery);

        string UpdatePaidStatus(int ClaimID, string Status,int ProcessBy, string LedgerQuery);

       

        T GetEmployeeFromJDE<T>(string empcode);

        T GetClaimMasterDetail<T>(int ClaimID, int nClaimDetailID);
        IEnumerable<T> GetClaimInvoiceDetail<T>(int ClaimDetailID);

        T GetClaimMaster<T>(int ClaimID);
        IEnumerable<T> GetClaimDetails<T>(int ClaimID);

        string SaveClaim(string  InsertSql , Cart ClaimData, List<JDAPViewModel> LPODetail);

        decimal CalculateReqItemBalance(int ClaimID);

        string ProcessPayment(List<UnPaidCashClaimViewModelService> model);

        string ProcessForCheckProcessing(ChequeHeader chequeHeader, List<ChequeDetail> model);

        IEnumerable<T>GetPendingChequesToBeProcess<T>(int UserID);

        Boolean IsInvoiceExist(int ClaimID);

        string ReceiveCashFromTreasury(List<ReceivePaymentFromTreasury> model);

    }
}
