using MintPlayer.PlatformBrowser;
using System.Diagnostics;

namespace Excursion360.Desktop.Services;

public class GenericBrowser(Browser browser) : IBrowser
{
    public ValueTask StartBrowser(Uri uri)
    {
        uri = uri ?? throw new ArgumentNullException(nameof(uri));
        Process.Start(browser.ExecutablePath, uri.AbsoluteUri);
        return new ValueTask();
    }
}
