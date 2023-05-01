using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InvoiceSystem.Core.ExternalServices
{
    public class CountryService
    {
        private const string BaseUrl = "http://api.geonames.org";
        private const string geoNamesUsername = "brightraudona";
        public async Task<XElement> GetCountryDataXMLByAddress(string country)
        {
            using var httpClient = new HttpClient();
            var url = $"{BaseUrl}/search?q={country}&featureCode=PCLI&maxRows=1&lang=en&username={geoNamesUsername}";

            var response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            var xdoc = XDocument.Parse(xml);
            var element = xdoc.Element("geonames").Element("geoname");

            return element;
        }
    }
}
