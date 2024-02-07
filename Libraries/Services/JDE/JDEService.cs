using AJCCFM.Core;
using AJCCFM.Models;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.JDE
{
    public class JDEService :IJDE
    {
        private JDERepository IRepository;
        public JDEService()
        {
            IRepository = new JDERepository();

        }
       
        public IResponse SubmitJDERequest(JDEModel model, string SubmittedTo, string EmpEmail, string SubmittedToEmail)
        {
            
            return IRepository.SubmitJDERequest(model, SubmittedTo, EmpEmail, SubmittedToEmail);
        }

        public IEnumerable<T> JDEPending<T>(string username)
        {
            return IRepository.JDEPending<T>(username);
        }

        public IEnumerable<T> JDEProgress<T>(string username)
        {
            return IRepository.JDEProgress<T>(username);
        }

        public T ViewRequest<T>(int TransactionID)
        {
            return IRepository.ViewRequest<T>(TransactionID);
        }

        public IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code)
        {
            return IRepository.GetLogHistory<T>(RecordID, Doc_Code);
        }

        public Task<bool> SubmitForApproval(AJCCFM.Core.Domain.SD_JDE.JDE model, string remarks)
        {
            return IRepository.SubmitForApproval(model, remarks);
        }

        public IEnumerable<T> ApprovedJDERequest<T>()
        {
            return IRepository.ApprovedJDERequest<T>();
        }

        public async Task<bool> RejectForm(int ID, string Remarks, string ServiceCode)
        {
            return await IRepository.RejectForm(ID, Remarks, ServiceCode);
        }

        public async Task<int> DeleteDetailRecord(int RecordID)
        {
            return await IRepository.DeleteDetailRecord(RecordID);
        }

        public async Task<IList<T>> GeneratePDF<T>(int RequestID)
        {
            return (IList<T>)await IRepository.GenerateServicePDF<T>(RequestID);
        }

        public async Task<int> ArchiveRecord(string AssetsNo, int RecordID)
        {
            return await IRepository.ArchiveRecord(AssetsNo, RecordID);
        }

        public IEnumerable<T> MyJDERequest<T>(string UserName)
        {
            return IRepository.MyJDERequest<T>(UserName);
        }

        public IEnumerable<T> AllJDERequest<T>()
        {
            return IRepository.AllJDERequest<T>();
        }

     
        public IEnumerable<T> ServiceProcessCount<T>()
        {
            return IRepository.ServiceProcessCount<T>();
        }

        public async  Task<int> UpdateJDEAddressNo(string JDENo, int RecordID)
        {
            return await IRepository.UpdateJDEAddressNo(JDENo, RecordID);
        }
    }
}
