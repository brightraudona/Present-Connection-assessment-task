using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.ExternalModels
{
    public class ExternalInvoiceItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
