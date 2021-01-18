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
using System.Reflection;
using Excursion360.Desktop.Exceptions;

namespace Excursion360.Desktop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            try
            {
                var targetDirectory = GetExcursionDirectory();
                using IHost host = CreateHost(args, targetDirectory);
                IBrowser browser = await SelectBrowser(args, targetDirectory, host).ConfigureAwait(false);
                var hostTask = host.RunAsync().ConfigureAwait(false);
                await browser.StartBrowser(host.GetListeningUri());
                await hostTask;
            }
            catch (IncorrectEnvironmentException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error, please tell us about that https://github.com/RTUITLab/Excursion360-Desktop/issues");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task<IBrowser> SelectBrowser(string[] args, string targetDirectory, IHost host)
        {
            bool useFirefox = IsNeedFireFox(args, targetDirectory);
            IBrowser browser;
            if (useFirefox)
            {
                browser = await SetupFirefox(host).ConfigureAwait(false);
            }
            else
            {
                browser = new DefaultBrowser();
            }

            return browser;
        }

        private static string GetExcursionDirectory()
        {
            var dirs = Directory.GetDirectories(Directory.GetCurrentDirectory()).Select(d => Path.GetFileName(d)).ToArray();
            if (dirs.Length == 0)
            {
                throw new IncorrectEnvironmentException("You must place excursion files to the subdirectory with executable file");
            }
            if (dirs.Length == 1)
            {
                return dirs[0];
            }
            return dirs[ConsoleHelper.SelectOneFromArray("Select directory with excursion", dirs)];
        }

        private static async Task<IBrowser> SetupFirefox(IHost host)
        {
            var firefoxInterop = host.Services.GetRequiredService<IFirefoxInterop>();
            if (!await firefoxInterop.IsFirefoxInstalled())
            {
                if (!await firefoxInterop.TryInstallFirefoxAsync().ConfigureAwait(false))
                {
                    throw new IncorrectEnvironmentException("Can't install firefox");
                }
            }
            return firefoxInterop;
        }

        private static bool IsNeedFireFox(string[] args, string targetDirectory)
        {
            if (args.Contains("--firefox"))
            {
                return true;
            }
            var browserMode = ConsoleHelper.SelectOneFromArray($"Select run option. Selected excursion: {targetDirectory}",
                new string[]
                {
                    "Use firefox (install if not present)",
                    "Use default browser"
                });
            Console.Clear();
            return browserMode switch
            {
                0 => true,
                1 => false,
                _ => false
            };
        }

        private static IHost CreateHost(string[] args, string targetDirectory)
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
                        FileProvider = new CompositeFileProvider(
                            new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), targetDirectory)),
                            new EmbeddedFileProvider(Assembly.GetExecutingAssembly()))
                    };
                    fso.DefaultFilesOptions.DefaultFileNames.Add("Resources/NotFound.html");
                    fso.StaticFileOptions.OnPrepareResponse = (context) =>
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    };
                    webBuilder.Configure(app =>
                    {
                        app.UseFileServer(fso);
                    });
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
