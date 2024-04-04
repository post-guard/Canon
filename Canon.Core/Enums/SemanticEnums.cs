namespace Canon.Core.Enums;

public enum SemanticTokenType
{
    Keyword,
    Number,
    Operator,
    Delimiter,
    Identifier,
    Character,
    Empty,
    Error,  // 加了一个错误token
    /// <summary>
    /// 语法分析中的栈底符号
    /// </summary>
    End
}

public enum DelimiterType
{
    Comma,
    Period,
    Colon,
    Semicolon,
    LeftParenthesis,
    RightParenthesis,
    LeftSquareBracket,
    RightSquareBracket,
    SingleQuotation,
    DoubleQuotation,
    /// <summary>
    /// 访问记录字段用的点 x.a
    /// </summary>
    Dot,
    /// <summary>
    /// 数组声明上下界之间的分隔符 1..50
    /// </summary>
    DoubleDots
}

public enum KeywordType
{
    Program,
    Const,
    Var,
    Procedure,
    Function,
    Begin,
    End,
    Array,
    Of,
    If,
    Then,
    Else,
    For,
    To,
    Do,
    Integer,
    Real,
    Boolean,
    Character,
    Divide,
    Not,
    Mod,
    And,
    Or
}

public enum OperatorType
{
    Equal,
    NotEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    Plus,
    Minus,
    Multiply,
    Divide,
    Assign
}

public enum NumberType
{
    Integer,
    Real,
    Hex
}

public enum StateType
{
    Word,
    Digit,
    Delimiter,
    Operator
}

public enum BasicIdType
{
    Int,
    Real,
    Char,
    Bool
}
