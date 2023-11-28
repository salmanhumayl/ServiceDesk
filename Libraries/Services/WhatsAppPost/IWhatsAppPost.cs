﻿using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WhatsAppPost
{
    public interface IWhatsAppPost
    {
        
        Task<T> GeneratePDF<T>(int RequestID);
        Task<RefNoID> SubmitLinkedInRequest(Core.Domain.WhatsApp model);
        T ViewRequest<T>(int TransactionID);

        Task<bool> SubmitForApproval(int ID, string Remarks);

    

        Task<bool> RejectForm(int ID, string Remarks);

        IEnumerable<T> WhatsAppPostPending<T>(string username);
    }
}