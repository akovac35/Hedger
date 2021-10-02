using System;
using System.Collections.Generic;

namespace Hedger.Core.Model
{
    public class OrderBook
    {
        public List<Bid> Bids { get; init; } = new();
        public List<Ask> Asks { get; init; } = new();
        public DateTime AcqTime { get; init; }

        public String CryptoExchangeId { get; set; } = Guid.NewGuid().ToString();

        public String Id { get; init; } = Guid.NewGuid().ToString();
    }
}
