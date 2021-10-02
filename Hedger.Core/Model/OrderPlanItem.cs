using System;

namespace Hedger.Core.Model
{
    public class OrderPlanItem
    {
        public Order OrderInstance { get; init; } = new();

        public String CryptoExchangeId { get; set; } = Guid.NewGuid().ToString();

        public String Id { get; init; } = Guid.NewGuid().ToString();
    }
}
