namespace Excursion360.Desktop.Services.Firefox;

public class UnsupportedOsFirefoxInterop : IFirefoxInterop
{
    public ValueTask<bool> IsFirefoxInstalled()
    {
        throw new NotSupportedException();
    }

    public ValueTask StartBrowser(Uri uri)
    {
        throw new NotSupportedException();
    }

    public Task<bool> TryInstallFirefoxAsync()
    {
        throw new NotSupportedException();
    }
}
