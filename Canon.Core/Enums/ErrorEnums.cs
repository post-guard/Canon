namespace Canon.Core.Enums;

public enum LexemeErrorType
{
    IllegalNumberFormat,//数字格式不正确
    UnknownCharacterOrString,//源代码包含无法识别的字符或字符串
    UnclosedStringLiteral,//字符串字面量未闭合
    UnclosedComment,//注释未闭合
    InvalidEscapeSequence,//无效的转义字符
    IllegalOperator,//非法的操作符
}
