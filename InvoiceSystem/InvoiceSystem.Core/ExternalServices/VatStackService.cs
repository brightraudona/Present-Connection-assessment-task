using InvoiceSystem.Core.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvoiceSystem.Core.ExternalServices
{
    public class VatStackService
    {
        private const string BaseUrl = "https://api.vatstack.com/v1";
        private readonly string _apiKey;
        public VatStackService(string apiKey)
        {
            _apiKey = apiKey;
        }
        public async Task<JsonElement> GetVatRates(string countryCode)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            var requestUrl = $"{BaseUrl}/rates?country_code={countryCode}";
            var response = await httpClient.GetAsync(requestUrl);

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(responseStream);

            var s = json.RootElement.ToString();

            return json.RootElement;
        }
    }
}
