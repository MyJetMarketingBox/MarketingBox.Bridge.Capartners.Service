using Autofac;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations;

namespace MarketingBox.Bridge.Capartners.Service.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SimpleTradingHttpClient>()
                .As<ISimpleTradingHttpClient>()
                .WithParameter("baseUrl", Program.ReloadedSettings(e => e.BrandUrl).Invoke())
                .SingleInstance();
        }
    }
}
