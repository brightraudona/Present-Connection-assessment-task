using InvoiceSystem.Core.Data.Entities;
using InvoiceSystem.Core.Enumeratiors;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.Entities
{
    public class Invoice
    {
        public int Number { get; set; }
        public decimal Subtotal { get; set; }
        public int VatRate { get; set; }
        public decimal VatTax { get; set; }
        public decimal Total { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ServiceProviderId { get; set; }
        public DateTime PayUntil { get; set; }
        public InvoiceStatuses InvoiceStatus { get; set; }
        public Client Customer { get; set; }
        public ServiceProvider ServiceProvider { get; set; }
    }
}
