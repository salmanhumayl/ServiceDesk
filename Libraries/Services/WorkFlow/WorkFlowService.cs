using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WorkFlow
{
    public class WorkFlowService : IWorkFlow
    {
        private WorkFlowRepository IRepository;
        public WorkFlowService()
        {
            IRepository = new WorkFlowRepository();

        }
        public void AddLogHistory(int TransactionID, string Status, string Processby, string SubmittedTo, string Remarks, int Doc_Code, int SeqNo)
        {
            IRepository.AddLogHistory(TransactionID, Status, Processby, SubmittedTo, Remarks, Doc_Code, SeqNo);
        }

        public IEnumerable<T> GetLogHistory<T>(int RecordId, int Doc_Code)
        {
            return IRepository.GetLogHistory<T>(RecordId, Doc_Code);
        }


        public IEnumerable<T> GetClearanceLogHistory<T>(int RecordId, int Doc_Code)
        {
            return IRepository.GetClearanceLogHistory<T>(RecordId, Doc_Code);
        }


        public T GetNextLevel<T>(int Doc_Code, int CurrentLevel)
        {
            return IRepository.GetNextLevel<T>(Doc_Code,CurrentLevel);
        }

        public IEnumerable<T> GetOptionalLevel<T>(int Doc_Code)
        {
            return IRepository.GetOptionalLevel<T>(Doc_Code);
        }

        public IEnumerable<T> RoleDetail<T>(int Doc_Code, int RoleID)
        {
            return IRepository.RoleDetail<T>(Doc_Code, RoleID);
        }

        public string SubmitForApproval(int Doc_Code, int CurrentLevel, int TransactionID, string Remarks,string TableName,string Submittedto,int SeqNo)
        {
            return IRepository.SubmitForApproval(Doc_Code, CurrentLevel, TransactionID,Remarks,TableName, Submittedto,SeqNo);
        }

        public int UpdateStatus(int ID, int Status, string TableName, int Doc_Code)
        {
            return IRepository.UpdateStatus(ID, Status, TableName, Doc_Code);

        }


        public void AddLogHistoryClearance(int TransactionID, string Status, string Processby, string SubmittedTo, string Remarks, int Doc_Code, int SeqNo, string Project)
        {

            
        }
    }
}
