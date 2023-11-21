using System;
using System.Collections.Generic;
using System.Text;
using Core.Domain;
using Services.Claim.ClaimViewModel;
using Services.JDOInterface;

namespace Services.Claim
{
    public class Claimservice : IClaim

    {

        public Claimservice()
        {
            IRepository = new ClaimRepository();

        }

        private ClaimRepository IRepository;
        public void AddToClaim(Cart Carts, string shoppingCartId)
        {

            IRepository.AddToClaim(Carts, shoppingCartId);
        }

        public Cart GetCartItemsByID(int RecordID)
        {

            return IRepository.GetCartItemsByID(RecordID);
        }
        public Cart GetClaimItemsByID(int RecordID)
        {

            return IRepository.GetClaimItemsByID(RecordID);
        }


        public List<ViewModelCart> GetCartItemsDetail(string shoppingCartId)
        {
            return IRepository.GetCartItemsDetail(shoppingCartId);
        }

        public List<Cart> GetCartItems(string shoppingCartId)
        {
            return IRepository.GetCartItems(shoppingCartId);


        }

        public string AutoGenerateClaim(int ID, string Reqno, int CompanyID, int BuID)
        {

            return IRepository.AutoGenerateClaim(ID, Reqno, CompanyID, BuID);
        }

        public string CreateClaim(List<Cart> Carts, string shoppingCartId)
        {
            return IRepository.CreateClaim(Carts, shoppingCartId);
        }

        public ViewModelSearchCliam TrackClaimPost(string TrackRefNo)
        {
            return IRepository.TrackClaimPost(TrackRefNo);
        }


        public List<ViewModelCart> GetClaims(int ClaimID)
        {
            return IRepository.GetClaims(ClaimID);
        }

        public void UpdateClaim(int ClaimID, int ClaimDetailID, Cart Carts)
        {
            IRepository.UpdateClaim(ClaimID, ClaimDetailID, Carts);
        }

        public List<ViewModelPendingClaims> GetPendingClaims(int UserID)
        {
            return IRepository.GetPendingClaims(UserID);
        }

        public ViewModelPendingDetail PendingClaimDetail(int ID)
        {
            return IRepository.PendingClaimDetail(ID);
        }
        public ViewModelAutoGenerateClaim AutoGenerateClaimDetail(int ID)
        {
            return IRepository.AutoGenerateClaimDetail(ID);
        }


        public IEnumerable<T> GetUnPaidClaims<T>(int ClaimID)
        {
            return IRepository.GetUnPaidClaims<T>(1);
        }

        public int LogCashReceivedClaim(int ClaimID, decimal Amount, int ProcessBy, bool lsettle)
        {
            return IRepository.LogCashReceivedClaim(ClaimID, Amount, ProcessBy, lsettle);
        }



        public void RejectLineItem(int ClaimDetailID, string Remarks)
        {
            IRepository.RejectLineItem(ClaimDetailID, Remarks);
        }

        public IEnumerable<T> GetClaimsBatchWise<T>()
        {
            return IRepository.GetClaimsBatchWise<T>();
        }

        public void UpdateBatchNo(int ClaimID, string BatchNo, int ApproverID)
        {
            IRepository.UpdateBatchNo(ClaimID, BatchNo, ApproverID);

        }

        public IEnumerable<T> PaymentDetailViewModel<T>(int ClaimID)
        {
            return IRepository.PaymentDetailViewModel<T>(ClaimID);
        }

        public T GetEmployeeFromJDE<T>(string empcode)
        {
            return IRepository.GetEmployeeFromJDE<T>(empcode);
        }

        public IEnumerable<T> PendingCashPayment<T>()
        {
            return IRepository.PendingCashPayment<T>();
        }

        public string LogCashPaidClaim(int ClaimID, decimal Amount, int ProcessBy, string LedgerQuery, string Status, int CompanyID,int BUID)
        {
            return IRepository.LogCashPaidClaim(ClaimID, Amount, ProcessBy, LedgerQuery, Status, CompanyID, BUID);
        }

        public IEnumerable<T> OutStandingClaims<T>(string EmpCode)
        {
            return IRepository.OutStandingClaims<T>(EmpCode);
        }

        public void DeleteClaimItemDetail(int id)
        {
            IRepository.DeleteClaimItemDetail(id);
        }

        public void AddjustCashRequisitionBalance(int ClaimID, string ntype, int CashReqHeaderID)
        {
            IRepository.AddjustCashRequisitionBalance(ClaimID, ntype, CashReqHeaderID);

        }

        public T GetClaimMasterDetail<T>(int ClaimID, int nClaimDetailID)
        {
            return IRepository.GetClaimMasterDetail<T>(ClaimID, nClaimDetailID);
        }

