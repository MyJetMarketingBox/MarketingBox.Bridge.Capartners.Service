using System;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Bridge.Capartners.Service.Domain.Utils;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Enums;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses;
using MarketingBox.Bridge.Capartners.Service.Settings;
using MarketingBox.Bridge.SimpleTrading.Service.Domain.Extensions;
using MarketingBox.Integration.Bridge.Client;
using MarketingBox.Integration.Service.Domain.Registrations;
using MarketingBox.Integration.Service.Grpc.Models.Common;
using Microsoft.Extensions.Logging;
using IntegrationBridge = MarketingBox.Integration.Service.Grpc.Models.Registrations.Contracts.Bridge;

namespace MarketingBox.Bridge.Capartners.Service.Services
{
    public class BridgeService : IBridgeService
    {
        private readonly ILogger<BridgeService> _logger;
        private readonly ICapartnersHttpClient _capartnersHttpClient;
        private readonly SettingsModel _settingsModel;

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
        public async Task<IntegrationBridge.RegistrationResponse> SendRegistrationAsync(
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

                return FailedMapToGrpc(new Error()
                {
                    Message = "Brand response error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }
        }

        public async Task<IntegrationBridge.RegistrationResponse> RegisterExternalCustomerAsync(
            RegistrationRequest brandRequest)
        {
            var registerResult =
                await _capartnersHttpClient.RegisterTraderAsync(brandRequest);

            // Failed
            if (registerResult.IsFailed)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = registerResult.FailedResult.Message,
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }

            // Success
            return SuccessMapToGrpc(registerResult.SuccessResult);
        }

        private RegistrationRequest MapToApi(IntegrationBridge.RegistrationRequest request)
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

        public static IntegrationBridge.RegistrationResponse SuccessMapToGrpc(RegistrationResponse brandRegistrationInfo)
        {
            return new IntegrationBridge.RegistrationResponse()
            {
                ResultCode = ResultCode.CompletedSuccessfully,
                ResultMessage = EnumExtensions.GetDescription(ResultCode.CompletedSuccessfully),
                CustomerInfo = new MarketingBox.Integration.Service.Grpc.Models.Registrations.CustomerInfo()
                {
                    CustomerId = brandRegistrationInfo.ProfileUUID,
                    LoginUrl = brandRegistrationInfo.RedirectUrl,
                    Token = string.Empty,
                }
            };
        }

        public static IntegrationBridge.RegistrationResponse FailedMapToGrpc(Error error, ResultCode code)
        {
            return new IntegrationBridge.RegistrationResponse()
            {
                ResultCode = code,
                ResultMessage = EnumExtensions.GetDescription(code),
                Error = error
            };
        }

        /// <summary>
        /// Get all registrations per period
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationBridge.RegistrationsReportingResponse> GetRegistrationsPerPeriodAsync(IntegrationBridge.ReportingRequest request)
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
                    return FailedReporttingMapToGrpc(new Error()
                    {
                        Message = registerResult.FailedResult.Message,
                        Type = ErrorType.Unknown
                    }, ResultCode.Failed);
                }

