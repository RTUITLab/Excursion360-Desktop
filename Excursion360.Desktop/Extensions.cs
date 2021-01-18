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

namespace Excursion360.Desktop
{
    static class Extensions
    {
        public static ILogger CreateLogger(this IWebHost host, string categoryName)
        {
            return host.Services.GetService<ILoggerFactory>().CreateLogger(categoryName);
        }
        public static Uri GetListeningUri(this IHost host)
        {
            return new Uri(host.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()
                .Addresses
                .Single(a => a.StartsWith("http:", StringComparison.Ordinal)));
        }
    }
}
