using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Bridge.Capartners.Service.Services;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Settings;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MarketingBox.Bridge.Capartners.Service.Tests
{
    public class TestHttpCreateRegistration
    {
        private Activity _unitTestActivity;
        private SettingsModel _settingsModel;
        private CapartnersHttpClient _httpClient;
        private static Random random = new Random();
        private ILogger<BridgeService> _logger;
        private BridgeService _registerService;

        public void Dispose()
        {
            _unitTestActivity.Stop();
        }

        public static string RandomDigitString(int length)
        {
            const string chars = "123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [SetUp]
        public void Setup()
        {
            _settingsModel = new SettingsModel()
            {
                SeqServiceUrl = "http://192.168.1.80:5341",
                BrandAffiliateId = "***",
                BrandAffiliateKey = "***",
                BrandBrandId = "Capartners",
                BrandUrl = "https://partner.capartners.cc",
            };

            _unitTestActivity = new Activity("UnitTest").Start();
            _httpClient = new CapartnersHttpClient(_settingsModel.BrandUrl, 
                _settingsModel.BrandAffiliateId, _settingsModel.BrandAffiliateKey);
            _logger = Mock.Of<ILogger<BridgeService>>();
            _registerService = new BridgeService(_logger, _httpClient, _settingsModel);
        }

        [Test]
        public async Task DirectHttpSend()
        {

#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.UtcNow;
            var request = new RegistrationRequest()
            {
                //-----
                FirstName = "Yuriy",
                LastName = "Test",
                Phone = "+79995556677",
                Email = "yuriy.test." + dt.ToString("yyyy.MM.dd") + "." + RandomDigitString(3) + "@mailinator.com",
                Password = "Trader123",
                Ip = "99.99.99.99",
                Country = "PL",
                Language = "EN",
                Referral = @"https://redirectedFromUrl.online/nb_1st_pfizer_hp_st_pl/?sub_id=101211&offer_id=28"
            };

            var result = await _httpClient.RegisterTraderAsync(request);
            Assert.AreEqual(true, !result.IsFailed);
        }

        [Test]
        public async Task ServiceHttpSend()
        {
#if !DEBUG
            Assert.Pass();
#endif
            var dt = DateTime.UtcNow;
            var request = new RegistrationRequest()
            {
                FirstName = "Yuriy",
                LastName = "Test",
                Phone = "+79995556677",
                Email = "yuriy.test." + dt.ToString("yyyy.MM.dd") + "." + RandomDigitString(3) + "@mailinator.com",
                Password = "Trader123",
                Ip = "99.99.99.99",
                Country = "PL",
                Language = "EN",
                Referral = @"https://redirectedFromUrl.online/nb_1st_pfizer_hp_st_pl/?sub_id=101211&offer_id=28"
            };

            var result = await _registerService.RegisterExternalCustomerAsync(request);
            Assert.AreEqual(ResponseStatus.Ok, result.Status);
        }

        [Test]
        public async Task ServiceAlreadyExistHttpSend()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt1 = DateTimeOffset.Now;
            var dt2 = dt1.AddMilliseconds(100);
            var email = "yuriy.test." + dt1.ToString("yyyy.MM.dd") + "." + RandomDigitString(3) + "@mailinator.com";
            var processId1 = dt1.ToString("yyyy-MM-ddThh:mm:ss.fffZ") + " " + email;
            var processId2 = dt2.ToString("yyyy-MM-ddThh:mm:ss.fffZ") + " " + email;

            var request1 = new RegistrationRequest()
            {
                FirstName = "Yuriy",
                LastName = "Test",
                Phone = "+79995556677",
                Email = email,
                Password = "Trader123",
                Ip = "99.99.99.99",
                Country = "PL",
                Language = "EN",
                Referral = @"https://redirectedFromUrl.online/nb_1st_pfizer_hp_st_pl/?sub_id=101211&offer_id=28"
            };

            var request2 = new RegistrationRequest()
            {
                FirstName = "Yuriy",
                LastName = "Test",
                Phone = "+79995556677",
                Email = email,
                Password = "Trader123",
                Ip = "99.99.99.99",
                Country = "PL",
                Language = "EN",
                Referral = @"https://redirectedFromUrl.online/nb_1st_pfizer_hp_st_pl/?sub_id=101211&offer_id"
            };
            var result = await _registerService.RegisterExternalCustomerAsync(request1);
            Assert.AreEqual(ResponseStatus.Ok, result.Status);
            // The same registration with another time window
            Assert.ThrowsAsync<Exception>(async () =>
                result = await _registerService.RegisterExternalCustomerAsync(request2));
        }

        [Test]
        public async Task ServiceDoubleClickHttpSend()
        {

#if !DEBUG
            Assert.Pass();
#endif            
            var dt = DateTimeOffset.Now;
            var email = "yuriy.test." + dt.ToString("yyyy.MM.dd") + "." + RandomDigitString(3) + "@mailinator.com";
            var processId = dt.ToString("yyyy-MM-ddThh:mm:ss.fffZ") + " " + email;


            var request1 = new RegistrationRequest()
            {
                FirstName = "Yuriy",
                LastName = "Test",
                Phone = "+79995556677",
                Email = email,
                Password = "Trader123",
                Ip = "99.99.99.99",
                Country = "PL",
                Language = "EN",
                Referral = @"https://redirectedFromUrl.online/nb_1st_pfizer_hp_st_pl/?sub_id=101211&offer_id"
            };

            var request2 = new RegistrationRequest()
            {
                FirstName = "Yuriy",
                LastName = "Test",
                Phone = "+79995556677",
                Email = email,
                Password = "Trader123",
                Ip = "99.99.99.99",
                Country = "PL",
                Language = "EN",
                Referral = @"https://redirectedFromUrl.online/nb_1st_pfizer_hp_st_pl/?sub_id=101211&offer_id"
            };
            var result = await _registerService.RegisterExternalCustomerAsync(request1);
            Assert.AreEqual(ResponseStatus.Ok, result.Status);
            // The same registration with another time window
            result = await _registerService.RegisterExternalCustomerAsync(request2);
            Assert.AreEqual(ResponseStatus.Ok, result.Status);
        }
    }
}
