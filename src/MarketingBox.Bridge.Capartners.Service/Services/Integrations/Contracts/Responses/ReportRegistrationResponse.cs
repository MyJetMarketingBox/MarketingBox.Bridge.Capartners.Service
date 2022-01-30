using System;
using System.Collections.Generic;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public class ReportRegistrationResponse
    {
        public IReadOnlyCollection<ReportRegistrationModel> Items;
    }

    public class ReportRegistrationModel
    {
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public string CrmStatus { get; set; }

        public string UrlQuery { get; set; }
    }

}