﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.LinkedInPost
{
    public interface ILinkedInPost
    {
        Task<string> SubmitLinkedInRequest(Core.Domain.LinkedInPost model);
        T ViewRequest<T>(int TransactionID);

        Task<bool> SubmitForApproval(int ID, string Remarks);

        Task<bool> RejectForm(int ID, string Remarks);
    }
}