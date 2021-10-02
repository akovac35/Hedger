using System;

namespace Hedger.Core.Model
{
    public class CryptoExchangeBalance
    {
        public double InBtc { get; set; }
        public double InEur { get; set; }

        public String Id { get; init; } = Guid.NewGuid().ToString();
    }
}
