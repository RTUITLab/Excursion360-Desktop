using System;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Services
{
    public interface IBrowser
    {
        ValueTask StartBrowser(Uri uri);
    }
}