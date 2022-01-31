using JetBrains.Annotations;
using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public class FailedResult
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("error")] [CanBeNull] public string Error { get; set; }
        [JsonProperty("fields")] [CanBeNull] public string Fields { get; set; }
    }
}