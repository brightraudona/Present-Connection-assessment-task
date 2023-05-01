using InvoiceSystem.Core.Entities;
using InvoiceSystem.Core.ExternalModels;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceSystem.Core.Data.Entities
{
    public class Client : AddressInformation
    {
        public bool NotACompany { get; set; }
        public Guid IndividualId { get; set; }
        public Guid CompanyId { get; set; }
        
        public Individual Individual { get; set; }
        public Company Company { get; set; }
    }
}
