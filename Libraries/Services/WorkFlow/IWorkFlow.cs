using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IWorkFlow
    {
        void AddLogHistory(int TransactionID, string Status, string Processby, string SubmittedTo, string Remarks, int Doc_Code,int SeqNo);
        void AddLogHistoryClearance(int TransactionID, string Status, string Processby, string SubmittedTo, string Remarks, int Doc_Code, int SeqNo, string Project);

        T GetNextLevel<T>(int Doc_Code, int CurrentLevel);

        IEnumerable<T> GetOptionalLevel<T>(int Doc_Code);

        IEnumerable<T> RoleDetail<T>(int Doc_Code, int RoleID);

        string SubmitForApproval(int Doc_Code, int CurrentLevel, int TransactionID, string Remarks,string TableName,string Submittedto,int SeqNo);

        IEnumerable<T> GetLogHistory<T>(int RecordID, int Doc_Code);
        IEnumerable<T> GetClearanceLogHistory<T>(int RecordID, int Doc_Code);

        int UpdateStatus(int ID, int Status, string TableName,int Doc_Code);


    }
}
