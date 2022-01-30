#nullable enable
using System.Collections.Generic;
using Destructurama.Attributed;
using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests
{
    public class RegistrationRequest
    {
        [LogMasked]
        [JsonProperty("firstName")]
        public string? FirstName { get; set; }

        [LogMasked]
        [JsonProperty("lastName")]
        public string? LastName { get; set; }

        [LogMasked(ShowFirst = 3)]
        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 3)]
        [JsonProperty("email")]
        public string? Email { get; set; }

        [LogMasked]
        [JsonProperty("password")]
        public string? Password { get; set; }

        [LogMasked(ShowFirst = 1, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("ip")]
        public string? Ip { get; set; }

        [JsonProperty("userAgent")]
        public string? UserAgent { get; set; }

        [JsonProperty("affId")]
        public int AffId { get; set; }
        [JsonProperty("trafficSource")]
        public string? TrafficSource { get; set; }

        [LogMasked]
        [JsonProperty("secretKey")]
        public string? SecretKey { get; set; }

        [JsonProperty("brandId")]
        public string? BrandId { get; set; }

        [JsonProperty("cxdToken")]
        public string? CxdToken { get; set; }

        [JsonProperty("langId")]
        public string? LangId { get; set; }

        [JsonProperty("countryOfRegistration")]
        public string? CountryOfRegistration { get; set; }

        [JsonProperty("landingPage")]
        public string? LandingPage { get; set; }

        [JsonProperty("redirectedFromUrl")]
        public string? RedirectedFromUrl { get; set; }

        [JsonProperty("countryByIp")]
        public string? CountryByIp { get; set; }

        [JsonProperty("processId")]
        public string? ProcessId { get; set; }

        [JsonProperty("target")]
        public string? Target { get; set; }

        [JsonProperty("cake")]
        public IDictionary<string, string>? Cake { get; set; }
    }
}