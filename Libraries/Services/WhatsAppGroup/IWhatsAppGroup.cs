using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WhatsAppGroup
{
    public interface IWhatsAppGroup
    {
        
        Task<T> GeneratePDF<T>(int RequestID);
        Task<RefNoID> SubmitLinkedInRequest(Core.Domain.WhatsAppGroup model);
        T ViewRequest<T>(int TransactionID);

        Task<bool> SubmitForApproval(int ID, int Status,string Submitedto,string Remarks);

    

        Task<bool> RejectForm(int ID, string Remarks);

       
    }
}
