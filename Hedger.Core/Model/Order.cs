using System;
using System.Text.Json.Serialization;

namespace Hedger.Core.Model
{
    public class Order
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderType Type { get; init; }
        public double Amount { get; init; }
        public double Price { get; init; }

        public String Id { get; init; } = Guid.NewGuid().ToString();
    }
}
