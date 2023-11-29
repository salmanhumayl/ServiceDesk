using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LinkedInPost
{
    public class LinkedInPostService : ILinkedInPost
    {

        private LinkedInPostRepository IRepository;

        public LinkedInPostService()
        {
            IRepository = new LinkedInPostRepository();

        }

        public IEnumerable<T> AllSocialNetWorking<T>()
        {
            return IRepository.AllSocialNetWorking<T>();
        }

        public Task<T> GeneratePDF<T>(int RequestID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LinkInPostPending<T>(string username)
        {
            return IRepository.LinkInPostPending<T>(username);
        }

        public IEnumerable<T> MySocialNetWorking<T>(string UserName)
        {
            return IRepository.MySocialNetWorking<T>(UserName);
        }

        public async Task<bool> RejectForm(int ID, string Remarks)
        {
            return await IRepository.RejectForm(ID, Remarks);
        }

        public async Task<bool> SubmitForApproval(int ID, string Remarks)
        {
            return await IRepository.SubmitForApproval(ID, Remarks);
        }

        public async Task<RefNoID> SubmitLinkedInRequest(Core.Domain.LinkedInPost model)
        {
            return await IRepository.SubmitGroupRequest(model);
        }

        public T ViewRequest<T>(int TransactionID)
        {
            return IRepository.ViewRequest<T>(TransactionID);
        }
    }
}