                // Success
                return SuccessMapToGrpc(registerResult.SuccessResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error get registration reporting {@context}", request);

                return FailedReporttingMapToGrpc(new Error()
                {
                    Message = "Brand response error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }
        }

        private ReportRequest MapRegistrationToApi(
            IntegrationBridge.ReportingRequest request)
        {
            return new ReportRequest()
            {
                RegistrationDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                RegistrationDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDepositDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDepositDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDeposit = false,
                Page = request.PageIndex,
            };
        }

        public static IntegrationBridge.RegistrationsReportingResponse FailedReporttingMapToGrpc(Error error, ResultCode code)
        {
            return new IntegrationBridge.RegistrationsReportingResponse()
            {
                Error = error
            };
        }

        public static IntegrationBridge.RegistrationsReportingResponse SuccessMapToGrpc(ReportRegistrationResponse brandRegistrations)
        {
            var registrations = brandRegistrations
                .Items
                .Where(s => s.FirstDeposit == false)
                .Select(report => new MarketingBox.Integration.Service.Grpc.Models.Registrations.RegistrationReporting
                {

                    Crm = MapCrmStatus(report.SalesStatus),
                    CustomerEmail = report.Email,
                    CustomerId = report.ClientUUID,
                    CreatedAt = report.RegistrationDate,
                    CrmUpdatedAt = DateTime.UtcNow
                }).ToList();

            return new IntegrationBridge.RegistrationsReportingResponse()
            {
                //ResultCode = ResultCode.CompletedSuccessfully,
                //ResultMessage = EnumExtensions.GetDescription((ResultCode)ResultCode.CompletedSuccessfully),
                Items = registrations
            };
        }

        public static CrmStatus MapCrmStatus(string status)
        {
            switch (status.ToUpper())
            {
                case "NEW":
                case "TEST":
                    return CrmStatus.New;

                case "DEPOSITOR":
                    return CrmStatus.FullyActivated;

                //case "":
                //    return CrmStatus.HighPriority;

                //case "":
                //    return CrmStatus.Callback;

                //case "":
                //    return CrmStatus.FailedExpectation;

                case "WRONG_INFO":
                case "WRONG_NUMBER":
                case "INVALID_LANGUAGE":
                case "INVALID_COUNTRY":
                    return CrmStatus.NotValid;

                case "NO_INTEREST":
                case "HUNG_UP":
                    return CrmStatus.NotInterested;

                //case "":
                //    return CrmStatus.Transfer;

                //case "":
                //    return CrmStatus.FollowUp;

                case "NEVER_ANSWER":
                case "VOICEMAIL":
                    return CrmStatus.NA;

                case "REASSIGN":
                    return CrmStatus.ConversionRenew;

                default:
                    Console.WriteLine($"Unknown crm status {status}");
                    return CrmStatus.Unknown;
            }
        }

        /// <summary>
        /// Get all deposits per period
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationBridge.DepositorsReportingResponse> GetDepositorsPerPeriodAsync(IntegrationBridge.ReportingRequest request)
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
                    return FailedDepositorsMapToGrpc(new Error()
                    {
                        Message = depositsResult.FailedResult.Message,
                        Type = ErrorType.Unknown
                    }, ResultCode.Failed);
                }

                // Success
                return SuccessMapToGrpc(depositsResult.SuccessResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error get registration reporting {@context}", request);

                return FailedDepositorsMapToGrpc(new Error()
                {
                    Message = "Brand response error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }
        }

        private ReportRequest MapDepositsToApi(
            IntegrationBridge.ReportingRequest request)
        {
            return new ReportRequest()
            {
                RegistrationDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                RegistrationDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDepositDateFrom = CalendarUtils.StartOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDepositDateTo = CalendarUtils.EndOfMonth(CalendarUtils.GetDateTime(request.DateFrom.Year, request.DateFrom.Month)).ToString("yyyy-MM-dd"),
                FirstDeposit = true,
                Page = request.PageIndex,
            };
        }

        public static IntegrationBridge.DepositorsReportingResponse FailedDepositorsMapToGrpc(Error error, ResultCode code)
        {
            return new IntegrationBridge.DepositorsReportingResponse()
            {
                Error = error
            };
        }

        public static IntegrationBridge.DepositorsReportingResponse SuccessMapToGrpc(ReportDepositResponse brandDeposits)
        {
            var registrations = brandDeposits
                .Items
                .Where(s => s.FirstDeposit == true)
                .Select(report => new MarketingBox.Integration.Service.Grpc.Models.Registrations.DepositorReporting
            {
                CustomerEmail = report.Email,
                CustomerId = report.ClientUUID,
                DepositedAt = report.FirstDepositDate,
            }).ToList();

            return new IntegrationBridge.DepositorsReportingResponse()
            {
                //ResultCode = ResultCode.CompletedSuccessfully,
                //ResultMessage = EnumExtensions.GetDescription((ResultCode)ResultCode.CompletedSuccessfully),
                Items = registrations
            };
        }
    }
}