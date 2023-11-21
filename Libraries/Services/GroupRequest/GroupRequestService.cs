using AJCCFM.Core;
using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.GroupRequest
{
    public class GroupRequestService : IGroupRequest
    {
        private GroupRequestRepository IRepository;
       
        public GroupRequestService()
        {
            IRepository = new GroupRequestRepository();

        }

        public IEnumerable<T> AllFolders<T>()
        {
            return IRepository.AllFolders<T>();
        }

        public IEnumerable<T> ApprovedShareFolderRequest<T>()
        {
            return IRepository.ApprovedShareFolderRequest<T>();
        }

        public async Task<int> ArchiveRecord(string AssetsNo,int RecordID)
        {
            return await IRepository.ArchiveRecord(AssetsNo,RecordID);
        }

        public async Task<bool> ConfirmReview(int ID,string Access)
        {
            return await IRepository.ConfirmReview(ID, Access);
        }

        public async Task<bool> ConfirmReviewIT(int ID, string Access, string Folder, string ProcessOwner, string ProcessOwnerLoginID, string ProcessOwnerEmail)
        {
            return await IRepository.ConfirmReviewIT(ID, Access, Folder,ProcessOwner, ProcessOwnerLoginID, ProcessOwnerEmail);
        }

        public async Task<int> DeleteDetailRecord(int RecordID)
        {
            return await IRepository.DeleteDetailRecord(RecordID);
        }

        public IEnumerable<T> FolderProcessCount<T>()
        {
            return IRepository.FolderProcessCount<T>();
        }

        public async Task<T> GeneratePDF<T>(int RequestID)
        {
            return await IRepository.GeneratePDF<T>(RequestID);
        }

        public Task<IList<T>> GeneratePDF<T>(int RequestID, string ServiceCode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetClaimDetails<T>(string CartId)
        {
            return IRepository.GetClaimDetails<T>(CartId);
        }

        public IEnumerable<T> GetRequestByGuid<T>(string Guid)
        {
            return IRepository.GetRequestByGuid<T>(Guid);
        }

        public async Task<int> GetToken(string mGUID, string DocCode )
        {
            return await IRepository.GetToken(mGUID, DocCode);
        }

        public bool IsGroupAlreadySelected(string groupName, string guid)
        {
            return IRepository.IsGroupAlreadySelected(groupName, guid);
        }

        public async Task<bool> LogEmail(int TransactionID, string GUID, string DocCode)
        {
            return await IRepository.LogEmail(TransactionID, GUID, DocCode);
        }

        public async Task<bool> LogEmailCancel(int TransactionID, string DocCode)
        {
            return await IRepository.LogEmailCancel(TransactionID, DocCode);
        }

        public IEnumerable<T> MyFolders<T>(string UserName)
        {
            return IRepository.MyFolders<T>(UserName);
        }

        public Task<bool> PostDelegateRequest(int ID, string ProcessOwner, string Remarks)
        {
            return IRepository.PostDelegateRequest(ID, ProcessOwner, Remarks);

        }

        public async Task<bool> RejectForm(int ID, string Remarks)
        {
            return await IRepository.RejectForm(ID, Remarks);
        }

        public string SaveClaim(Cart ClaimData)
        {
            return IRepository.SaveClaim( ClaimData);
        }

        public IEnumerable<T> ShareFolderPending<T>(string username)
        {
            return IRepository.ShareFolderPending<T>(username);
        }

        public IEnumerable<T> ShareFolderProgress<T>(string username)
         {
            return IRepository.ShareFolderProgress<T>(username);
        }

        public async Task<bool> SubmitForApproval(int ID, string Remarks)
        {
            return await IRepository.SubmitForApproval(ID, Remarks);
        }

        public string SubmitGroupRequest(EmployeeDetail model,string CardID)
        {
            return IRepository.SubmitGroupRequest(model, CardID);
        }

        public T ViewRequest<T>(int TransactionID)
        {
            return IRepository.ViewRequest<T>(TransactionID);
        }

        public IEnumerable<T> ViewRequestDetail<T>(int TransactionID)
        {
            return IRepository.ViewRequestDetail<T>(TransactionID);
        }
    }
}
