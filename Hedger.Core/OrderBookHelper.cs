using Hedger.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Hedger.Core
{
    public static class OrderBookHelper
    {
        public static List<OrderBook> ReadOrderBooksFromJson(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));
            }

            try
            {
                var orderBooks = JsonSerializer.Deserialize<List<OrderBook>>(File.ReadAllText(filePath)) ?? throw new InvalidOperationException($"File is null or empty.");
                return orderBooks;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Json file is not a valid {nameof(OrderBook)} list: {filePath}", ex);
            }
        }
    }
}
