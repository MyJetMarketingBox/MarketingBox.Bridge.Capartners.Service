using System;
using System.Collections.Generic;

namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Responses
{
    public class ReportDepositResponse
    {
        public IReadOnlyCollection<ReportDepositModel> Items;
    }

    public class ReportDepositModel
    {
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public bool StatusComplete { get; set; } = true;

        public string UrlQuery { get; set; }
    }

}