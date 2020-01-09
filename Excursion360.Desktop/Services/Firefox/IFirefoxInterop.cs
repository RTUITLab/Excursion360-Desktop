using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Services.Firefox
{
    public interface IFirefoxInterop
    {
        Task<bool> TryInstallFirefoxAsync();
        ValueTask<bool> IsFirefoxInstalled();
        ValueTask StartFirefox(Uri uri);
    }
}
