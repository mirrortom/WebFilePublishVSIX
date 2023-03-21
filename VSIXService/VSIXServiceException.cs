using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXService;

internal class VSIXServiceException : Exception
{
    public VSIXServiceException() { }
    public VSIXServiceException(string message) : base(message) { }
    public VSIXServiceException(string message, Exception inner) : base(message, inner) { }

}
