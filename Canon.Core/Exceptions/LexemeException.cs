namespace Canon.Core.Exceptions;
/// <summary>
/// 词法分析中引发的异常
/// </summary>
public class LexemeException : Exception
{
    public LexemeException()  { }

    public LexemeException(string message) : base(message) { }

    public LexemeException(string message, Exception innerException) :
        base(message, innerException) { }

    /// <param name="line">单词的行号</param>
    /// <param name="charPosition">单词的列号</param>
    /// <param name="message">错误信息</param>
    public LexemeException(uint line, uint charPosition, string message) :
        base("line:" + line + ", charPosition:" + charPosition + " :" + message) { }

    public LexemeException(uint line, uint charPosition, Exception innerException) :
        base("line:" + line + ", charPosition:" + charPosition + " : ", innerException) { }

    public LexemeException(uint line, uint charPosition, string message, Exception innerException) :
        base("line:" + line + ", charPosition:" + charPosition + " :" + message, innerException) { }
}
