using NUnit.Framework;
using System.IO;

namespace Hedger.Core.Tests
{
    public static class TestsHelper
    {
        public static string GetSmallOrderBooksDataPath()
        {
            var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles/small_order_books_data.json");
            return filePath;
        }
    }
}
