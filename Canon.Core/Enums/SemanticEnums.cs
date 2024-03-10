namespace Canon.Core.Enums;

public enum SemanticTokenType
{
    Keyword,
    Number,
    Operator,
    Delimiter,
    Identifier,
    Character,
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
