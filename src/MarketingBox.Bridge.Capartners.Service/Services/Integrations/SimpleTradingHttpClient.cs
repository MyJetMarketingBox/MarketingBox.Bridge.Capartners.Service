using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses;
using Newtonsoft.Json;
using Serilog;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations
{
    public class SimpleTradingHttpClient : ISimpleTradingHttpClient
    {
        private readonly string _baseUrl;
        public SimpleTradingHttpClient(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        // Url: /integration/v1/auth/register
        public async Task<Response<RegistrationResponse, FailRegisterResponse>>
            RegisterTraderAsync(RegistrationRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var result = await _baseUrl
                .AppendPathSegments("integration", "v1", "auth", "register")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                .AllowHttpStatus("401")
                .PostJsonAsync(request);
            return await result.ResponseMessage.DeserializeTo<RegistrationResponse, FailRegisterResponse>();
        }

        // Url: /integration/v1/reports/Count
        public async Task<Response<ReportCountersResponse, FailRegisterResponse>> GetCountsAsync(ReportCountersRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var result = await _baseUrl
                .AppendPathSegments("integration", "v1", "reports", "Count")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                .AllowHttpStatus("401")
                .PostJsonAsync(request);
            return await result.ResponseMessage.DeserializeTo<ReportCountersResponse, FailRegisterResponse>();
        }

        // Url: /integration/v1/reports/Deposits
        public async Task<Response<ReportDepositResponse, FailRegisterResponse>> GetDepositsAsync(ReportRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var httpResponse = await _baseUrl
                .AppendPathSegments("integration", "v1", "reports", "Deposits")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                .AllowHttpStatus("401")
                .PostJsonAsync(request);
            //return await result.ResponseMessage.DeserializeTo<ReportDepositResponse, FailRegisterResponse>();
            string resultData = string.Empty;
            try
            {
                resultData = await httpResponse.ResponseMessage.Content.ReadAsStringAsync();
                if (httpResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    var items = JsonConvert.DeserializeObject<IReadOnlyCollection<ReportDepositModel>>(resultData);
                    return Response<ReportDepositResponse, FailRegisterResponse>.CreateSuccess(
                        new ReportDepositResponse()
                        {
                            Items = items
                        });
                }
                else
                if (httpResponse.ResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception(resultData);
                }
                else
                {
                    var response = JsonConvert.DeserializeObject<FailRegisterResponse>(resultData);
                    return Response<ReportDepositResponse, FailRegisterResponse>.CreateFailed(response);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "DeserializeTo failed. Response : {resultData}", resultData);
                throw;
            }
        }

        // Url: /integration/v1/reports/Registrations
        public async Task<Response<ReportRegistrationResponse, FailRegisterResponse>> GetRegistrationsAsync(ReportRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var httpResponse = await _baseUrl
                .AppendPathSegments("integration", "v1", "reports", "Registrations")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                .AllowHttpStatus("401")
                .PostJsonAsync(request);

            //return await result.ResponseMessage.DeserializeListTo<IReadOnlyList<ReportRegistrationModel>, FailRegisterResponse>();
            string resultData = string.Empty;
            try
            {
                resultData = await httpResponse.ResponseMessage.Content.ReadAsStringAsync();
                if (httpResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    var items = JsonConvert.DeserializeObject<IReadOnlyCollection<ReportRegistrationModel>>(resultData);
                    return Response<ReportRegistrationResponse, FailRegisterResponse>.CreateSuccess(
                        new ReportRegistrationResponse()
                        {
                            Items = items
                        });
                }
                else
                if (httpResponse.ResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception(resultData);
                }
                else
                {
                    var response = JsonConvert.DeserializeObject<FailRegisterResponse>(resultData);
                    return Response<ReportRegistrationResponse, FailRegisterResponse>.CreateFailed(response);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "DeserializeTo failed. Response : {resultData}", resultData);
                throw;
            }
        }

    }
}