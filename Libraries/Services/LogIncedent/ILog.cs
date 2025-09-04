using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LogIncedent
{
   public interface ILog
    {
        Task<bool> Assignment(int ID, string ProcessOwner, string Remarks);
     
        IEnumerable<T> AllLogRequest<T>();

        IEnumerable<T> ServiceProcessCount<T>();

        Task<IList<T>> GeneratePDF<T>(int RequestID);

        IEnumerable<T> ApprovedJDERequest<T>(string username);
        Task<bool> SubmitForApproval(AJCCFM.Core.Domain.SD_JDE.JDE model, string remarks);
        Task<bool> RejectForm(int ID, string Remarks, string ServiceCode);

        Task<bool> Completed(int ID, string Remarks);


        Task<int> ArchiveRecord(string AssetsNo, int RecordID);

        Task<int> UpdateJDEAddressNo(string JDENo, int RecordID);
        Task<int> DeleteDetailRecord(int RecordID);



        Model.IResponse SubmitJDERequest(AJCCFM.Models.LogModel model, string SubmittedTo, string EmpEmail, string SubmittedToEmail);
        T ViewRequest<T>(int TransactionID);

        //  void AddLogHistory(int TransactionID, string Status, string ProcessBy, string SubmittedTo, string Remarks, string Doc_Code);

        IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code);


        IEnumerable<T> JDEPending<T>(string username);
        IEnumerable<T> JDEProgress<T>(string username);
    }
}

