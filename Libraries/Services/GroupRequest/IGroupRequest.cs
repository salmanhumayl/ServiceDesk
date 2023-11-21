using AJCCFM.Core;
using Core.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.GroupRequest
{
    public interface IGroupRequest
    {
        // Task<IEnumerable<T>> GeneratePDF<T>(int RequestID);
        Task<bool> PostDelegateRequest(int ID, string ProcessOwner, string Remarks);

        Task<bool> LogEmail(int TransactionID, string GUID,string DocCode);
        Task<bool> LogEmailCancel(int TransactionID,string DocCode);

        Task<int> GetToken(string GUID, string DocCode);

        Task<T> GeneratePDF<T>(int RequestID);



        IEnumerable<T> FolderProcessCount<T>();
        Task<int> ArchiveRecord(string AssetsNo,int RecordID);
        Task<int> DeleteDetailRecord(int RecordID);
        Task<bool> SubmitForApproval(int ID, string Remarks);

        Task<bool> ConfirmReview(int ID, string Access);
        Task<bool> ConfirmReviewIT(int ID, string Access, string Folder, string ProcessOwner,string ProcessOwnerLoginID,string ProcessOwnerEmail);


        Task<bool> RejectForm(int ID, string Remarks);
        string SubmitGroupRequest(EmployeeDetail model, string CardID);
        string SaveClaim (Cart ClaimData);

        T ViewRequest<T>(int TransactionID);
        IEnumerable<T> ViewRequestDetail<T>(int TransactionID);

        IEnumerable<T> GetRequestByGuid<T>(string Guid);

        //Task<IEnumerable<T>> MyFolders<T>(string UserName);

        IEnumerable<T> MyFolders<T>(string UserName);

        IEnumerable<T> AllFolders<T>();
        Task<IList<T>> GeneratePDF<T>(int RequestID, string ServiceCode);


        bool IsGroupAlreadySelected(string groupName, string guid);
        IEnumerable<T> GetClaimDetails<T>(string CartId);
        IEnumerable<T> ShareFolderPending<T>(string username);
        IEnumerable<T> ShareFolderProgress<T>(string username);

        IEnumerable<T> ApprovedShareFolderRequest<T>();


    }
}
