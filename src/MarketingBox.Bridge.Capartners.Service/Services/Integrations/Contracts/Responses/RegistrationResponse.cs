using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Enums;
using Newtonsoft.Json;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public class RegistrationResponse
    {
        [JsonProperty("profileUUID")]
        public string ProfileUUID { get; set; }
        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }
    }
}