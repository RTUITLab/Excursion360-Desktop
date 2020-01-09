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

namespace Excursion360.Desktop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = WebHost
                .CreateDefaultBuilder()
                .UseWebRoot("")
                .Configure(app => app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = (context) =>
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                }))
                .ConfigureLogging(logs =>
                {
                    logs.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            var firefoxInstallLogger = host.CreateLogger("FireFox.Installer");

            if (!IsFirefoxInstalled(firefoxInstallLogger))
            {
                if (! await TryInstallFirefoxAsync(firefoxInstallLogger))
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

            object path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "", null);

            if (path != null) { logger.LogInformation("Firefox installed"); }
            return path != null;
        }

        static async Task<bool> TryInstallFirefoxAsync(ILogger logger)
        {
            logger.LogInformation("Installing firefox...");

            string installerFilePath = Path.Combine(Directory.GetCurrentDirectory() + @"\firefox.msi");

            HttpClient client = new HttpClient();
            Uri downloadUri = new Uri("https://download-installer.cdn.mozilla.net/pub/firefox/releases/71.0/win64/ru/Firefox%20Setup%2071.0.msi");
            using (HttpResponseMessage response = client.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead).Result )
            {
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogInformation($"The request returned with HTTP status code {response.StatusCode}");
                    return false;
                }

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(installerFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            totalReads += 1;
                        }
                    }
                    while (isMoreToRead);
                }
            }

            Process installerProcess = new Process();
            installerProcess.StartInfo = new ProcessStartInfo("cmd.exe")
            {
                Arguments = "/c msiexec /qn /l* " + Directory.GetCurrentDirectory() + @"\firefox.txt /norestart /i " + installerFilePath,
                Verb = "runas",
                UseShellExecute = false
            };
            installerProcess.Start();

            int i = 0;

            while (installerProcess.HasExited == false)
            {
                logger.LogInformation("Installing...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            logger.LogInformation("Installed!");

            File.Delete(installerFilePath);

            return IsFirefoxInstalled(logger);
        }

        private static void StartFirefox(ILogger logger, string url)
        {
            logger.LogInformation($"Starting firefox on {url}");

            Process.Start(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "", null).ToString(), url + "/index.html");
        }
    }
}
