using System;
using System.Text.Json.Serialization;

namespace Hedger.Core.Model
{
    public class Order
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderType Type { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }

        public String Id { get; set; } = Guid.NewGuid().ToString();
    }
}
