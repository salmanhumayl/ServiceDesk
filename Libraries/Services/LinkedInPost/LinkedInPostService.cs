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
        public Task<bool> RejectForm(int ID, string Remarks)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubmitForApproval(int ID, string Remarks)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SubmitLinkedInRequest(Core.Domain.LinkedInPost model)
        {
            return await IRepository.SubmitGroupRequest(model);
        }

        public T ViewRequest<T>(int TransactionID)
        {
            throw new NotImplementedException();
        }
    }
}
