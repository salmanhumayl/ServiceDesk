using AJCCFM.Core;
using AJCCFM.Models;
using AJCCFM.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace Services.JDE
{
    public interface IJDE
    {
      
        IEnumerable<T> MyJDERequest<T>(string UserName);
        IEnumerable<T> AllJDERequest<T>();

        IEnumerable<T> ServiceProcessCount<T>();

        Task<IList<T>> GeneratePDF<T>(int RequestID);

        IEnumerable<T> ApprovedJDERequest<T>();
        Task<bool> SubmitForApproval(AJCCFM.Core.Domain.SD_JDE.JDE model, string remarks);
        Task<bool> RejectForm(int ID, string Remarks, string ServiceCode);
       

        Task<int> ArchiveRecord(string AssetsNo, int RecordID);

        Task<int> UpdateJDEAddressNo(string JDENo, int RecordID);
        Task<int> DeleteDetailRecord(int RecordID);
       

      
        IResponse SubmitJDERequest(JDEModel model, string SubmittedTo, string EmpEmail, string SubmittedToEmail);
        T ViewRequest<T>(int TransactionID);

        //  void AddLogHistory(int TransactionID, string Status, string ProcessBy, string SubmittedTo, string Remarks, string Doc_Code);

        IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code);


        IEnumerable<T> JDEPending<T>(string username);
        IEnumerable<T> JDEProgress<T>(string username);
    }
}
