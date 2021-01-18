using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Services.Firefox
{
    class UnsupportedOsFirefoxInterop : IFirefoxInterop
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
}
