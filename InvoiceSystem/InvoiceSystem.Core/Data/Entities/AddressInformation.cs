using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InvoiceSystem.Core.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using InvoiceSystem.Core.ExternalModels;

namespace InvoiceSystem.Core.Entities
{
    public class AddressInformation
    {
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        public static AddressInformation ParseAddressInformation(string address)
        {
            string[] addressParts = address.Split(':');
            if (addressParts.Length != 4)
            {
                return null;
            }

            return new AddressInformation()
            { 
                Address = addressParts[0],
                Country = addressParts[1],
                City = addressParts[2],
                PostalCode = addressParts[3]
            };
        }
    }
}
