using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests
{
    public class ReportRequest
    {
        [JsonProperty("registrationDateFrom")] public string RegistrationDateFrom { get; set; }
        [JsonProperty("registrationDateTo")] public string RegistrationDateTo { get; set; }
        [JsonProperty("firstDepositDateFrom")] public string FirstDepositDateFrom { get; set; }
        [JsonProperty("firstDepositDateTo")] public string FirstDepositDateTo { get; set; }
        [JsonProperty("firstDeposit")] public bool FirstDeposit { get; set; }
        [JsonProperty("page")] public int Page { get; set; }
    }
}
