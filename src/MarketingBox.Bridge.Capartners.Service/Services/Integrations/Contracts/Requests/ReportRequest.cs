using Destructurama.Attributed;
using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests
{
    public class ReportRequest
    {
        [JsonProperty("year")] public int Year { get; set; }
        [JsonProperty("month")] public int Month { get; set; }
        [JsonProperty("apiKey")] [LogMasked] public string ApiKey { get; set; }
        [JsonProperty("page")] public int Page { get; set; }
        [JsonProperty("pageSize")] public int PageSize { get; set; }
    }
}
