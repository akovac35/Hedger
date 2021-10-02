using Hedger.Core.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Hedger.Core.Tests
{
    [TestFixture]
    public class HedgerServiceTests
    {
        private HedgerService HedgerServiceInstance { get; set; } = new();

        private List<OrderBook> ExchangeOrderBooks { get; set; } = new();
        private List<CryptoExchangeBalance> ExchangeBalances { get; set; } = new();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ExchangeOrderBooks = OrderBookHelper.ReadOrderBooks(TestsHelper.GetSmallOrderBooksDataPath());
        }

        [SetUp]
        public void Setup()
        {
            ExchangeBalances.Clear();
            ExchangeBalances.Add(new() { InEur = 20000, InBtc = 2 });
            ExchangeBalances.Add(new() { InEur = 10000, InBtc = 1 });

            for (int i = 0; i < ExchangeOrderBooks.Count; i++)
            {
                if (i % 2 == 0)
                {
                    ExchangeOrderBooks[i].CryptoExchangeId = ExchangeBalances[0].Id;
                }
                else
                {
                    ExchangeOrderBooks[i].CryptoExchangeId = ExchangeBalances[1].Id;
                }
            }
        }

        [Test]
        public void PrepareOrderPlan_Buy()
        {
            var order = new Order() { Type = OrderType.Buy, Amount = 10 };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            Assert.AreEqual(10.00312823856998d, orderPlan.UpdatedBalances.Sum(item => item.InBtc), "Invalid balance.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.CryptoExchangeId).Distinct().Count() > 1, "More than one exchange should be used.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.OrderInstance.Type == OrderType.Sell).Distinct().Count() == 1, "Plan contains an invalid order type.");
        }

        [Test]
        public void PrepareOrderPlan_Sell()
        {
            var order = new Order() { Type = OrderType.Sell, Amount = 1 };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            Assert.AreEqual(32960.690000000002d, orderPlan.UpdatedBalances.Sum(item => item.InEur), "Invalid balance.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.CryptoExchangeId).Distinct().Count() > 1, "More than one exchange should be used.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.OrderInstance.Type == OrderType.Buy).Distinct().Count() == 1, "Plan contains an invalid order type.");
        }
    }
}