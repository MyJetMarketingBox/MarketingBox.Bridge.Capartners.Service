using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Bridge.Capartners.Service.Services;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Settings;
using MarketingBox.Integration.Service.Grpc.Models.Registrations.Contracts.Bridge;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MarketingBox.Bridge.Capartners.Service.Tests
{
    public class TestHttpReports
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
        public async Task HttpGetRegistrationsAsync()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.Parse("2021-12-01");
            var request = new ReportRequest
            {
                RegistrationDateFrom = "2022-01-01",
                RegistrationDateTo = "2022-01-31",
                //FirstDepositDateFrom = "2022-01-01",
                //FirstDepositDateTo = "2022-01-31",
                FirstDeposit = false,
                Page = 1
            };

            var result = await _httpClient.GetRegistrationsAsync(request);
            Assert.IsFalse(result.IsFailed);
        }


        [Test]
        public async Task HttpGetDepositsAsync()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.Parse("2021-12-01");
            var request = new ReportRequest()
            {
                //RegistrationDateFrom = "2022-01-01",
                //RegistrationDateTo = "2022-01-31",
                FirstDepositDateFrom = "2022-01-01",
                FirstDepositDateTo = "2022-01-31",
                FirstDeposit = true,
                Page = 1
            };

            var result = await _httpClient.GetDepositsAsync(request);
            Assert.IsFalse(result.IsFailed);
        }


        [Test]
        public async Task GetRegistrationsPerPeriodAsync()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.Parse("2021-12-01");
            var request = new ReportingRequest()
            {
                DateFrom = dt.AddMonths(-1),
                DateTo = dt,
                PageIndex = 1,
                PageSize = 100,
            };

            var result = await _registerService.GetRegistrationsPerPeriodAsync(request);
            Assert.IsTrue(result.Data.Count > 0);
        }

        [Test]
        public async Task GetRegistrationsPerPeriodEmptyAsync()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.Parse("2020-02-01");
            var request = new ReportingRequest()
            {
                DateFrom = dt.AddMonths(-1),
                DateTo = dt,
                PageIndex = 1,
                PageSize = 100,
            };

            var result = await _registerService.GetRegistrationsPerPeriodAsync(request);
            Assert.IsTrue(result.Data.Count == 0);
        }

        [Test]
        public async Task GetDepositorsPerPeriodAsync()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.Parse("2021-12-01");
            var request = new ReportingRequest()
            {
                DateFrom = dt.AddMonths(-1),
                DateTo = dt,
                PageIndex = 1,
                PageSize = 100,
            };

            var result = await _registerService.GetDepositorsPerPeriodAsync(request);
            Assert.IsTrue(result.Data.Count > 0);
        }

        [Test]
        public async Task GetDepositorsPerPeriodEmptyAsync()
        {
#if !DEBUG
            Assert.Pass();
#endif

            var dt = DateTime.Parse("2020-02-01");
            var request = new ReportingRequest()
            {
                DateFrom = dt.AddMonths(-1),
                DateTo = dt,
                PageIndex = 1,
                PageSize = 100,
            };

            var result = await _registerService.GetDepositorsPerPeriodAsync(request);
            Assert.IsTrue(result.Data.Count == 0);
        }
    }
}
