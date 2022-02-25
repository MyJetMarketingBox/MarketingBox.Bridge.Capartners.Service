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
                Source = request.AdditionalInfo.Sub
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

        private static ReportRequest MapRegistrationToApi(IntegrationBridge.ReportingRequest request)
        {
            return new ReportRequest()
            {
                RegistrationDateFrom = CalendarUtils
                    .StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                RegistrationDateTo = CalendarUtils
                    .EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                FirstDepositDateFrom = CalendarUtils
                    .StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                FirstDepositDateTo = CalendarUtils
                    .EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                FirstDeposit = false,
                Page = request.PageIndex,
            };
        }

        private static Response<IReadOnlyCollection<RegistrationReporting>> SuccessMapToGrpc(
            ReportRegistrationResponse brandRegistrations)
        {
            var registrations = brandRegistrations
                .Items
                .Where(s => s.FirstDeposit == false)
                .Select(report => new MarketingBox.Integration.Service.Grpc.Models.Registrations.RegistrationReporting
                {
                    Crm = MapCrmStatus(report.SalesStatus),
                    CustomerEmail = report.Email,
                    CustomerId = report.ClientUUID,
                    CreatedAt = Convert.ToDateTime(report.RegistrationDate),
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
            switch (status.ToUpper())
            {
                case "NEW":
                case "TEST":
                    return CrmStatus.New;

                case "DEPOSITOR":
                case "CONVERTED":
                case "SELF_DEPOSITOR":
                    return CrmStatus.FullyActivated;

                case "POTENTIAL_HIGH":
                case "HOT":
                    return CrmStatus.HighPriority;

                case "INITIAL_CALL":
                case "CALLBACK":
                case "LONG_TERM_CALL_BACK":
                case "CALL_BACK_INSFF":
                    return CrmStatus.Callback;

                case "NO_MONEY":
                case "POTENTIAL_LOW":
                case "FAILED_DEPOSIT":
                case "JUNK_LEAD":
                case "EXPECTATION":
                case "POTENTIAL_FRAUD":
                case "PENDING":
                    return CrmStatus.FailedExpectation;

                case "WRONG_INFO":
                case "WRONG_NUMBER":
                case "INVALID_LANGUAGE":
                case "INVALID_COUNTRY":
                case "DUPLICATE":
                case "UNDER_18":
                case "BLACK_LIST_COUNTRY":
                    return CrmStatus.NotValid;

                case "NO_INTEREST":
                case "HUNG_UP":
                case "DO_NOT_CALL":
                    return CrmStatus.NotInterested;

                case "WIRE_SENT":
                    return CrmStatus.Transfer;

                case "DIALER_ASSIGNED":
                case "DIALER_DROP":
                case "DIALER_NA":
                case "DIALER_NEW":
                case "PUBLIC_NUMBER":
                case "SHARED_3":
                case "SHARED_2":
                case "SHARED":
                case "MEDIA":
                    return CrmStatus.FollowUp;

                case "NEVER_ANSWER":
                case "VOICEMAIL":
                case "NO_ANSWER":
                case "NO_ANSWER_2":
                case "NO_ANSWER_3":
                case "NO_ANSWER_4":
                case "NO_ANSWER_5":

                    return CrmStatus.NA;

                case "REASSIGN":
                    return CrmStatus.ConversionRenew;

                default:
                    Console.WriteLine($"Unknown crm status {status}");
                    return CrmStatus.Unknown;
            }
        }

        private static ReportRequest MapDepositsToApi(IntegrationBridge.ReportingRequest request)
        {
            return new ReportRequest()
            {
                RegistrationDateFrom = CalendarUtils
                    .StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                RegistrationDateTo = CalendarUtils
                    .EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                FirstDepositDateFrom = CalendarUtils
                    .StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                FirstDepositDateTo = CalendarUtils
                    .EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month))
                    .ToString("yyyy-MM-dd"),
                FirstDeposit = true,
                Page = request.PageIndex,
            };
        }

        private static Response<IReadOnlyCollection<DepositorReporting>> SuccessMapToGrpc(
            ReportDepositResponse brandDeposits, DateTime from, DateTime to)
        {
            var registrations = brandDeposits
                .Items
                .Where(s => s.FirstDeposit &&
                            (Convert.ToDateTime(s.FirstDepositDate) >= from &&
                             Convert.ToDateTime(s.FirstDepositDate) <= to))
                .Select(report => new DepositorReporting
                {
                    CustomerEmail = report.Email,
                    CustomerId = report.ClientUUID,
                    DepositedAt = Convert.ToDateTime(report.FirstDepositDate),
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
        public async Task<Response<CustomerInfo>> SendRegistrationAsync(IntegrationBridge.RegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new LeadInfo {@context}", request);

                var brandRequest = MapToApi(request);

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
        public async Task<Response<IReadOnlyCollection<RegistrationReporting>>> GetRegistrationsPerPeriodAsync(
            IntegrationBridge.ReportingRequest request)
        {
            try
            {
                _logger.LogInformation("GetRegistrationsPerPeriodAsync {@context}", request);

                var brandRequest = MapRegistrationToApi(request);

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
        public async Task<Response<IReadOnlyCollection<DepositorReporting>>> GetDepositorsPerPeriodAsync(
            IntegrationBridge.ReportingRequest request)
        {
            try
            {
                _logger.LogInformation("GetRegistrationsPerPeriodAsync {@context}", request);

                var brandRequest = MapDepositsToApi(request);

                // Get deposits
                var depositsResult = await _capartnersHttpClient.GetDepositsAsync(brandRequest);
                // Failed
                if (depositsResult.IsFailed)
                {
                    throw new Exception(depositsResult.FailedResult.Message);
                }

                // Success
                return SuccessMapToGrpc(depositsResult.SuccessResult, request.DateFrom, request.DateTo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error get registration reporting {@context}", request);
                return e.FailedResponse<IReadOnlyCollection<DepositorReporting>>();
            }
        }
    }
}