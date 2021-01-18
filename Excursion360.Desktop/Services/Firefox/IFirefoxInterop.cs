using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Services.Firefox
{
    public interface IFirefoxInterop : IBrowser
    {
        Task<bool> TryInstallFirefoxAsync();
        ValueTask<bool> IsFirefoxInstalled();
    }
}
