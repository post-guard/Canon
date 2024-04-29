using System.Text;
using Canon.Core.Abstractions;

namespace Canon.Server.Models;

public class CompilerLogger : ICompilerLogger
{
    private readonly ThreadLocal<StringBuilder> _builder = new(() => new StringBuilder());

    public string Build()
    {
        if (_builder.Value is not null)
        {
            string result = _builder.Value.ToString();
            _builder.Value.Clear();
            return result;
        }

        throw new InvalidOperationException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (_builder.Value is not null)
        {
            _builder.Value.Append(logLevel).Append(": ").Append(formatter(state, exception)).Append('\n');
        }
    }
}
