using System.Collections.Generic;
using System.Linq;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class
            => enumerable.Where(e => e != null).Select(e => e!);

        public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string?> enumerable)
            => enumerable.Where(e => !string.IsNullOrEmpty(e)).Select(e => e!);
    }
}