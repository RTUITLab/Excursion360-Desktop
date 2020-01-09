using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Services.Firefox
{
    public class WindowsFirefoxInterop : IFirefoxInterop
    {
        private const string FirefosinstallerUri = "https://download-installer.cdn.mozilla.net/pub/firefox/releases/71.0/win64/ru/Firefox%20Setup%2071.0.msi";
        private const string RegistryFirefoxKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe";
        private readonly ILogger<WindowsFirefoxInterop> logger;
        private readonly HttpClient httpClient;

        public WindowsFirefoxInterop(
            ILogger<WindowsFirefoxInterop> logger,
            IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            httpClient = httpClientFactory.CreateClient(nameof(IFirefoxInterop));
        }
        public async Task<bool> TryInstallFirefoxAsync()
        {
            logger.LogInformation("Installing firefox...");

            string installerFilePath = Path.Combine(Directory.GetCurrentDirectory() + @"\firefox.msi");

            Uri downloadUri = new Uri(FirefosinstallerUri);
            using (HttpResponseMessage response = httpClient.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogInformation($"The request returned with HTTP status code {response.StatusCode}");
                    return false;
                }

                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var fileStream = File.OpenWrite(installerFilePath);
                var totalRead = 0L;
                var totalReads = 0L;
                var buffer = ArrayPool<byte>.Shared.Rent(8192);
                var isMoreToRead = true;

                do
                {
                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await fileStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);

                        totalRead += read;
                        totalReads += 1;
                    }
                }
                while (isMoreToRead);

                ArrayPool<byte>.Shared.Return(buffer);
            }

            using Process installerProcess = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = "/c msiexec /qn /l* " + Directory.GetCurrentDirectory() + @"\firefox.txt /norestart /i " + installerFilePath,
                    Verb = "runas",
                    UseShellExecute = false
                }
            };
            installerProcess.Start();

            while (installerProcess.HasExited == false)
            {
                logger.LogInformation("Installing...");
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }

            logger.LogInformation("Installed!");

            File.Delete(installerFilePath);

            return await IsFirefoxInstalled();
        }

        public ValueTask<bool> IsFirefoxInstalled()
        {
            logger.LogInformation("Checking for firefox...");

            object path = Registry.GetValue(RegistryFirefoxKey, "", null);

            if (path != null) { logger.LogInformation("Firefox installed"); }
            return new ValueTask<bool>(path != null);
        }

        public ValueTask StartFirefox(Uri uri)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));
            logger.LogInformation($"Starting firefox on {uri.AbsoluteUri}");

            Process.Start(Registry.GetValue(RegistryFirefoxKey, "", null).ToString(), uri.AbsoluteUri);
            return new ValueTask();
        }
    }
}
