namespace Canon.Core.Exceptions;
using Enums;
/// <summary>
/// 词法分析中引发的异常
/// </summary>
public class LexemeException : Exception
{
    public LexemeErrorType ErrorType { get; }
    public uint Line { get; }
    public uint CharPosition { get; }
    public LexemeException()  { }

    public LexemeException(string message) : base(message) { }

    public LexemeException(string message, Exception innerException) :
        base(message, innerException) { }

    /// <param name="errorType">错误类型</param>
    /// <param name="line">单词的行号</param>
    /// <param name="charPosition">单词的列号</param>
    /// <param name="message">错误信息</param>
    public LexemeException(LexemeErrorType errorType, uint line, uint charPosition, string message) :
        base("line:" + line + ", charPosition:" + charPosition + " :" + message)
    {
        ErrorType = errorType;
        Line = line;
        CharPosition = charPosition;
    }

    public override string ToString()
    {
        return $"LexemeException: ErrorType={ErrorType}, Line={Line}, CharPosition={CharPosition}, Message={Message}\n";
    }
}
