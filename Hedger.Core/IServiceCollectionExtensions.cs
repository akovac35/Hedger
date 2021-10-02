using Hedger.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddHedgerCore(this IServiceCollection collection)
        {
            collection.TryAddScoped<HedgerService>();
            return collection;
        }
    }
}