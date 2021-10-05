using com.github.akovac35.Logging;
using FastDeepCloner;
using Hedger.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Hedger.Core
{
    public class HedgerService
    {
        private readonly ILogger _logger = NullLogger.Instance;

        public HedgerService(ILogger<HedgerService>? logger = null)
        {
            if (logger != null) _logger = logger;
        }

        /// <summary>
        /// Prepares an order plan based on the provided order and order books from various cryptocurrency exchanges and their balances.
        /// </summary>
        public OrderPlan PrepareOrderPlan(Order order, List<OrderBook> exchangeOrderBooks, List<CryptoExchangeBalance> exchangeBalances)
        {
            _logger.Here(l => l.Entering(order, exchangeBalances));

            if (order is null)
            {
                throw new System.ArgumentNullException(nameof(order));
            }

            if (order.Amount < 0)
            {
                throw new System.ArgumentException("Invalid amount.", nameof(order));
            }

            if (exchangeOrderBooks is null)
            {
                throw new System.ArgumentNullException(nameof(exchangeOrderBooks));
            }

            // TODO Exclude invalid exchange order books, e.g. negative amounts

            if (exchangeBalances is null)
            {
                throw new System.ArgumentNullException(nameof(exchangeBalances));
            }

            var orderPlan = new OrderPlan();
            var exchangeBalancesClone = exchangeBalances.Clone();

            if (order.Type == OrderType.Buy)
            {
                var asks = exchangeOrderBooks
                    .SelectMany(item => item.Asks, (item, ask) => new { CryptoExchangeId = item.CryptoExchangeId, Ask = ask })
                    .OrderBy(item => item.Ask.Order.Price)
                    .ToList();

                var remainingAssetAmountToBuy = order.Amount;

                for (int i = 0; i < asks.Count; i++)
                {
                    var exchange = exchangeBalancesClone.Where(item => item.Id == asks[i].CryptoExchangeId).FirstOrDefault();

                    var buyAmount = TryBuyAmount(ref exchange, remainingAssetAmountToBuy, asks[i].Ask.Order.Amount, asks[i].Ask.Order.Price);

                    if (buyAmount > 0)
                    {
                        var clonedOrder = asks[i].Ask.Order.Clone();
                        clonedOrder.Amount = buyAmount;

                        remainingAssetAmountToBuy -= buyAmount;
                        orderPlan.OrderPlanItems.Add(new() { Order = clonedOrder, CryptoExchangeId = asks[i].CryptoExchangeId });
                    }

                    if (remainingAssetAmountToBuy <= 0) break;
                    // TODO Consider add a limiter on the number of scanned items based on some amount difference percentage threshold and scan duration or similar
                }
            }
            else if (order.Type == OrderType.Sell)
            {
                var bids = exchangeOrderBooks
                    .SelectMany(item => item.Bids, (item, bid) => new { CryptoExchangeId = item.CryptoExchangeId, Bid = bid })
                    .OrderByDescending(item => item.Bid.Order.Price)
                    .ToList();

                var remainingAssetAmountToSell = order.Amount;

                for (int i = 0; i < bids.Count; i++)
                {
                    var exchange = exchangeBalancesClone.Where(item => item.Id == bids[i].CryptoExchangeId).FirstOrDefault();

                    var sellAmount = TrySellAmount(ref exchange, remainingAssetAmountToSell, bids[i].Bid.Order.Amount, bids[i].Bid.Order.Price);

                    if (sellAmount > 0)
                    {
                        var clonedOrder = bids[i].Bid.Order.Clone();
                        clonedOrder.Amount = sellAmount;

                        remainingAssetAmountToSell -= sellAmount;
                        orderPlan.OrderPlanItems.Add(new() { Order = clonedOrder, CryptoExchangeId = bids[i].CryptoExchangeId });
                    }

                    if (remainingAssetAmountToSell <= 0) break;
                    // TODO Consider add a limiter on the number of scanned items based on some amount difference percentage threshold and scan duration or similar
                }
            }

            orderPlan.UpdatedBalances.AddRange(exchangeBalancesClone);

            _logger.Here(l => l.Exiting(orderPlan));
            return orderPlan;
        }

        public double TryBuyAmount(ref CryptoExchangeBalance? exchange, double remainingAssetAmountToBuy, double availableAssetAmount, double assetPrice)
        {
            var remainingEur = (exchange?.EurBalance ?? -1);
            var assetBuyAmount = remainingEur / assetPrice;

            if (assetBuyAmount <= 0)
            {
                return 0;
            }
            else if (assetBuyAmount > remainingAssetAmountToBuy || assetBuyAmount > availableAssetAmount)
            {
                if (remainingAssetAmountToBuy > availableAssetAmount)
                {
                    exchange!.EurBalance -= availableAssetAmount * assetPrice;
                    exchange!.BtcBalance += availableAssetAmount;
                    return availableAssetAmount;
                }
                else
                {
                    exchange!.EurBalance -= remainingAssetAmountToBuy * assetPrice;
                    exchange!.BtcBalance += remainingAssetAmountToBuy;
                    return remainingAssetAmountToBuy;
                }
            }
            else
            {
                exchange!.EurBalance = 0;
                exchange!.BtcBalance += assetBuyAmount;
                return assetBuyAmount;
            }
        }

        public double TrySellAmount(ref CryptoExchangeBalance? exchange, double remainingAssetAmountToSell, double availableAssetAmount, double assetPrice)
        {
            var remainingBtc = (exchange?.BtcBalance ?? -1);

            if (remainingBtc <= 0)
            {
                return 0;
            }
            else if (remainingBtc > remainingAssetAmountToSell || remainingBtc > availableAssetAmount)
            {
                if (remainingAssetAmountToSell > availableAssetAmount)
                {
                    exchange!.EurBalance += availableAssetAmount * assetPrice;
                    exchange!.BtcBalance -= availableAssetAmount;
                    return availableAssetAmount;
                }
                else
                {
                    exchange!.EurBalance += remainingAssetAmountToSell * assetPrice;
                    exchange!.BtcBalance -= remainingAssetAmountToSell;
                    return remainingAssetAmountToSell;
                }
            }
            else
            {
                exchange!.EurBalance += remainingBtc * assetPrice;
                exchange!.BtcBalance = 0;
                return remainingBtc;
            }
        }
    }
}
