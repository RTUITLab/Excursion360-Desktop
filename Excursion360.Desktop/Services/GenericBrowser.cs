using MintPlayer.PlatformBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Services
{
    public class GenericBrowser : IBrowser
    {
        private readonly Browser browser;

        public GenericBrowser(Browser browser)
        {
            this.browser = browser;
        }
        public ValueTask StartBrowser(Uri uri)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Process.Start(browser.ExecutablePath, uri.AbsoluteUri);
            return new ValueTask();
        }
    }
}
