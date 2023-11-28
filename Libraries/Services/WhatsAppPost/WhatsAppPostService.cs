﻿using Model;
using Services.WhatsAppPost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WhatsAppPost
{
    public class WhatsAppPostService : IWhatsAppPost
    {

        private WhatsAppPostRepository IRepository;

        public WhatsAppPostService()
        {
            IRepository = new WhatsAppPostRepository();

        }

        public Task<T> GeneratePDF<T>(int RequestID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> WhatsAppPostPending<T>(string username)
        {
            return IRepository.LinkInPostPending<T>(username);
        }

       

        public async Task<bool> RejectForm(int ID, string Remarks)
        {
            return await IRepository.RejectForm(ID, Remarks);
        }

        public async Task<bool> SubmitForApproval(int ID, string Remarks)
        {
            return await IRepository.SubmitForApproval(ID, Remarks);
        }

        public async Task<RefNoID> SubmitLinkedInRequest(Core.Domain.WhatsApp model)
        {
            return await IRepository.SubmitGroupRequest(model);
        }

        public T ViewRequest<T>(int TransactionID)
        {
            return IRepository.ViewRequest<T>(TransactionID);
        }
    }
}