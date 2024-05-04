using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;
using Microsoft.AspNetCore.Http.Features;

namespace Excursion360.Desktop
{
    static class Extensions
    {
        public static ILogger CreateLogger(this IWebHost host, string categoryName) 
            => host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(categoryName);
        public static Uri GetListeningUri(this IHost host)
        {
            return new Uri(host.Services
                .GetRequiredService<IServer>()
                .Features
                .GetRequiredFeature<IServerAddressesFeature>()
                .Addresses
                .Single(a => a.StartsWith("http:", StringComparison.Ordinal)));
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
