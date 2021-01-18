using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excursion360.Desktop.Exceptions
{
    public class IncorrectEnvironmentException : Exception
    {
        public IncorrectEnvironmentException() : base("Incorrect environment for excurion viewer")
        {
        }

        public IncorrectEnvironmentException(string message) : base(message)
        {
        }

        public IncorrectEnvironmentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
