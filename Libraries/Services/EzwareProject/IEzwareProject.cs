﻿using Model;
using Model.EzwareProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EzwareProject
{
    public interface IEzwareProject
    {
        IEnumerable<T> AllEzwareProjectRequest<T>();

        IEnumerable<T> EzwareProjectProcessCount<T>();

        Task<IList<T>> GeneratePDF<T>(int RequestID);

        IEnumerable<T> ApprovedEzwareProjectRequest<T>();
        Task<bool> SubmitForApproval(int ID, string remarks);
        Task<bool> RejectForm(int ID, string Remarks);


        Task<int> ArchiveRecord(string AssetsNo, int RecordID);

        Task<int> DeleteDetailRecord(int RecordID);

        IResponse SubmitRequest(EzwareModel model, string SubmittedTo, string EmpEmail, string SubmittedToEmail);
        EzwareViewModel ViewRequest<T>(int TransactionID);


        IEnumerable<T> GetLogHistory<T>(int RecordID, string Doc_Code);


        IEnumerable<T> EzwareProjectPending<T>(string username);
        IEnumerable<T> EzwareProjectProgress<T>(string username);

        IEnumerable<T> AllEzwareRequest<T>();
    }
}
