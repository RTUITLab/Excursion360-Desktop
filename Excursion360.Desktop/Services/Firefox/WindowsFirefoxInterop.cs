using Microsoft.Win32;
using System.Buffers;
using System.Diagnostics;

namespace Excursion360.Desktop.Services.Firefox
{
    public class WindowsFirefoxInterop(HttpClient httpClient, ILogger<WindowsFirefoxInterop> logger) : IFirefoxInterop
    {
        private const string FirefosinstallerUri = "https://download-installer.cdn.mozilla.net/pub/firefox/releases/71.0/win64/ru/Firefox%20Setup%2071.0.msi";
        private const string RegistryFirefoxKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe";

        public async Task<bool> TryInstallFirefoxAsync()
        {
            logger.LogInformation("Installing firefox...");

            string installerFilePath = Path.Combine(Directory.GetCurrentDirectory() + @"\firefox.msi");

            Uri downloadUri = new Uri(FirefosinstallerUri);
            using (var response = await httpClient.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead))
            {
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogInformation("The request returned with HTTP status code {ResponseStatusCode}", response.StatusCode);
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
                    var read = await contentStream.ReadAsync(buffer).ConfigureAwait(false);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);

                        totalRead += read;
                        totalReads += 1;
                    }
                }
                while (isMoreToRead);

                ArrayPool<byte>.Shared.Return(buffer);
            }

            using var installerProcess = new Process
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
            File.Delete(Directory.GetCurrentDirectory() + @"\firefox.txt");

            return await IsFirefoxInstalled();
        }

        public ValueTask<bool> IsFirefoxInstalled()
        {
            logger.LogInformation("Checking for firefox...");

            object? path = Registry.GetValue(RegistryFirefoxKey, "", null);

            if (path != null) { logger.LogInformation("Firefox installed"); }
            return new ValueTask<bool>(path != null);
        }

        public ValueTask StartBrowser(Uri uri)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));
            logger.LogInformation("Starting firefox on {StartUrl}", uri.AbsoluteUri);
            var firefoxPath = Registry.GetValue(RegistryFirefoxKey, "", null)?.ToString()
                ?? throw new InvalidDataException($"Can't run firefox, not found registry key value {RegistryFirefoxKey}");
            Process.Start(firefoxPath, uri.AbsoluteUri);
            return new ValueTask();
        }
    }
}
