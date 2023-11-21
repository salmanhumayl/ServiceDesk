using AJCCFM.Core;
using AJCCFM.Core.Domain.SD_VPN;
using Core.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.AJESServices
{
    public interface IServices
    {

        IEnumerable<T> GetRequestByGuid<T>(string Guid);
        IEnumerable<T> MyService<T>(string UserName);
        IEnumerable<T> AllService<T>();

        IEnumerable<T> ServiceProcessCount<T>();
        
        Task<IList<T>> GeneratePDF<T>(int RequestID, string ServiceCode);
      
        IEnumerable<T> ApprovedServices<T>();
        Task<bool> SubmitForApproval(ServiceRequestModel_SD_VPN model, string remarks);
        Task<bool> RejectForm(int ID, string Remarks, string ServiceCode);
        string SaveService(CartService ClaimData);

        Task<int> ArchiveRecord(string AssetsNo,int RecordID);
        Task<int> DeleteDetailRecord(int RecordID);
        bool IsServiceAlreadySelected(string ServiceName, string guid);

        IEnumerable<T> GetServiceDetails<T>(string CartId);

        string SubmitServiceRequest(EmployeeDetail model, string CardID, string SubmittedTo, string EmpEmail, string SubmittedToEmail);
        T ViewRequest<T>(int TransactionID);

      //  void AddLogHistory(int TransactionID, string Status, string ProcessBy, string SubmittedTo, string Remarks, string Doc_Code);

        IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code);


        IEnumerable<T> ServicePending<T>(string username);
        IEnumerable<T> ServiceProgress<T>(string username);
    }
}
