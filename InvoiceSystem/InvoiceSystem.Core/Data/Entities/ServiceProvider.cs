using InvoiceSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.Data.Entities
{
    public class ServiceProvider : AddressInformation
    {
        public string ProviderName { get; set; }
        public bool IsVatPayer { get; set; }
    }
}
