using Autofac;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations;

namespace MarketingBox.Bridge.Capartners.Service.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CapartnersHttpClient>()
                .As<ICapartnersHttpClient>()
                .WithParameter("baseUrl", Program.ReloadedSettings(e => e.BrandUrl).Invoke())
                .WithParameter("login", Program.ReloadedSettings(e => e.BrandAffiliateId).Invoke())
                .WithParameter("password", Program.ReloadedSettings(e => e.BrandAffiliateKey).Invoke())
                .SingleInstance();
        }
    }
}
