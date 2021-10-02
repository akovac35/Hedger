using Hedger.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Hedger.Core
{
    public static class OrderBookHelper
    {
        public static List<OrderBook> ReadOrderBooks(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));
            }

            var orderBooks = new List<OrderBook>();

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var json = line.Split(new[] { '\t' }, 2)[1];
                    var orderBook = JsonSerializer.Deserialize<OrderBook>(json);
                    orderBooks.Add(orderBook!);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to parse string as {nameof(OrderBook)}: {line}", ex);
                }
            }

            return orderBooks;
        }
    }
}
