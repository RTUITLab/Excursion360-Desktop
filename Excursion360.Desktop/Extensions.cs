using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Excursion360.Desktop;

static class Extensions
{
    public static ILogger CreateLogger(this IWebHost host, string categoryName) 
        => host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(categoryName);
    public static Uri GetListeningUri(this IHost host)
    {
        return new Uri(host.Services
            .GetRequiredService<IServer>()
            .Features
            .GetRequiredFeature<IServerAddressesFeature>()
            .Addresses
            .Single(a => a.StartsWith("http:", StringComparison.Ordinal)));
    }

    public static string ExcursionDirectoryPath(this IConfiguration configuration)
        => configuration.GetValue<string?>("excursionsPath", null) ?? Directory.GetCurrentDirectory();
}
