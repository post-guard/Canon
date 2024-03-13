namespace Canon.Core.Exceptions;
/// <summary>
/// 语义分析中引发的异常
/// </summary>
public class SemanticException : Exception
{
    public SemanticException() : base() { }

    public SemanticException(string message) : base(message) { }

    public SemanticException(string message, Exception innerException) : base(message, innerException) { }
}
