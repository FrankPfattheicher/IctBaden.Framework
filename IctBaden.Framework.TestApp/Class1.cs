using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IctBaden.Framework.TestApp;

public class Class1
{
    public ILoggerFactory MyLoggerFactory = NullLoggerFactory.Instance;
}