        public IEnumerable<T> GetClaimInvoiceDetail<T>(int ClaimDetailID)
        {
            return IRepository.GetClaimInvoiceDetail<T>(ClaimDetailID);
        }

        public T GetClaimMaster<T>(int ClaimID)
        {
            return IRepository.GetClaimMaster<T>(ClaimID);
        }

        public IEnumerable<T> GetClaimDetails<T>(int ClaimID)
        {

            return IRepository.GetClaimDetails<T>(ClaimID);
        }

        public string SaveClaim(string InsertSql, Cart ClaimData, List<JDAPViewModel> LPODetail)
        {
            return IRepository.SaveClaim(InsertSql, ClaimData, LPODetail);

        }

        public List<ViewModelPendingClaims> GetClaimsIndex()
        {
            return IRepository.GetClaimsIndex();
        }

        public int AddClaim(Cart Carts)
        {
            return IRepository.AddClaim(Carts);
        }

        public string UpdatePaidStatus(int ClaimID, string Status, int ProcessBy, string LedgerQuery)
        {
            return IRepository.UpdatePaidStatus(ClaimID, Status, ProcessBy, LedgerQuery);

        }

        public string LogRequisitionPaidClaim(int ClaimID, int ReqID, decimal Amount, int ProcessBy, string LedgerQuery)
        {
            return IRepository.LogRequisitionPaidClaim(ClaimID, ReqID, Amount, ProcessBy, LedgerQuery);
        }

        public decimal CalculateReqItemBalance(int ClaimID)
        {
            return IRepository.CalculateReqItemBalance(ClaimID);

        }

        public string ProcessPayment(List<UnPaidCashClaimViewModelService> model)
        {
            return IRepository.ProcessPayment(model);

        }

        public void RemoveClaimInvoice(int id, int ClaimDetailID)
        {
            IRepository.RemoveClaimInvoice(id, ClaimDetailID);

        }

        public void TopupSettle(int id)
        {
            IRepository.TopupSettle(id);

        }

        public void Update(int id, int ClaimDetailID)
        {
            throw new NotImplementedException();
        }

        public bool IsInvoiceExist(int ClaimID)
        {
            return IRepository.IsInvoiceExist(ClaimID);
        }

        public string SettleInAccount(int ID, string Reqno, int CompanyID, int BuID)
        {
            return IRepository.SettleInAccount(ID, Reqno, CompanyID, BuID);
        }

        public IEnumerable<T> GetTreasuryClaims<T>()
        {
            return IRepository.GetTreasuryClaims<T>();
        }

        public IEnumerable<T> ListofBatches<T>(int ProcessBy, bool? showAllBatches)
        {
            return IRepository.ListofBatches<T>(ProcessBy, showAllBatches);
        }

        public IEnumerable<T> PendingBillProcess<T>(int UserID)
        {
            return IRepository.PendingBillProcess<T>(UserID);
        }

        public IEnumerable<T> ReceivePaymentFromTreasury<T>(int UserID)
        {
            return IRepository.ReceivePaymentFromTreasury<T>(UserID);
        }

        public string ProcessForCheckProcessing(ChequeHeader chequeHeader, List<ChequeDetail> model)
        {
            return IRepository.ProcessForCheckProcessing(chequeHeader, model);
        }

        public IEnumerable<T> GetPendingChequesToBeProcess<T>(int UserID)
        {
            return IRepository.GetPendingChequesToBeProcess<T>(UserID);
        }

        public string ReceiveCashFromTreasury(List<ReceivePaymentFromTreasury> model)
        {
            return IRepository.ReceiveCashFromTreasury(model);
        }

        public int DeleteDetailRecord(int ID, int Reqid, decimal Amount)
        {
            return IRepository.DeleteDetailRecord(ID, Reqid, Amount);
        }

        public int CancelClaim(int ID)
        {
            return IRepository.CancelClaim(ID);
        }

        public int ProcessClaimSummary(string BatchNo, int CompanyID, int SubmitTO, int Level)
        {
            return IRepository.ProcessClaimSummary(BatchNo, CompanyID, SubmitTO, Level);
        }

        public int ProcessSummary(int ClaimID, int CompanyID, int SubmitTo, int Level, string BatchNo)
        {
            return IRepository.ProcessSummary(ClaimID, CompanyID, SubmitTo, Level, BatchNo);
        }

        public void DeleteSummaryPending(int ClaimID, string BatchNo)
        {
            IRepository.DeleteSummaryPending(ClaimID, BatchNo);
        }

        public string ProcessFundAdjustment(int ClaimID)
        {
            return IRepository.ProcessFundAdjustment(ClaimID);
        }

