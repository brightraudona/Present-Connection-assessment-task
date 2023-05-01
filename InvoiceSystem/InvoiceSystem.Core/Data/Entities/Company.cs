using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.Data.Entities
{
    public class Company
    {
        public Company(string companyName, string vatCode)
        {
            CompanyName = companyName;
            VatCode = vatCode;
        }

        public string CompanyName { get; set; }
        public string VatCode { get; set; }
    }
}
