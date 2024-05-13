namespace Canon.Core.Exceptions;
using Enums;
/// <summary>
/// 词法分析中引发的异常
/// </summary>
public class LexemeException : CanonException
{
    public LexemeErrorType ErrorType { get; }

    public uint Line { get; }

    public uint CharPosition { get; }

    private readonly string _message;

    /// <param name="errorType">错误类型</param>
    /// <param name="line">单词的行号</param>
    /// <param name="charPosition">单词的列号</param>
    /// <param name="message">错误信息</param>
    public LexemeException(LexemeErrorType errorType, uint line, uint charPosition, string message)
    {
        ErrorType = errorType;
        Line = line;
        CharPosition = charPosition;
        _message = message;
    }

    public override string Message => ToString();

    public override string ToString()
    {
        return $"LexemeException: ErrorType={ErrorType}, Line={Line}, CharPosition={CharPosition}, Message={_message}\n";
    }
}
