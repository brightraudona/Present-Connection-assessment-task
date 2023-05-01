using InvoiceSystem.Core.Enumeratiors;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Tests.Models
{
    public class InvoiceExpectedResults
    {
        public bool IsNotNull { get; set; }
        public InvoiceStatuses ExpectedStatus { get; set; }
        public decimal ExpectedTotal { get; set; }
        public int ExpectedVatRate { get; set; }
        public decimal ExpectedVatTax { get; set; }
        public bool ExpectedClientCompany { get; set; }
    }
}
