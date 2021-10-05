using System;

namespace Hedger.Core.Model
{
    public class CryptoExchangeBalance
    {
        public double BtcBalance { get; set; }
        public double EurBalance { get; set; }

        public String Id { get; set; } = Guid.NewGuid().ToString();
    }
}
