using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Excursion360.Desktop
{
    static class Extensions
    {
        public static ILogger CreateLogger(this IWebHost host, string categoryName)
        {
            return host.Services.GetService<ILoggerFactory>().CreateLogger(categoryName);
        }
        public static UriBuilder WithPath(this UriBuilder uriBuilder, string path)
        {
            uriBuilder.Path = path;
            return uriBuilder;
        }
    }
}
