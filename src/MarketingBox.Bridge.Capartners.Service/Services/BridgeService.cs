using System;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ISimpleTradingHttpClient _simpleTradingHttpClient;
        private readonly SettingsModel _settingsModel;

        public BridgeService(ILogger<BridgeService> logger,
            ISimpleTradingHttpClient simpleTradingHttpClient, SettingsModel settingsModel)
        {
            _logger = logger;
            _simpleTradingHttpClient = simpleTradingHttpClient;
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

            var brandRequest = MapToApi(request, _settingsModel.BrandBrandId,
                Convert.ToInt32(_settingsModel.BrandAffiliateId), _settingsModel.BrandAffiliateKey,
                DateTimeOffset.UtcNow.ToString());

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
                await _simpleTradingHttpClient.RegisterTraderAsync(brandRequest);

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
            if (registerResult.SuccessResult.IsSuccessfully() &&
                (SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.Ok)
            {
                // Success
                return SuccessMapToGrpc(registerResult.SuccessResult);
            }

            // Success, but software failure
            if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.UserExists)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Registration already exists",
                    Type = ErrorType.AlreadyExist
                }, ResultCode.Failed);
            }

            if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.InvalidUserNameOrPassword)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Invalid username or password",
                    Type = ErrorType.InvalidUserNameOrPassword
                }, ResultCode.Failed);
            }

            if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.PersonalDataNotValid)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Registration data not valid",
                    Type = ErrorType.InvalidPersonalData
                }, ResultCode.Failed);
            }

            if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.SystemError)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Brand Error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }

            return FailedMapToGrpc(new Error()
            {
                Message = "Unknown Error",
                Type = ErrorType.Unknown
            }, ResultCode.Failed);
        }

        private RegistrationRequest MapToApi(IntegrationBridge.RegistrationRequest request,
            string authBrandId, int authAffId, string authAffApiKey, string requestId)
        {
            return new RegistrationRequest()
            {
                FirstName = request.Info.FirstName,
                LastName = request.Info.LastName,
                Password = request.Info.Password,
                Email = request.Info.Email,
                Phone = request.Info.Phone,
                LangId = request.Info.Language,
                Ip = request.Info.Ip,
                CountryByIp = request.Info.Country,
                AffId = authAffId,
                BrandId = authBrandId,
                SecretKey = authAffApiKey,
                ProcessId = requestId,
                CountryOfRegistration = request.Info.Country,
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
                    CustomerId = brandRegistrationInfo.TraderId,
                    LoginUrl = brandRegistrationInfo.RedirectUrl,
                    Token = brandRegistrationInfo.Token
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

            var brandRequest = MapToApi(request, _settingsModel.BrandAffiliateKey);

            try
            {
                // Get registrations
                var registerResult = await _simpleTradingHttpClient.GetRegistrationsAsync(brandRequest);
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

        private ReportRequest MapToApi(
            IntegrationBridge.ReportingRequest request,
            string authAffApiKey)
        {
            return new ReportRequest()
            {
                Year = request.DateFrom.Year,
                Month = request.DateFrom.Month,
                Page = request.PageIndex,
                PageSize = request.PageSize,
                ApiKey = authAffApiKey
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
            var registrations = brandRegistrations.Items.Select(report => new MarketingBox.Integration.Service.Grpc.Models.Registrations.RegistrationReporting
            {
                Crm = MapCrmStatus(report.CrmStatus),
                CustomerEmail = report.Email,
                CustomerId = report.UserId,
                CreatedAt = report.CreatedAt,
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

        /// <summary>
        /// Get all deposits per period
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IntegrationBridge.DepositorsReportingResponse> GetDepositorsPerPeriodAsync(IntegrationBridge.ReportingRequest request)
        {
            _logger.LogInformation("GetRegistrationsPerPeriodAsync {@context}", request);

            var brandRequest = MapToApi(request, _settingsModel.BrandAffiliateKey);

            try
            {
                // Get deposits
                var depositsResult = await _simpleTradingHttpClient.GetDepositsAsync(brandRequest);
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

        public static IntegrationBridge.DepositorsReportingResponse FailedDepositorsMapToGrpc(Error error, ResultCode code)
        {
            return new IntegrationBridge.DepositorsReportingResponse()
            {
                Error = error
            };
        }

        public static IntegrationBridge.DepositorsReportingResponse SuccessMapToGrpc(ReportDepositResponse brandDeposits)
        {
            var registrations = brandDeposits.Items.Select(report => new MarketingBox.Integration.Service.Grpc.Models.Registrations.DepositorReporting
            {
                CustomerEmail = report.Email,
                CustomerId = report.UserId,
                DepositedAt = report.CreatedAt,


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