        public IEnumerable<T> UnPaidCashPersonnelClaims<T>()
        {
            return IRepository.UnPaidCashPersonnelClaims<T>();
        }

        public IEnumerable<T> RejectedClaims<T>()
        {
            return IRepository.RejectedClaims<T>();
        }


        public string LogUnPaidCashPaidClaim(int ClaimID, decimal Amount, int ProcessBy, string LedgerQuery, string Status, int CompanyID)
        {
            return IRepository.LogUnPaidCashPaidClaim(ClaimID, Amount, ProcessBy, LedgerQuery, Status, CompanyID);
        }

        public List<MyClaimList> MyClaimsList()
        {
            return IRepository.MyClaimsList();
        }

        public void RejectClaim(int ClaimID, string Remarks)
        {
            IRepository.RejectClaim(ClaimID, Remarks);
        }

        public void ReverseClaim(int nClaimID, decimal Amount, int nCompanyID, int nBUID, string UpdateLedger)
        {
            IRepository.ReverseClaim(nClaimID, Amount, nCompanyID, nBUID, UpdateLedger);
        }

        public void ReverseClaimAndCopy(int nClaimID, decimal Amount, int nCompanyID, int nBUID, string UpdateLedger)
        {
            IRepository.ReverseClaimAndCopy(nClaimID, Amount, nCompanyID, nBUID, UpdateLedger);
        }

        public List<UpdatVAT> EditVAT(string ClaimRefNo)
        {
            return IRepository.EditVAT(ClaimRefNo);
        }

        public void UpdateVAT(List<UpdatVAT> model)
        {
            IRepository.UpdateVAT(model);
        }

        public void NotReverseClaim(int nClaimID)
        {
            IRepository.NotReverseClaim(nClaimID);
        }

        public string GetClamaintEmailAddress(int ClaimID)
        {
            return IRepository.GetClamaintEmailAddress(ClaimID);
        }

        public int RevertClaimAfterApproved(int ID)
        {
            return IRepository.RevertClaimAfterApproved(ID);
        }

        public void RevertBatch(string nBatchNo)
        {
            IRepository.RevertBatch(nBatchNo);
        }

        public void UpdateChequeSignature(string nBatchNo, int ChequeBeneficiaryID)
        {
            IRepository.UpdateChequeSignature(nBatchNo, ChequeBeneficiaryID);
        }

        public void RejectCompleteBatch(string BatchNo, string Remarks)
        {
            IRepository.RejectCompleteBatch(BatchNo, Remarks);
        }

        public IEnumerable<T> GetBatchToGerateS2Summary<T>(int UserID)
        {
            return IRepository.GetBatchToGerateS2Summary<T>(UserID);
        }

        public void S2S3Summary(string Batchno, string S2RefNo, string S3RefNo, int SummarygeneratedBy, int SummaryChequeBef)
        {
            IRepository.S2S3Summary(Batchno, S2RefNo, S3RefNo, SummarygeneratedBy, SummaryChequeBef);
        }

        public IEnumerable<T> ListofS2S3Summary<T>()
        {
            return IRepository.ListofS2S3Summary<T>();
        }

        public List<MyClaimList> SiteStatusClaimsList()
        {
            return IRepository.SiteStatusClaimsList();
        }

        public IEnumerable<T> SitesListofBatches<T>(int ProcessBy)
        {
            return IRepository.SitesListofBatches<T>(ProcessBy);
        }

        public IEnumerable<T> TrackSummary<T>(string TrackRefNo)
        {
            return IRepository.TrackSummary<T>(TrackRefNo);
        }

        public IEnumerable<T> EditSummaryClaim<T>(string ClaimRefNo)
        {
            return IRepository.EditSummaryClaim<T>(ClaimRefNo);
        }

        public void EditSummaryProcess(decimal CashPaid, decimal ApprovedCashPaid, decimal AdjustCashPaid, decimal VatAmount, decimal AdjustVatAmount, int claimID, int detailID,string PaymentStatus, int PaidBy,int CompanyID,string RefNo,string Remarks)
        {
            IRepository.EditSummaryProcess(CashPaid, ApprovedCashPaid, AdjustCashPaid, VatAmount, AdjustVatAmount, claimID, detailID, PaymentStatus, PaidBy, CompanyID, RefNo, Remarks);

        }

        public T GetRecord<T>(int ClaimID)
        {
            return IRepository.GeRecrod<T>(ClaimID);
        }

        public int GetPaidBy(int ClaimID)
        {
            return IRepository.GetPaidBy(ClaimID);
        }

        public IEnumerable<T> OutStandingPersonnalClaims<T>(string EmpCode)
        {
            return IRepository.OutStandingPersonnalClaims<T>(EmpCode);
        }
    }
}
