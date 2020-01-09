using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;
using System.Runtime.InteropServices;
using Excursion360.Desktop.Services.Firefox;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;

namespace Excursion360.Desktop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging(logs =>
                {
                    logs.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseWebRoot("");
                    webBuilder.Configure(app => app.UseStaticFiles(new StaticFileOptions
                    {
                        OnPrepareResponse = (context) =>
                        {
                            context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                            context.Context.Response.Headers.Add("Expires", "-1");
                        }
                    }));
                })
                .ConfigureServices(services =>
                {
                    services.AddHttpClient(nameof(IFirefoxInterop));
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        services.AddSingleton<IFirefoxInterop, WindowsFirefoxInterop>();
                    }// TODO Add other OS
                    else
                    {
                        services.AddSingleton<IFirefoxInterop, UnsupportedOsFirefoxInterop>();
                    }
                })
                .Build();

            var firefoxInterop = host.Services.GetRequiredService<IFirefoxInterop>();

            if (!await firefoxInterop.IsFirefoxInstalled())
            {
                if (!await firefoxInterop.TryInstallFirefoxAsync().ConfigureAwait(false))
                {
                    return;
                }
            }
            var hostTask = host.RunAsync().ConfigureAwait(false);
            Console.WriteLine();
            var uri = new Uri(host.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()
                .Addresses
                .Single(a => a.StartsWith("http:", StringComparison.Ordinal)));

            await firefoxInterop.StartFirefox(new UriBuilder(uri).WithPath("index.html").Uri);

            await hostTask;
        }
    }
}
