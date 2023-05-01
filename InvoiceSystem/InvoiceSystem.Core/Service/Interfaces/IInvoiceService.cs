using InvoiceSystem.Core.Data.Entities;
using InvoiceSystem.Core.Entities;
using InvoiceSystem.Core.ExternalModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSystem.Core.Service.Interfaces
{
    public interface IInvoiceService
    {
        Task<Invoice> GenerateInvoice(ExternalInvoice externalInvoice);
    }
}
