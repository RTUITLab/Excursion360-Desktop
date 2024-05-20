using System.Runtime.InteropServices;
using Excursion360.Desktop.Services.Firefox;
using Microsoft.Extensions.FileProviders;
using Excursion360.Desktop.Services;
using System.Reflection;
using Excursion360.Desktop.Exceptions;
using MintPlayer.PlatformBrowser;
using Excursion360.Desktop;
using Microsoft.AspNetCore.StaticFiles;

Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.White;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddSingleton<IStateImagesMetricsStore, InMemoryIStateImagesMetricsStore>();
builder.Services.AddResponseCompression();
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddHttpClient<IFirefoxInterop, WindowsFirefoxInterop>();
}
else
{
    builder.Services.AddHttpClient<IFirefoxInterop, UnsupportedOsFirefoxInterop>();
}

var app = builder.Build();
var targetDirectory = GetExcursionDirectory(builder.Configuration.ExcursionDirectoryPath());

app.MapGet("/eapi/preload.json", (IStateImagesMetricsStore stateImagesMetrics) => {
    return new
    {
        images = stateImagesMetrics.MostPopularUrls(),
    };
});

var fso = new FileServerOptions
{
    FileProvider = new CompositeFileProvider(
            new PhysicalFileProvider(Path.Combine(builder.Configuration.ExcursionDirectoryPath(), targetDirectory)),
            new EmbeddedFileProvider(Assembly.GetExecutingAssembly()))
};
fso.DefaultFilesOptions.DefaultFileNames.Add("Resources/NotFound.html");
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";
fso.StaticFileOptions.ContentTypeProvider = provider;
fso.StaticFileOptions.OnPrepareResponse = (context) =>
{
    if (context.File.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && context.File.PhysicalPath is not null)
    {
        var stateImagesMetricsStore = context.Context.RequestServices.GetRequiredService<IStateImagesMetricsStore>();
        stateImagesMetricsStore.IncrementImageHit(context.Context.Request.Path);
    }
};
app.UseResponseCompression();
app.UseFileServer(fso);


IBrowser browser = await SelectBrowser(targetDirectory, app.Services, app.Configuration).ConfigureAwait(false);

var hostTask = app.RunAsync();
await browser.StartBrowser(app.GetListeningUri());
await hostTask;


async Task<IBrowser> SelectBrowser(string targetDirectory, IServiceProvider serviceProvider, IConfiguration configuration)
{
    if (configuration.GetValue<bool>("--firefox"))
    {
        return await SetupFirefox(serviceProvider).ConfigureAwait(false);
    }
    var selectedBrowser = await SelectInstalledBrowserAsync(targetDirectory);
    return new GenericBrowser(selectedBrowser);
}

string GetExcursionDirectory(string rootDir)
{
    var dirs = Directory.GetDirectories(rootDir).Select(d => Path.GetFileName(d)).ToArray();
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

async Task<IBrowser> SetupFirefox(IServiceProvider serviceProvider)
{
    var firefoxInterop = serviceProvider.GetRequiredService<IFirefoxInterop>();
    if (!await firefoxInterop.IsFirefoxInstalled())
    {
        if (!await firefoxInterop.TryInstallFirefoxAsync().ConfigureAwait(false))
        {
            throw new IncorrectEnvironmentException("Can't install firefox");
        }
    }
    return firefoxInterop;
}


async Task<Browser> SelectInstalledBrowserAsync(string targetDirectory)
{
    var browsers = (await PlatformBrowser
        .GetInstalledBrowsers())
        .Where(b => !b.Name.Contains("Explorer"))
        .DistinctBy(b => b.Name)
        .ToArray();

    var browsersList = new List<string> { "Use Firefox (install if not present)" };
    browsersList.AddRange(browsers.Select(b => b.Name));

    var browserMode = ConsoleHelper.SelectOneFromArray($"Select run option. Selected excursion: {targetDirectory}", [.. browsersList]);
    Console.Clear();
    return browsers[browserMode - 1];
}
