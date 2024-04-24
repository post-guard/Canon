using Canon.Core.Enums;

namespace Canon.Core.LexicalParser;

public static class LexRules
{
    // 保留关键字
    private static readonly Dictionary<string, KeywordType> s_keywordTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "program", KeywordType.Program },
            { "const", KeywordType.Const },
            { "var", KeywordType.Var },
            { "procedure", KeywordType.Procedure },
            { "function", KeywordType.Function },
            { "begin", KeywordType.Begin },
            { "end", KeywordType.End },
            { "array", KeywordType.Array },
            { "of", KeywordType.Of },
            { "if", KeywordType.If },
            { "then", KeywordType.Then },
            { "else", KeywordType.Else },
            { "for", KeywordType.For },
            { "to", KeywordType.To },
            { "do", KeywordType.Do },
            { "integer", KeywordType.Integer },
            { "real", KeywordType.Real },
            { "boolean", KeywordType.Boolean },
            { "char", KeywordType.Character },
            { "div", KeywordType.Divide }, // 注意: Pascal 使用 'div' 而不是 '/'
            { "not", KeywordType.Not },
            { "mod", KeywordType.Mod },
            { "and", KeywordType.And },
            { "or", KeywordType.Or }
        };

    public static bool GetKeywordTypeByKeywprd(string keyword, out KeywordType type)
        => s_keywordTypes.TryGetValue(keyword, out type);


    private static readonly HashSet<char> s_delimiter = [';', ',', ':', '.', '(', ')', '[', ']', '\'', '"'];

    private static readonly HashSet<string> s_operator = ["=", "<>", "<", "<=", ">", ">=", "+", "-", "*", "/", ":="];

    // 判断字符
    public static bool IsDigit(char ch)
    {
        if (ch is >= '0' and <= '9') return true;
        return false;
    }

    public static bool IsHexDigit(char ch)
    {
        if (ch is >= '0' and <= '9' || ch is <= 'F' and >= 'A') return true;
        return false;
    }

    public static bool IsLetter(char ch)
    {
        if (ch is >= 'A' and <= 'Z' || (ch is >= 'a' and <= 'z' || ch == '_'))
        {
            return true;
        }

        return false;
    }

    public static bool IsDelimiter(char ch)
        => s_delimiter.Contains(ch);

    public static bool IsOperator(char ch)
    {
        return s_operator.Any(op => op.Contains(ch));
    }

    public static bool IsBreakPoint(char ch)
    {
        if (ch == ' ' || ch == '\n' || ch == '\t' || ch == '\r')
        {
            return true;
        }

        return false;
    }
}
