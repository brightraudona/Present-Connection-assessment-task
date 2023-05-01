using InvoiceSystem.Core.Entities;
using InvoiceSystem.Core.Enumeratiors;
using InvoiceSystem.Core.ExternalModels;
using InvoiceSystem.Core.ExternalServices;
using InvoiceSystem.Core.Service;
using InvoiceSystem.Core.Service.Interfaces;
using InvoiceSystem.Tests.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InvoiceSystem.Tests.Service
{
    public class InvoiceServiceTests
    {
        private readonly InvoiceService _sut; //System under test
        private readonly IConfiguration Configuration;

        public InvoiceServiceTests(ITestOutputHelper output)
        {
            var mockConfigJson = @"{
                ""VatStack"": {
                    ""API_KEY"": ""pk_live_a145e460caf1bc4ff5b5b0b443cb7854"",
                }
            }";

            var configDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(mockConfigJson);

            // Flatten the nested dictionary to match the format required by AddInMemoryCollection
            var flattenedConfig = new Dictionary<string, string>();
            foreach (var parent in configDictionary)
            {
                foreach (var child in parent.Value)
                {
                    flattenedConfig.Add($"{parent.Key}:{child.Key}", child.Value);
                }
            }

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(flattenedConfig)
                .Build();

            _sut = new InvoiceService(
                output.BuildLoggerFor<InvoiceService>(),
                Configuration);
        }

        public static readonly object[][] ExternalInvoiceTestData =
        {
            new object[] {
                "Expected result: Invoice should be generated," +
                "Open with vat rate 21," +
                "service provider and client in Lithuania",
                new ExternalInvoice
                {
                    InvoiceNumber = 1,
                    ServiceProviderName = "Service Provider in Lithuania",
                    ServiceProviderAddress = "Lentupio g. 12A:Lithuania:Vilnius:10218",
                    ProviderIsVatPayer = true,
                    ClientName = "Company in Lithuania",
                    ClientAddress = "Vaižganto g. 24:Lithuania:Kaunas:44229",
                    ClientIsNotACompany = false,
                    VatCode = "VAT123",
                    SupplyServiceDate = new DateTime(2023, 5, 1),
                    Items = new List<ExternalInvoiceItem>
                    {
                        new ExternalInvoiceItem { Description = "Item A", Quantity = 2, Price = 10m },
                        new ExternalInvoiceItem { Description = "Item B", Quantity = 1, Price = 20m }
                    },
                    PaymentTerms = "30 days",
                    IsPaid = false
                },
                new InvoiceExpectedResults
                {
                    IsNotNull = true,
                    ExpectedStatus = InvoiceStatuses.Open,
                    ExpectedTotal = 48.40m,
                    ExpectedVatRate = 21,
                    ExpectedVatTax = 8.40m,
                    ExpectedClientCompany = true
                }
            },
            new object[] {
                "Expected result: Invoice should be generated," +
                "Open with vat rate 0," +
                "service privider Lithuanian, but client in Australia",
                new ExternalInvoice
                {
                    InvoiceNumber = 1,
                    ServiceProviderName = "Service Provider in Lithuania",
                    ServiceProviderAddress = "Lentupio g. 12A:Lithuania:Vilnius:10218",
                    ProviderIsVatPayer = true,
                    ClientName = "Company in Australia",
                    ClientAddress = "Croydon Park NSW:Australia:Brighton Ave:2133",
                    ClientIsNotACompany = false,
                    VatCode = "VAT123",
                    SupplyServiceDate = new DateTime(2023, 5, 1),
                    Items = new List<ExternalInvoiceItem>
                    {
                        new ExternalInvoiceItem { Description = "Item A", Quantity = 2, Price = 10m },
                        new ExternalInvoiceItem { Description = "Item B", Quantity = 1, Price = 20m }
                    },
                    PaymentTerms = "30 days",
                    IsPaid = false
                },
                new InvoiceExpectedResults
                {
                    IsNotNull = true,
                    ExpectedStatus = InvoiceStatuses.Open,
                    ExpectedTotal = 40m,
                    ExpectedVatRate = 0,
                    ExpectedVatTax = 0m,
                    ExpectedClientCompany = true
                }
            },
            new object[] {
                "Expected result: Invoice should be generated," +
                "Paid with vat rate 22," +
                "Service provider in Italy, client in Lithuania",
                new ExternalInvoice
                {
                    InvoiceNumber = 1,
                    ServiceProviderName = "Service Provider in Italy",
                    ServiceProviderAddress = "Via Antonio Magliabechi, 5:Italy:Firenze FI:50122",
                    ProviderIsVatPayer = true,
                    ClientName = "Vardenis Lietuvenis",
                    ClientAddress = "Vaižganto g. 24:Lithuania:Kaunas:44229",
                    ClientIsNotACompany = true,
                    SupplyServiceDate = new DateTime(2023, 4, 1),
                    Items = new List<ExternalInvoiceItem>
                    {
                        new ExternalInvoiceItem { Description = "Item B", Quantity = 1, Price = 20m }
                    },
                    PaymentTerms = "1 day",
                    IsPaid = true
                },
                new InvoiceExpectedResults
                {
                    IsNotNull = true,
                    ExpectedStatus = InvoiceStatuses.Paid,
                    ExpectedTotal = 24.4m,
                    ExpectedVatRate = 22,
                    ExpectedVatTax = 4.4m,
                    ExpectedClientCompany = false
                }
            },
            new object[] {
                "Expected result: Invoice should be generated," +
                "Expired without vat because service not a vat payer",
                new ExternalInvoice
                {
                    InvoiceNumber = 1,
                    ServiceProviderName = "Service Provider in Itali",
                    ServiceProviderAddress = "Via Antonio Magliabechi, 5:Italy:Firenze FI:50122",
                    ProviderIsVatPayer = false,
                    ClientName = "Vardenis Lietuvenis",
                    ClientAddress = "Vaižganto g. 24:Lithuania:Kaunas:44229",
                    ClientIsNotACompany = true,
                    SupplyServiceDate = new DateTime(2023, 4, 1),
                    Items = new List<ExternalInvoiceItem>
                    {
                        new ExternalInvoiceItem { Description = "Item B", Quantity = 2, Price = 20m }
                    },
                    PaymentTerms = "1 day",
                    IsPaid = false
                },
                new InvoiceExpectedResults
                {
                    IsNotNull = true,
                    ExpectedStatus = InvoiceStatuses.Expired,
                    ExpectedTotal = 40.00m,
                    ExpectedVatRate = 0,
                    ExpectedVatTax = 0m,
                    ExpectedClientCompany = false
                }
            },
            new object[] {
                "Expected result: Invoice should be generated," +
                "Cancelled with no total",
                new ExternalInvoice
                {
                    InvoiceNumber = 1,
                    ServiceProviderName = "Service Provider in LIthuania",
                    ServiceProviderAddress = "Lentupio g. 12A:Lithuania:Vilnius:10218",
                    ProviderIsVatPayer = true,
                    ClientName = "Vardenis Lietuvenis",
                    ClientAddress = "Vaižganto g. 24:Lithuania:Kaunas:44229",
                    ClientIsNotACompany = true,
                    SupplyServiceDate = new DateTime(2023, 4, 1),
                    Items = new List<ExternalInvoiceItem>(),
                    PaymentTerms = "Never",
                    IsPaid = false
                },
                new InvoiceExpectedResults
                {
                    IsNotNull = true,
                    ExpectedStatus = InvoiceStatuses.Cancelled,
                    ExpectedTotal = 0.00m,
                    ExpectedVatRate = 21,
                    ExpectedVatTax = 0m,
                    ExpectedClientCompany = false
                }
            },
            new object[] {
                "Expected result: Invoice not generated," +
                "missing client address",
                new ExternalInvoice
                {
                    InvoiceNumber = 1,
                    ServiceProviderName = "Service Provider in LIthuania",
                    ServiceProviderAddress = "Lentupio g. 12A:Lithuania:Vilnius:10218",
                    ProviderIsVatPayer = true,
                    ClientName = "Vardenis Lietuvenis",
                    ClientIsNotACompany = true,
                    SupplyServiceDate = new DateTime(2023, 5, 1),
                    Items = new List<ExternalInvoiceItem>(),
                    PaymentTerms = "30 days",
                    IsPaid = false
                },
                new InvoiceExpectedResults
                {
                    IsNotNull = false
                }
            }
        };
        [Theory]
        [MemberData(nameof(ExternalInvoiceTestData))]
        public async Task GenerateInvoice(string description,
                ExternalInvoice externalInvoice,
                InvoiceExpectedResults expectedResult)
        {
            // Arange
            // Act
            var result = await _sut.GenerateInvoice(externalInvoice);

            // Assert
            Assert.True((result != null) == expectedResult.IsNotNull, description);
            if (expectedResult.IsNotNull)
            {
                Assert.True(result.InvoiceStatus == expectedResult.ExpectedStatus, $"Expected status failed for {description}");
                Assert.True(result.Total == expectedResult.ExpectedTotal, $"Expected total failed for {description}");
                Assert.True(result.VatRate == expectedResult.ExpectedVatRate, $"Expected vat rate failed for {description}");
                Assert.True(result.VatTax == expectedResult.ExpectedVatTax, $"Expected vat tax failed for {description}");
                Assert.True((result.Customer.Company != null) == expectedResult.ExpectedClientCompany, $"Expected is company failed for {description}");
                Assert.True((result.Customer.Individual != null) == !expectedResult.ExpectedClientCompany, $"Expected is individual failed for {description}");
            }
        }
    }
}
