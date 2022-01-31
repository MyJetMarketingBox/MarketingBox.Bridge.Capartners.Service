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
        public string? FirstName { get; set; }  // required

        [LogMasked]
        [JsonProperty("lastName")]
        public string? LastName { get; set; }  // required

        [JsonProperty("country")]
        public string? Country { get; set; } // ISO 639-1:2002 (DE, CN, etc.)

        [JsonProperty("gender")]
        public string? Gender { get; set; }  // MALE or FEMALE

        [LogMasked(ShowFirst = 2, ShowLast = 3)]
        [JsonProperty("email")]
        public string? Email { get; set; }  // required

        [LogMasked]
        [JsonProperty("password")]
        public string? Password { get; set; } // required, 6-16 symbols, at least one letter and one digit

        [LogMasked(ShowFirst = 3)]
        [JsonProperty("phone")]
        public string? Phone { get; set; }   // required

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonProperty("referral")]
        public string? Referral { get; set; }

        [LogMasked(ShowFirst = 1, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("ip")]
        public string? Ip { get; set; }

        [LogMasked]
        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("postCode")]
        public string? PostCode { get; set; }

        [JsonProperty("birthday")]
        public string? Birthday { get; set; }
    }
}