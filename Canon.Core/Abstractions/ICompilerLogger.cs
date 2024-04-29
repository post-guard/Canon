using Microsoft.Extensions.Logging;

namespace Canon.Core.Abstractions;

public interface ICompilerLogger : ILogger
{
    IDisposable ILogger.BeginScope<TState>(TState state) => default!;

    bool ILogger.IsEnabled(LogLevel logLevel) => true;

    public string Build();
}
