using MyYamlParser;

namespace MarketingBox.Bridge.Capartners.Service.Settings
{
    public class SettingsModel
    {
        [YamlProperty("MarketingBoxCapartnersgBridge.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("MarketingBoxCapartnersgBridge.JaegerUrl")]
        public string JaegerUrl { get; set; }

        [YamlProperty("MarketingBoxCapartnersgBridge.Brand.Url")]
        public string BrandUrl { get; set; }

        [YamlProperty("MarketingBoxCapartnersgBridge.Brand.AffiliateId")]
        public string BrandAffiliateId { get; set; }

        [YamlProperty("MarketingBoxCapartnersgBridge.Brand.BrandId")]
        public string BrandBrandId { get; set; }

        [YamlProperty("MarketingBoxCapartnersgBridge.Brand.AffiliateKey")]
        public string BrandAffiliateKey { get; set; }
    }
}
