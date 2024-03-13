namespace Canon.Core.Enums;

public enum SemanticTokenType
{
    Keyword,
    Number,
    Operator,
    Delimiter,
    Identifier,
    Character,
    /// <summary>
    /// 语法分析中的栈底符号
    /// </summary>
    End,
    /// <summary>
    /// 语法分析中的空串符号
    /// </summary>
    Empty
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
    DoubleQuotation
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
    Mod,
    And,
    Or,
    Assign
}

public enum NumberType
{
    Integer,
    Real,
    Hex
}
