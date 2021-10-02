using com.github.akovac35.Logging;
using com.github.akovac35.Logging.Serilog;
using Hedger.Core;
using Hedger.Core.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using static com.github.akovac35.Logging.LoggerHelper<Hedger.Api.Program>;

namespace Hedger.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerilogHelper.CreateLogger(configure => configure.AddJsonFile("serilog.json", optional: false, reloadOnChange: true));
            LoggerFactoryProvider.LoggerFactory = SerilogHelper.CreateLoggerFactory();

            Here(l => l.Entering(args));

            try
            {
                var host = CreateHostBuilder(args).Build();

                var orderBooksJsonFilePath = Path.Combine(AppContext.BaseDirectory, ApiConstants.OrderBooksJsonFile);
                Here(l => l.LogInformation($"Populating cache from {orderBooksJsonFilePath}."));
                var cache = host.Services.GetRequiredService<IMemoryCache>();
                cache.Set<List<OrderBook>>(ApiConstants.ExchangeOrderBooksCacheKey, OrderBookHelper.ReadOrderBooks(orderBooksJsonFilePath));

                host.Run();
            }
            catch (Exception ex)
            {
                Here(l => l.LogError(ex, ex.Message));
                throw;
            }
            finally
            {
                SerilogHelper.CloseAndFlushLogger();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging(logging =>
                    {
                        // Needed to remove duplicate log entries
                        logging.ClearProviders();
                    }).UseSerilog();
                });
    }
}
