using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Canon.Tests.Utils;

public class TestLogger<T>(ITestOutputHelper testOutputHelper) : ILogger<T>, IDisposable
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        testOutputHelper.WriteLine("{0}: {1}", logLevel, formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Dispose()
    {
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }
}
