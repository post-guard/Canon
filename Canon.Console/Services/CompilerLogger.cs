using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Canon.Console.Services;

/// <summary>
/// 编译器日志配置类
/// </summary>
public sealed class CompilerLoggerConfiguration
{
    /// <summary>
    /// 配置各个等级日志的输出颜色
    /// </summary>
    public Dictionary<LogLevel, ConsoleColor> LogLevelColorMap { get; } = new()
    {
        { LogLevel.Critical, ConsoleColor.Red },
        { LogLevel.Error, ConsoleColor.Red },
        { LogLevel.Warning, ConsoleColor.Yellow },
        { LogLevel.Information, ConsoleColor.Green },
        { LogLevel.Debug, ConsoleColor.Gray }
    };
}

/// <summary>
/// 空日志记录器
/// 不输出任何内容
/// </summary>
public sealed class EmptyLogger : ILogger
{
    public bool IsEnabled(LogLevel _) => false;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
    }
}

/// <summary>
/// 编译器日志记录器
/// </summary>
/// <param name="getCurrentConfiguration">获得编译器日志记录器配置对象</param>
public sealed class CompilerLogger(Func<CompilerLoggerConfiguration> getCurrentConfiguration) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
        return getCurrentConfiguration().LogLevelColorMap.ContainsKey(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        CompilerLoggerConfiguration configuration = getCurrentConfiguration();
        ConsoleColor originalColor = System.Console.ForegroundColor;

        System.Console.ForegroundColor = configuration.LogLevelColorMap[logLevel];
        System.Console.Write($"{FormatLogLevel(logLevel)}: ");

        System.Console.ForegroundColor = originalColor;
        System.Console.WriteLine(formatter(state, exception));
    }

    private static string FormatLogLevel(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Debug:
                return "dbg";
            case LogLevel.Information:
                return "info";
            case LogLevel.Warning:
                return "warn";
            case LogLevel.Error:
                return "error";
            case LogLevel.Critical:
                return "critical";
            default:
                return "log";
        }
    }
}

[UnsupportedOSPlatform("browser")]
public sealed class CompilerLoggerProvider(IOptions<CompilerLoggerConfiguration> options) : ILoggerProvider
{
    private readonly ILogger _logger = new CompilerLogger(() => options.Value);
    private readonly ILogger _emptyLogger = new EmptyLogger();

    public ILogger CreateLogger(string categoryName)
    {
        // 只显示编译器中的日志信息
        if (categoryName.StartsWith("Canon"))
        {
            return _logger;
        }
        else
        {
            return _emptyLogger;
        }
    }

    public void Dispose()
    {
    }
}
