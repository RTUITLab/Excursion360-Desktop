namespace Excursion360.Desktop.Services.Firefox;

public interface IFirefoxInterop : IBrowser
{
    Task<bool> TryInstallFirefoxAsync();
    ValueTask<bool> IsFirefoxInstalled();
}
