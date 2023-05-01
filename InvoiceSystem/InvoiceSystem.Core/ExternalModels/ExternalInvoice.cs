using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.ExternalModels
{
    public class ExternalInvoice
    {
        public int InvoiceNumber { get; set; }
        public string ServiceProviderName { get; set; }
        public string ServiceProviderAddress { get; set; }
        public bool ProviderIsVatPayer { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public bool ClientIsNotACompany { get; set; }
        public string VatCode { get; set; }
        public DateTime SupplyServiceDate { get; set; }
        public List<ExternalInvoiceItem> Items { get; set; }
        public string PaymentTerms { get; set; }
        public bool IsPaid { get; set; }
    }
}
