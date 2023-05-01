using InvoiceSystem.Core.Data.Entities;
using InvoiceSystem.Core.Entities;
using InvoiceSystem.Core.Enumeratiors;
using InvoiceSystem.Core.ExternalModels;
using InvoiceSystem.Core.ExternalServices;
using InvoiceSystem.Core.Helpers;
using InvoiceSystem.Core.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InvoiceSystem.Core.Service
{
    public class InvoiceService : IInvoiceService
    {
        private readonly VatStackService _vatStackService;
        private readonly CountryService _countryService;
        private readonly ILogger _logger;
        public InvoiceService(ILogger<InvoiceService> logger,
                             IConfiguration configuration)
        {
            _vatStackService = new VatStackService(configuration.GetSection("VatStack:API_KEY")?.Value);
            _countryService = new CountryService();
            _logger = logger;
        }

        public async Task<Invoice> GenerateInvoice(ExternalInvoice externalInvoice)
        {
            if (string.IsNullOrEmpty(externalInvoice.ClientAddress) ||
                string.IsNullOrEmpty(externalInvoice.ServiceProviderAddress))
            {
                _logger.LogError("No address provided for client or service provider.");
                return null;
            }

            var clientAddressInfo = AddressInformation
                .ParseAddressInformation(externalInvoice.ClientAddress);

            var serviceAddressInfo = AddressInformation
                .ParseAddressInformation(externalInvoice.ServiceProviderAddress);

            if (clientAddressInfo == null ||
                serviceAddressInfo == null)
            {
                _logger.LogError("Address was provided in the wrong format for client or service provider.");
                return null;
            }

            // Create Client
            Client client = new Client
            {
                NotACompany = externalInvoice.ClientIsNotACompany,
                Address = clientAddressInfo.Address,
                Country = clientAddressInfo.Country,
                City = clientAddressInfo.City,
                PostalCode = clientAddressInfo.PostalCode
            };


            if (client.NotACompany)
            {
                client.Individual = new Individual(externalInvoice.ClientName);
            }
            else
            {
                client.Company = new Company(externalInvoice.ClientName, externalInvoice.VatCode);
            }

            // Create ServiceProvider
            ServiceProvider serviceProvider = new ServiceProvider
            {
                ProviderName = externalInvoice.ServiceProviderName,
                IsVatPayer = externalInvoice.ProviderIsVatPayer,
                Address = serviceAddressInfo.Address,
                Country = serviceAddressInfo.Country,
                City = serviceAddressInfo.City,
                PostalCode = serviceAddressInfo.PostalCode
            };

            // Create invoice
            decimal subtotal = 0;
            subtotal = externalInvoice.Items.Sum(i => i.Quantity * i.Price);

            Invoice invoice = new Invoice
            {
                Number = externalInvoice.InvoiceNumber,
                Customer = client,
                ServiceProvider = serviceProvider,
                VatRate = 0,
                Subtotal = subtotal,
            };

            if (externalInvoice.ProviderIsVatPayer)
            {
                int VAT = 0;

                // Get Country code (in any language)
                XElement clientCountry, poviderCountry;
                try
                {
                    poviderCountry = await _countryService
                        .GetCountryDataXMLByAddress(serviceProvider.Country);

                    // Check if matches not to fetch same data.
                    if (!serviceProvider.Country.Equals(client.Country))
                        clientCountry = await _countryService
                            .GetCountryDataXMLByAddress(client.Country);
                    else
                        clientCountry = poviderCountry;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not get Country data, error: {ex.Message}");
                    return null;
                }

                // Get VAT Rate.
                JsonElement clientVatStack, poviderVatStack;
                try
                {
                    poviderVatStack = await _vatStackService.GetVatRates(
                        poviderCountry.Element("countryCode")?.Value.ToString());

                    // Check if matches not to fetch same data.
                    if (!serviceProvider.Country.Equals(client.Country))
                        clientVatStack = await _vatStackService.GetVatRates(
                            clientCountry.Element("countryCode")?.Value.ToString());
                    else
                        clientVatStack = poviderVatStack;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not get VAT data, error: {ex.Message}");
                    return null;
                }


                // Country is in EU for both client and provider, with a VAT rate.
                if (clientVatStack.GetProperty("rates_count").GetInt32() != 0 &&
                    poviderVatStack.GetProperty("rates_count").GetInt32() != 0)
                {
                    VAT = poviderVatStack.GetProperty("rates")[0].GetProperty("standard_rate").GetInt32();
                }

                // Calculate VAT amount.
                decimal vatTotalAmount = subtotal * ((decimal)VAT / 100);

                invoice.VatRate = VAT;
                invoice.VatTax = vatTotalAmount;
                invoice.Total = subtotal + vatTotalAmount;
            } 
            else
            {
                invoice.VatTax = 0;
                invoice.Total = subtotal;
            }

            TimeSpan? paymentSpan = PaymentTermHelper.GetTimeSpan(externalInvoice.PaymentTerms);
            if (paymentSpan != null) {
                DateTime paymentDate =
                    externalInvoice.SupplyServiceDate.Add((TimeSpan)paymentSpan);

                invoice.PayUntil = paymentDate;
                if (externalInvoice.IsPaid)
                    invoice.InvoiceStatus = InvoiceStatuses.Paid;
                else if (paymentDate < DateTime.Now)
                    invoice.InvoiceStatus = InvoiceStatuses.Expired;
                else if (paymentDate > DateTime.Now)
                    invoice.InvoiceStatus = InvoiceStatuses.Open;
            }
            else if (string.IsNullOrEmpty(externalInvoice.PaymentTerms))
            {
                invoice.InvoiceStatus = InvoiceStatuses.Concept;
            }
            else
            {
                if (externalInvoice.IsPaid)
                    invoice.InvoiceStatus = InvoiceStatuses.Paid;
                else
                    invoice.InvoiceStatus = InvoiceStatuses.Cancelled;
            }

            return invoice;
        }
    }
}
