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

namespace Excursion360.Desktop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = WebHost
                .CreateDefaultBuilder()
                .UseWebRoot("")
                .Configure(app => app.UseStaticFiles())
                .ConfigureLogging(logs =>
                {
                    logs.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var firefoxInstallLogger = host.CreateLogger("FireFox.Installer");

            if (!IsFirefoxInstalled(firefoxInstallLogger))
            {
                if (!TryInstallFirefox(firefoxInstallLogger))
                {
                    return;
                }
            }
            var hostTask = host.RunAsync().ConfigureAwait(false);

            var url = host.ServerFeatures
                .Get<IServerAddressesFeature>()
                .Addresses
                .Single(a => a.StartsWith("http:", StringComparison.Ordinal));

            StartFirefox(host.CreateLogger("FireFox.Start"), url);

            await hostTask;
        }


        static bool IsFirefoxInstalled(ILogger logger)
        {
            logger.LogInformation("Checking for firefox...");
            // TODO
            return true;
        }

        static bool TryInstallFirefox(ILogger logger)
        {
            logger.LogInformation("Installing firefox...");
            // TODO
            return true;
        }

        private static void StartFirefox(ILogger logger, string url)
        {
            logger.LogInformation($"Starting firefox on {url}");
        }
    }
}
