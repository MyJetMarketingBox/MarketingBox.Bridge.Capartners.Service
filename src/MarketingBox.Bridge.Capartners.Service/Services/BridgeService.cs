using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Bridge.Capartners.Service.Domain.Utils;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses;
using MarketingBox.Bridge.Capartners.Service.Settings;
using MarketingBox.Integration.Bridge.Client;
using MarketingBox.Integration.Service.Domain.Registrations;
using MarketingBox.Integration.Service.Grpc.Models.Registrations;
using MarketingBox.Sdk.Common.Extensions;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;
using IntegrationBridge = MarketingBox.Integration.Service.Grpc.Models.Registrations.Contracts.Bridge;

namespace MarketingBox.Bridge.Capartners.Service.Services
{
    public class BridgeService : IBridgeService
    {
        private readonly ILogger<BridgeService> _logger;
        private readonly ICapartnersHttpClient _capartnersHttpClient;
        private readonly SettingsModel _settingsModel;

        public async Task<Response<CustomerInfo>> RegisterExternalCustomerAsync(
            RegistrationRequest brandRequest)
        {
            var registerResult =
                await _capartnersHttpClient.RegisterTraderAsync(brandRequest);

            // Failed
            if (registerResult.IsFailed)
            {
                throw new Exception(registerResult.FailedResult.Message);
            }

            // Success
            return SuccessMapToGrpc(registerResult.SuccessResult);
        }

        private static RegistrationRequest MapToApi(IntegrationBridge.RegistrationRequest request)
        {
            return new RegistrationRequest()
            {
                FirstName = request.Info.FirstName,
                LastName = request.Info.LastName,
                Password = request.Info.Password,
                Email = request.Info.Email,
                Phone = request.Info.Phone,
                Language = request.Info.Language,
                Ip = request.Info.Ip,
                Country = request.Info.Country,
                Referral = request.AdditionalInfo.So,
            };
        }

        private static Response<CustomerInfo> SuccessMapToGrpc(RegistrationResponse brandRegistrationInfo)
        {
            return new Response<CustomerInfo>
            {
                Status = ResponseStatus.Ok,
                Data = new CustomerInfo()
                {
                    CustomerId = brandRegistrationInfo.ProfileUUID,
                    LoginUrl = brandRegistrationInfo.RedirectUrl,
                    Token = string.Empty,
                }
            };
        }
        
        private ReportRequest MapRegistrationToApi(
            IntegrationBridge.ReportingRequest request)
        {
            return new ReportRequest()
            {
                RegistrationDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                RegistrationDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                //FirstDepositDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                //FirstDepositDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDeposit = false,
                Page = request.PageIndex,
            };
        }

        private static Response<IReadOnlyCollection<RegistrationReporting>> SuccessMapToGrpc(ReportRegistrationResponse brandRegistrations)
        {
            var registrations = brandRegistrations.Items.Select(report => new RegistrationReporting
            {
                Crm = MapCrmStatus(report.CrmStatus),
                CustomerEmail = report.Email,
                CustomerId = report.UserId,
                CreatedAt = report.CreatedAt,
                CrmUpdatedAt = DateTime.UtcNow
            }).ToList();

            return new Response<IReadOnlyCollection<RegistrationReporting>>()
            {
                Status = ResponseStatus.Ok,
                Data = registrations
            };
        }

        private static CrmStatus MapCrmStatus(string status)
        {
            switch (status.ToLower())
            {
                case "new":
                    return CrmStatus.New;

                case "fullyactivated":
                    return CrmStatus.FullyActivated;

                case "highpriority":
                    return CrmStatus.HighPriority;

                case "callback":
                    return CrmStatus.Callback;

                case "failedexpectation":
                    return CrmStatus.FailedExpectation;

                case "notvaliddeletedaccount":
                case "notvalidwrongnumber":
                case "notvalidnophonenumber":
                case "notvalidduplicateuser":
                case "notvalidtestlead":
                case "notvalidunderage":
                case "notvalidnolanguagesupport":
                case "notvalidneverregistered":
                case "notvalidnoneligiblecountries":
                    return CrmStatus.NotValid;

                case "notinterested":
                    return CrmStatus.NotInterested;

                case "transfer":
                    return CrmStatus.Transfer;

                case "followup":
                    return CrmStatus.FollowUp;

                case "noanswer":
                case "autocall":
                    return CrmStatus.NA;

                case "conversionrenew":
                    return CrmStatus.ConversionRenew;

                default:
                    return CrmStatus.Unknown;
            }
        }
        
        private static ReportRequest MapDepositsToApi(IntegrationBridge.ReportingRequest request)
        {
            return new ReportRequest()
            {
                //RegistrationDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                //RegistrationDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDepositDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDepositDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDeposit = true,
                Page = request.PageIndex,
            };
        }

        private static Response<IReadOnlyCollection<DepositorReporting>> SuccessMapToGrpc(ReportDepositResponse brandDeposits)
        {
            var registrations = brandDeposits.Items.Select(report => new DepositorReporting
            {
                CustomerEmail = report.Email,
                CustomerId = report.UserId,
                DepositedAt = report.CreatedAt,
            }).ToList();

            return new Response<IReadOnlyCollection<DepositorReporting>>
            {
                Status = ResponseStatus.Ok,
                Data = registrations
            };
        }

        public BridgeService(ILogger<BridgeService> logger,
            ICapartnersHttpClient capartnersHttpClient, SettingsModel settingsModel)
        {
            _logger = logger;
            _capartnersHttpClient = capartnersHttpClient;
            _settingsModel = settingsModel;
        }
        /// <summary>
        /// Register new lead
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<CustomerInfo>> SendRegistrationAsync(
            IntegrationBridge.RegistrationRequest request)
        {
            _logger.LogInformation("Creating new LeadInfo {@context}", request);

            var brandRequest = MapToApi(request);

            try
            {
                return await RegisterExternalCustomerAsync(brandRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating lead {@context}", request);

                return e.FailedResponse<CustomerInfo>();
            }
        }

        /// <summary>
        /// Get all registrations per period
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<IReadOnlyCollection<RegistrationReporting>>> GetRegistrationsPerPeriodAsync(IntegrationBridge.ReportingRequest request)
        {
            _logger.LogInformation("GetRegistrationsPerPeriodAsync {@context}", request);

            var brandRequest = MapRegistrationToApi(request);

            try
            {
                // Get registrations
                var registerResult = await _capartnersHttpClient.GetRegistrationsAsync(brandRequest);
                // Failed
                if (registerResult.IsFailed)
                {
                    throw new Exception(registerResult.FailedResult.Message);
                }

                // Success
                return SuccessMapToGrpc(registerResult.SuccessResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error get registration reporting {@context}", request);
                return e.FailedResponse<IReadOnlyCollection<RegistrationReporting>>();
            }
        }

        /// <summary>
        /// Get all deposits per period
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<IReadOnlyCollection<DepositorReporting>>> GetDepositorsPerPeriodAsync(IntegrationBridge.ReportingRequest request)
        {
            _logger.LogInformation("GetRegistrationsPerPeriodAsync {@context}", request);

            var brandRequest = MapDepositsToApi(request);

            try
            {
                // Get deposits
                var depositsResult = await _capartnersHttpClient.GetDepositsAsync(brandRequest);
                // Failed
                if (depositsResult.IsFailed)
                {
                    throw new Exception(depositsResult.FailedResult.Message);
                }

                // Success
                return SuccessMapToGrpc(depositsResult.SuccessResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error get registration reporting {@context}", request);
                return e.FailedResponse<IReadOnlyCollection<DepositorReporting>>();
            }
        }
    }
}