using Destructurama.Attributed;
using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests
{
    public class ReportCountersRequest
    {
        [JsonProperty("year")] public int Year { get; set; }
        [JsonProperty("month")] public int Month { get; set; }
        [JsonProperty("apiKey")] [LogMasked] public string ApiKey { get; set; }
    }

}