using Core.Domain.EzwareRequest;
using Model;
using Model.EzwareProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EzwareProject
{
    public class EzwareProjectService : IEzwareProject
    {

        private EzwareProjectRepository IRepository;
        public EzwareProjectService()
        {
            IRepository = new EzwareProjectRepository();

        }
        public IEnumerable<T> AllEzwareProjectRequest<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ApprovedEzwareProjectRequest<T>()
        {
            throw new NotImplementedException();
        }

        public Task<int> ArchiveRecord(string AssetsNo, int RecordID)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteDetailRecord(int RecordID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> EzwareProjectPending<T>(string username)
        {
            return IRepository.EzwareProjectPending<T>(username);
        }

        public IEnumerable<T> EzwareProjectProcessCount<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> EzwareProjectProgress<T>(string username)
        {
            return IRepository.EzwareProjectProgress<T>(username);
        }

        public Task<IList<T>> GeneratePDF<T>(int RequestID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code)
        {
            throw new NotImplementedException();
        }


        public Task<bool> RejectForm(int ID, string Remarks, string ServiceCode)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubmitForApproval(AJCCFM.Core.Domain.SD_JDE.JDE model, string remarks)
        {
            throw new NotImplementedException();
        }

        public IResponse SubmitRequest(EzwareModel model, string SubmittedTo, string EmpEmail, string SubmittedToEmail)
        {
            return IRepository.SubmitRequest(model, SubmittedTo, EmpEmail, SubmittedToEmail);
        }

        public EzwareViewModel ViewRequest<T>(int TransactionID)
        {
           RequestHeader obj = IRepository.ViewRequest<RequestHeader>(TransactionID);
           var Detail = IRepository.ViewRequestDetail<RightModel>(TransactionID);

            EzwareViewModel EzwareModel = new EzwareViewModel();
            EzwareModel.empdetail  = obj;
            EzwareModel.EzwareRights = Detail;

            return EzwareModel;


        }
    }
}
