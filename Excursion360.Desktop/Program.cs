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
using System.Net.NetworkInformation;
using Microsoft.Extensions.FileProviders;
using Excursion360.Desktop.Services;

namespace Excursion360.Desktop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = CreateHost(args);

            bool useFirefox = IsNeedFireFox(args);
            IBrowser browser;
            if (useFirefox)
            {
                var firefoxInterop = host.Services.GetRequiredService<IFirefoxInterop>();
                browser = firefoxInterop;
                if (!await firefoxInterop.IsFirefoxInstalled())
                {
                    if (!await firefoxInterop.TryInstallFirefoxAsync().ConfigureAwait(false))
                    {
                        return;
                    }
                }
            }
            else
            {
                browser = new DefaultBrowser();
            }
            var hostTask = host.RunAsync().ConfigureAwait(false);
            Console.WriteLine();
            var uri = new Uri(host.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()
                .Addresses
                .Single(a => a.StartsWith("http:", StringComparison.Ordinal)));

            await browser.StartBrowser(uri);

            await hostTask;
        }

        private static bool IsNeedFireFox(string[] args)
        {
            if (args.Contains("--firefox"))
            {
                return true;
            }
            while (true)
            {
                Console.WriteLine("Select run option");
                Console.WriteLine("1: Use firefox (install if not present)");
                Console.WriteLine("2: Use default browser");
                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '1':
                        return true;
                    case '2':
                        return false;
                    default:
                        break;
                }
            }
        }

        private static IHost CreateHost(string[] args)
        {
            var host = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging(logs =>
                {
                    logs.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var fso = new FileServerOptions
                    {
                        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                    };
                    fso.StaticFileOptions.OnPrepareResponse = (context) =>
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    };
                    webBuilder.Configure(app => app.UseFileServer(fso));
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
            return host;
        }
    }
}
