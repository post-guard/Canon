namespace Canon.Core.LexicalParser;

public static class LexRules
{
    // 保留关键字
    private static readonly string[] _keywords =
    [
        "Program", "Const", "Var", "Procedure",
        "Function", "Begin", "End", "Array",
        "Of", "If", "Then", "Else",
        "For", "To", "Do", "Integer",
        "Real", "Boolean", "Character", "Divide",
        "Not", "Mod", "And", "Or"
    ];

    private static readonly string[] _delimiter = [";", ",", ":", ".", "(", ")", "[", "]", "'", "\"", ".."];

    private static readonly string[] _operator = ["=", "<>", "<", "<=", ">", ">=", "+", "-", "*", "/", ":="];

    // 判断字符
    public static bool IsDigit(char _ch) {
        if (_ch >= '0' && _ch <= '9') return true;
        return false;
    }

    public static bool IsHexDigit(char _ch)
    {
        if ((_ch >= '0' && _ch <= '9') || (_ch<= 'F' && _ch >= 'A')) return true;
        return false;
    }

    public static bool IsLetter(char _ch) {
        if ((_ch >= 'A' && _ch <= 'Z') || (_ch >= 'a' && _ch <= 'z' || _ch == '_')) {
            return true;
        }
        return false;
    }

    public static bool IsKeyword(string tokenString)
    {

        foreach (var t in _keywords)
        {
            if (string.Equals(tokenString, t, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }


    public static bool IsDelimiter(char ch)
    {
        foreach (var delimiter in _delimiter)
        {
            if (delimiter.Contains(ch))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsOperator(char ch)
    {
        foreach (var o in _operator)
        {
            if (o.Contains(ch))
            {
                return true;
            }
        }
        return false;
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
