using NUnit.Framework;
using System.IO;

namespace Hedger.Core.Tests
{
    public static class TestsHelper
    {
        public static string GetType1Data()
        {
            var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles/type1_data.json");
            return filePath;

        }
    }
}
