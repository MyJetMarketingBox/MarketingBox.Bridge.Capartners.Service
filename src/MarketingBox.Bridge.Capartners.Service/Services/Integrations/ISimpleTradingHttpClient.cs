using System.Threading.Tasks;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Requests;
using MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations
{
    public interface ISimpleTradingHttpClient
    {
        /// <summary>
        /// A purchase deduct amount immediately. This transaction type is intended when the goods or services
        /// can be immediately provided to the customer. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<RegistrationResponse, FailRegisterResponse>> RegisterTraderAsync(
            RegistrationRequest request);

        /// <summary>
        /// It allows to get previous transaction basic information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<ReportCountersResponse, FailRegisterResponse>> GetCountsAsync(
            ReportCountersRequest request);
        /// <summary>
        /// It allows to get registration reports
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<ReportRegistrationResponse, FailRegisterResponse>> GetRegistrationsAsync(
            ReportRequest request);
        /// <summary>
        /// It allows to get previous transaction basic information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Response<ReportDepositResponse, FailRegisterResponse>> GetDepositsAsync(
            ReportRequest request);
    }
}