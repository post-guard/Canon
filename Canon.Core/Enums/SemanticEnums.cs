namespace Canon.Core.Enums;

public enum SemanticTokenType
{
    Keyword,
    Number,
    Operator,
    Delimiter,
    Identifier,
    Character,
    String,
    Empty,
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
    Or,
    True,
    False,
    While
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
    Start,
    Comment,
    Word,
    Num,
    Delimiter,
    Operator,
    BreakPoint,
    Unknown,
    Done
}
