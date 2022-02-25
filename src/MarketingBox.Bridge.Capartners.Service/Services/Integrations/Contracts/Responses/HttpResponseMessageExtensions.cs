using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<Response<TSuccessResponse, TFailedResponse>> DeserializeTo<TSuccessResponse,
            TFailedResponse>(this HttpResponseMessage httpResponseMessage)
            where TSuccessResponse : class
            where TFailedResponse : class
        {
            string resultData = await httpResponseMessage.Content.ReadAsStringAsync();
            Log.Logger.Information("Capartners brand return response : {@RawResult}", resultData);
            try
            {
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var response = JsonConvert.DeserializeObject<TSuccessResponse>(resultData);
                    return Response<TSuccessResponse, TFailedResponse>.CreateSuccess(response);
                }
                else
                if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception(resultData);
                }
                else
                {
                    if (typeof(TFailedResponse) == typeof(string))
                        return Response<TSuccessResponse, TFailedResponse>.CreateFailed(resultData as TFailedResponse);

                    var response = JsonConvert.DeserializeObject<TFailedResponse>(resultData);
                    return Response<TSuccessResponse, TFailedResponse>.CreateFailed(response);
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