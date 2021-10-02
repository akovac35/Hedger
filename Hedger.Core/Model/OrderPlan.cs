using System;
using System.Collections.Generic;

namespace Hedger.Core.Model
{
    /// <summary>
    /// An order plan containing a list of order items and expected balances should the plan be processed.
    /// </summary>
    public class OrderPlan
    {
        public List<OrderPlanItem> OrderPlanItems { get; init; } = new();
        public List<CryptoExchangeBalance> UpdatedBalances { get; init; } = new();

        public String Id { get; init; } = Guid.NewGuid().ToString();
    }
}
