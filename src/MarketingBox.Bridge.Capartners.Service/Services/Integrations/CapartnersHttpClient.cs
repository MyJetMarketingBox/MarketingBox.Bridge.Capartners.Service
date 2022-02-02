using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses;
using Newtonsoft.Json;
using Serilog;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations
{
    public class CapartnersHttpClient : ICapartnersHttpClient
    {
        private readonly string _baseUrl;
        private readonly string _login;
        private readonly string _password;

        public CapartnersHttpClient(string baseUrl, string login, string password)
        {
            _baseUrl = baseUrl;
            _login = login;
            _password = password;
        }

        // POST Url: /clients
        public async Task<Response<RegistrationResponse, FailRegisterResponse>>
            RegisterTraderAsync(RegistrationRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var requestUrl = _baseUrl
                .AppendPathSegments("clients")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("login", _login)
                .WithHeader("password", _password)
                .AllowHttpStatus("400")
                .AllowHttpStatus("403");

            var result = await requestUrl
                .PostJsonAsync(request);
            return await result.ResponseMessage.DeserializeTo<RegistrationResponse, FailRegisterResponse>();
        }

        // GET Url: /clients
        public async Task<Response<ReportDepositResponse, FailRegisterResponse>> GetDepositsAsync(ReportRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var httpResponse = await _baseUrl
                .AppendPathSegments("clients")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("login", _login)
                .WithHeader("password", _password)
                .AllowHttpStatus("400")
                .AllowHttpStatus("403")
                .SendJsonAsync(HttpMethod.Get, request);
            //return await result.ResponseMessage.DeserializeTo<ReportDepositResponse, FailRegisterResponse>();
            string resultData = string.Empty;
            try
            {
                resultData = await httpResponse.ResponseMessage.Content.ReadAsStringAsync();
                if (httpResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    var full = JsonConvert.DeserializeObject<ReportDepositResponse>(resultData);
                    var items = full?.Items;
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

        // GET Url: /clients
        public async Task<Response<ReportRegistrationResponse, FailRegisterResponse>> GetRegistrationsAsync(ReportRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var httpResponse = await _baseUrl
                .AppendPathSegments("clients")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("login", _login)
                .WithHeader("password", _password)
                .AllowHttpStatus("400")
                .AllowHttpStatus("403")
                .SendJsonAsync(HttpMethod.Get, request);

            //return await result.ResponseMessage.DeserializeListTo<IReadOnlyList<ReportRegistrationModel>, FailRegisterResponse>();
            string resultData = string.Empty;
            try
            {
                resultData = await httpResponse.ResponseMessage.Content.ReadAsStringAsync();
                if (httpResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    var full = JsonConvert.DeserializeObject<ReportRegistrationResponse>(resultData);
                    var items = full?.Items;
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