namespace Canon.Core.Exceptions;

/// <summary>
/// 编译器中的统一异常基类
/// </summary>
public class CanonException : Exception
{
    public CanonException()
    {
    }

    public CanonException(string message) : base(message)
    {
    }

    public CanonException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
