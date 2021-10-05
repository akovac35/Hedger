using System;
using System.Collections.Generic;

namespace Hedger.Core.Model
{
    public class OrderBook
    {
        public List<Bid> Bids { get; set; } = new();
        public List<Ask> Asks { get; set; } = new();
        public DateTime AcqTime { get; set; }

        public String CryptoExchangeId { get; set; } = Guid.NewGuid().ToString();

        public String Id { get; set; } = Guid.NewGuid().ToString();
    }
}
