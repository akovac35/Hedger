using FastDeepCloner;
using Hedger.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using com.github.akovac35.Logging;

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

            if (exchangeOrderBooks is null)
            {
                throw new System.ArgumentNullException(nameof(exchangeOrderBooks));
            }

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
                        remainingAssetAmountToBuy -= buyAmount;
                        orderPlan.OrderPlanItems.Add(new() { OrderInstance = asks[i].Ask.Order, CryptoExchangeId = asks[i].CryptoExchangeId });
                    }

                    if (exchange?.InEur <= 0 || remainingAssetAmountToBuy <= 0) break;
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
                        remainingAssetAmountToSell -= sellAmount;
                        orderPlan.OrderPlanItems.Add(new() { OrderInstance = bids[i].Bid.Order, CryptoExchangeId = bids[i].CryptoExchangeId });
                    }

                    if (exchange?.InBtc <= 0 || remainingAssetAmountToSell <= 0) break;
                }
            }

            orderPlan.UpdatedBalances.AddRange(exchangeBalancesClone);

            _logger.Here(l => l.Exiting(orderPlan));
            return orderPlan;
        }

        public double TryBuyAmount(ref CryptoExchangeBalance? exchange, double remainingAssetAmountToBuy, double availableAssetAmount, double assetPrice)
        {
            var remainingEur = (exchange?.InEur ?? -1);
            var assetBuyAmount = remainingEur / assetPrice;

            if (assetBuyAmount <= 0)
            {
                return 0;
            }
            else if (assetBuyAmount > remainingAssetAmountToBuy)
            {
                exchange!.InEur -= remainingAssetAmountToBuy * assetPrice;
                exchange!.InBtc += remainingAssetAmountToBuy;
                return remainingAssetAmountToBuy;
            }
            else if (assetBuyAmount > availableAssetAmount)
            {
                exchange!.InEur -= availableAssetAmount * assetPrice;
                exchange!.InBtc += availableAssetAmount;
                return availableAssetAmount;
            }
            else
            {
                exchange!.InEur = 0;
                exchange!.InBtc += assetBuyAmount;
                return assetBuyAmount;
            }
        }

        public double TrySellAmount(ref CryptoExchangeBalance? exchange, double remainingAssetAmountToSell, double availableAssetAmount, double assetPrice)
        {
            var remainingBtc = (exchange?.InBtc ?? -1);

            if (remainingBtc <= 0)
            {
                return 0;
            }
            else if (remainingBtc > remainingAssetAmountToSell)
            {
                exchange!.InEur += remainingAssetAmountToSell * assetPrice;
                exchange!.InBtc -= remainingAssetAmountToSell;
                return remainingAssetAmountToSell;
            }
            else if (remainingBtc > availableAssetAmount)
            {
                exchange!.InEur += availableAssetAmount * assetPrice;
                exchange!.InBtc -= availableAssetAmount;
                return availableAssetAmount;
            }
            else
            {
                exchange!.InEur += remainingBtc * assetPrice;
                exchange!.InBtc = 0;
                return remainingBtc;
            }
        }
    }
}
