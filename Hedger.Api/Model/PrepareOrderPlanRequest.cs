using Hedger.Core.Model;
using System.Collections.Generic;

namespace Hedger.Api.Model
{
    public class PrepareOrderPlanRequest
    {
        public Order? Order { get; set; }
        public List<CryptoExchangeBalance>? ExchangeBalances { get; set; }
    }
}
