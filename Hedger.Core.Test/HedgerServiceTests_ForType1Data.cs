using Hedger.Core.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedger.Core.Tests
{
    [TestFixture]
    public class HedgerServiceTests_ForType1Data
    {
        private HedgerService HedgerServiceInstance { get; set; } = new();

        private List<OrderBook> ExchangeOrderBooks { get; set; } = new();
        private List<CryptoExchangeBalance> ExchangeBalances { get; set; } = new();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ExchangeOrderBooks = OrderBookHelper.ReadOrderBooksFromJson(TestsHelper.GetType1Data());
        }

        [SetUp]
        public void Setup()
        {
            ExchangeBalances.Clear();
            ExchangeBalances.Add(new() { EurBalance = 10000d, BtcBalance = 1.5d, Id = "PipiExchange" });
            ExchangeBalances.Add(new() { EurBalance = 10000d, BtcBalance = 1.5d, Id = "MelkijadExchange" });
        }

        [Test]
        public void PrepareOrderPlan_Buy_AllWithLimiter()
        {
            var order = new Order() { Type = OrderType.Buy, Amount = 10d };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            // Only 3 bitcoins are available in total for purchase - on two exchanges. Together with our balance we get 6 bitcoins
            Assert.AreEqual(6d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
            // We expect 20000 - 2*(1*2964.3 + 0.5*2964.29) = 20000 - 8892.89 = 11107.11
            Assert.AreEqual(11107.109999999999d, orderPlan.UpdatedBalances.Sum(item => item.EurBalance), "Invalid balance.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.CryptoExchangeId).Distinct().Count() == 2, "Exactly two exchanges should be used.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.Order.Type == OrderType.Sell).Distinct().Count() == 1, "Plan contains an invalid order type.");
        }

        [Test]
        public void PrepareOrderPlan_Buy_Zero()
        {
            var order = new Order() { Type = OrderType.Buy, Amount = 0d };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            Assert.AreEqual(3d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
            Assert.AreEqual(20000d, orderPlan.UpdatedBalances.Sum(item => item.EurBalance), "Invalid balance.");
        }

        [Test]
        public void PrepareOrderPlan_Buy_Negative()
        {
            var order = new Order() { Type = OrderType.Buy, Amount = -1d };

            Assert.Throws<ArgumentException>(() => HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances));
        }

        [Test]
        public void PrepareOrderPlan_Buy_VerifyPrioritizationBasedOnPrice()
        {
            var order = new Order() { Type = OrderType.Buy, Amount = 1d };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            // We buy one bitcoin
            Assert.AreEqual(4d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
            // We expect to pay 2964.29, that is 20000 - 2964.29 = 17035.71
            Assert.AreEqual(17035.709999999999d, orderPlan.UpdatedBalances.Sum(item => item.EurBalance), "Invalid balance.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.CryptoExchangeId).Distinct().Count() == 2, "Exactly two exchanges should be used.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.Order.Type == OrderType.Sell).Distinct().Count() == 1, "Plan contains an invalid order type.");
        }

        [Test]
        public void PrepareOrderPlan_Buy_WithNegativeBalance()
        {
            var order = new Order() { Type = OrderType.Buy, Amount = 10 };

            foreach (var item in ExchangeBalances)
            {
                item.EurBalance = -10000;
            }

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            // We should not buy anything
            Assert.AreEqual(3d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
        }

        [Test]
        public void PrepareOrderPlan_Sell_AllWithLimiter()
        {
            var order = new Order() { Type = OrderType.Sell, Amount = 10d };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            // We have sold all bitcoin
            Assert.AreEqual(0d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
            // Only 3 bitcoins are available in total for sale - as balances on two exchanges. That is 2*(0.5*2960.64 + 1*2960.63) + 20000 = 28881.9 
            Assert.AreEqual(20000d + 8881.900000000001d, orderPlan.UpdatedBalances.Sum(item => item.EurBalance), "Invalid balance.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.CryptoExchangeId).Distinct().Count() == 2, "Exactly two exchanges should be used.");
            Assert.IsTrue(orderPlan.OrderPlanItems.Select(item => item.Order.Type == OrderType.Buy).Distinct().Count() == 1, "Plan contains an invalid order type.");
        }

        [Test]
        public void PrepareOrderPlan_Sell_Zero()
        {
            var order = new Order() { Type = OrderType.Sell, Amount = 0d };

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            Assert.AreEqual(3d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
            Assert.AreEqual(20000d, orderPlan.UpdatedBalances.Sum(item => item.EurBalance), "Invalid balance.");
        }

        [Test]
        public void PrepareOrderPlan_Sell_Negative()
        {
            var order = new Order() { Type = OrderType.Sell, Amount = -1d };

            Assert.Throws<ArgumentException>(() => HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances));
        }

        [Test]
        public void PrepareOrderPlan_Sell_WithNegativeBalance()
        {
            var order = new Order() { Type = OrderType.Sell, Amount = 10d };

            foreach (var item in ExchangeBalances)
            {
                item.BtcBalance = -1.5;
            }

            var orderPlan = HedgerServiceInstance.PrepareOrderPlan(order, ExchangeOrderBooks, ExchangeBalances);

            // We should not sold anything
            Assert.AreEqual(-3d, orderPlan.UpdatedBalances.Sum(item => item.BtcBalance), "Invalid balance.");
        }
    }
}