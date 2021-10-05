using NUnit.Framework;
using System.Linq;

namespace Hedger.Core.Tests
{
    [TestFixture]
    public class OrderBookHelperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ReadOrderBooks_Works()
        {
            var orderBooks = OrderBookHelper.ReadOrderBooksFromJson(TestsHelper.GetType1Data());

            Assert.AreNotEqual(0, orderBooks.Count());
        }
    }
}
