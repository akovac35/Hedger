using System;

namespace Hedger.Core.Model
{
    public class OrderPlanItem
    {
        public Order Order { get; set; } = new();

        public String CryptoExchangeId { get; set; } = Guid.NewGuid().ToString();

        public String Id { get; set; } = Guid.NewGuid().ToString();
    }
}
