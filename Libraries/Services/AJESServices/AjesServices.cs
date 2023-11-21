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
    public class AjesServices :IServices
    {

        private ServicesRepository IRepository;

        public AjesServices()
        {
            IRepository = new ServicesRepository();

        }
        public string SaveService(CartService ClaimData)
        {
            return IRepository.SaveClaim(ClaimData);
        }

        public bool IsServiceAlreadySelected(string ServiceName, string guid)
        {
            return IRepository.IsServiceAlreadySelected(ServiceName, guid);
        }

        public IEnumerable<T> GetServiceDetails<T>(string CartId)
        {
            return IRepository.GetServiceDetails<T>(CartId);
        }

        public string SubmitServiceRequest(EmployeeDetail model, string CardID,string SubmittedTo,string EmpEmail,string SubmittedToEmail)
        {
            return IRepository.SubmitServiceRequest(model, CardID, SubmittedTo, EmpEmail, SubmittedToEmail);
        }

        public IEnumerable<T> ServicePending<T>(string username)
        {
            return IRepository.ServicePending<T>(username);
        }

        public IEnumerable<T> ServiceProgress<T>(string username)
        {
            return IRepository.ServiceProgress<T>(username);
        }

        public T ViewRequest<T>(int TransactionID)
        {
            return IRepository.ViewRequest<T>(TransactionID);
        }

        public IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code)
        {
            return IRepository.GetLogHistory<T>(RecordID, Doc_Code);
        }

        public Task<bool> SubmitForApproval(ServiceRequestModel_SD_VPN model, string remarks)
        {
            return IRepository.SubmitForApproval(model, remarks);
        }

        public IEnumerable<T> ApprovedServices<T>()
        {
            return IRepository.ApprovedServices<T>();
        }

        public async Task<bool> RejectForm(int ID, string Remarks, string ServiceCode)
        {
            return await IRepository.RejectForm(ID, Remarks, ServiceCode);
        }

        public async Task<int> DeleteDetailRecord(int RecordID)
        {
            return await IRepository.DeleteDetailRecord(RecordID);
        }

        public async Task<IList<T>> GeneratePDF<T>(int RequestID, string ServiceCode)
        {
            return (IList<T>) await IRepository.GenerateServicePDF<T>(RequestID, ServiceCode);
        }

        public async Task<int> ArchiveRecord(string AssetsNo,int RecordID)
        {
            return await IRepository.ArchiveRecord(AssetsNo,RecordID);
        }

        public IEnumerable<T> MyService<T>(string UserName)
        {
            return IRepository.MyService<T>(UserName);
        }

        public IEnumerable<T> AllService<T>()
        {
            return IRepository.AllService<T>();
        }

        public IEnumerable<T> GetRequestByGuid<T>(string Guid)
        {
            return IRepository.GetRequestByGuid<T>(Guid);
        }

        public IEnumerable<T> ServiceProcessCount<T>()
        {
             return IRepository.ServiceProcessCount<T>();
        }







        //public void AddLogHistory(int TransactionID, string Status, string ProcessBy, string SubmittedTo, string Remarks,string Doc_Code)
        //{
        //    IRepository.AddLogHistory(TransactionID, Status, ProcessBy, SubmittedTo,Remarks,Doc_Code);
        //}
    }
}
