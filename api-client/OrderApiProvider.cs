using System;
using Refit;

namespace api_client
{
    public static class OrderApiProvider
    {
        public static IOrderApi GetApi(string baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));
            return RestService.For<IOrderApi>(baseAddress);
        }
    }
}
