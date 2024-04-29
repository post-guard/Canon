using Canon.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Canon.Tests.Utils;

public class TestLogger(ITestOutputHelper testOutputHelper) : ICompilerLogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        testOutputHelper.WriteLine($"{logLevel}: {formatter(state, exception)}");
    }

    public string Build() => string.Empty;
}
