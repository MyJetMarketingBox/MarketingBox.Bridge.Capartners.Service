using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public class ReportDepositResponse
    {
        [JsonProperty("totalElements")] public int TotalElements { get; set; }
        [JsonProperty("totalPages")] public int TotalPages { get; set; }
        [JsonProperty("content")] public IReadOnlyCollection<ReportDepositModel> Items { get; set; }
        [JsonProperty("last")] public int Last { get; set; }
        [JsonProperty("page")] public int Page { get; set; }
        [JsonProperty("size")] public int Size { get; set; }
    }

    public class ReportDepositModel
    {
        [JsonProperty("clientUUID")] public string ClientUUID { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("registrationDate")] public DateTime RegistrationDate { get; set; }
        [JsonProperty("firstDeposit")] public bool FirstDeposit { get; set; }
        [JsonProperty("firstDepositDate")] public DateTime FirstDepositDate { get; set; }
        [JsonProperty("salesStatus")] public string SalesStatus { get; set; }
        [JsonProperty("source")] public string Source { get; set; }
        [JsonProperty("referral")] public string Referral { get; set; }
    }

}