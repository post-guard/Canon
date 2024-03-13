namespace Canon.Core.Exceptions;
/// <summary>
/// 语法分析中引发的异常
/// </summary>
public class GrammarException : Exception
{
    public GrammarException()  { }

    public GrammarException(string message) : base(message) { }

    public GrammarException(string message, Exception innerException) : base(message, innerException) { }
}
