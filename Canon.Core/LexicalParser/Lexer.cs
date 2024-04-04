using System.Numerics;
using System.Text;
using Canon.Core.Enums;
using Canon.Core.Exceptions;

namespace Canon.Core.LexicalParser;

public class Lexer(string source)
{

    // 保留关键字
    private readonly string[] _keywords =
    [
        "Program", "Const", "Var", "Procedure",
        "Function", "Begin", "End", "Array",
        "Of", "If", "Then", "Else",
        "For", "To", "Do", "Integer",
        "Real", "Boolean", "Character", "Divide",
        "Not", "Mod", "And", "Or"
    ];

    private readonly string[] _delimiter = [";", ",", ":", ".", "(", ")", "[", "]", "'", "\"", ".."];

    private readonly string[] _operator = ["=", "<>", "<", "<=", ">", ">=", "+", "-", "*", "/", ":="];

    // 状态机
    private StateType _state;
    private char _ch;

    private LinkedList<char> _token = new LinkedList<char>();

    // bool save;
    // int saved_state;
    bool _finish;


    //缓冲区
    private readonly char[] _buffer = new char[2048];

    // int start_pos;
    private int _fwdPos;

    // 计数器
    private uint _line = 1;
    private uint _chPos;

    private readonly Dictionary<SemanticTokenType, int> _tokenCount = new Dictionary<SemanticTokenType, int>
    {
        { SemanticTokenType.Keyword, 0 },
        { SemanticTokenType.Number, 0 },
        { SemanticTokenType.Operator, 0 },
        { SemanticTokenType.Delimiter, 0 },
        { SemanticTokenType.Identifier, 0 },
        { SemanticTokenType.Character, 0 },
        { SemanticTokenType.Error, 0 },
        { SemanticTokenType.End, 0 }
    };

    private readonly List<SemanticToken> _tokens = [];

    public List<SemanticToken> Tokenize()
    {
        // 缓冲区
        // start_pos = 0;
        _fwdPos = 0;

        // 状态机
        _finish = false;

        while (!_finish)
        {
            GetChar();
            GetNbc();
            if (_finish) break;

            _token = new LinkedList<char>();

            if (IsLetter())
            {
                _state = StateType.Word;
            }
            else if(_ch == '.')
            {
                char next = PeekNextChar();
                if (next >= '0' && next <= '9')
                {
                    _state = StateType.Digit;
                }
                else
                {
                    _state = StateType.Delimiter;
                }
            }
            else if (IsDigit() || _ch == '$')
            {
                _state = StateType.Digit;
            }
            else if (IsDelimiter())
            {
                _state = StateType.Delimiter;
            }
            else if (_ch == '{')
            {
                while (_ch != '}')
                {
                    GetChar();
                    if (_ch == '\n')
                    {
                        _line++;
                        _chPos = 0;
                    }
                    if (_finish)
                    {
                        throw new LexemeException(LexemeErrorType.UnclosedComment, _line, _chPos, "The comment is not closed.");
                    }

                }

                continue;
            }
            else
            {
                _state = StateType.Operator;
            }

            switch (_state)
            {
                case StateType.Word:
                    while (IsDigit() || IsLetter())
                    {
                        Cat();
                        GetChar();
                    }

                    Retract();

                    if (IsKeyword())
                    {
                        KeywordType keywordType =
                            KeywordSemanticToken.GetKeywordTypeByKeyword(LinkedListToString(_token.First));
                        MakeToken(keywordType);
                    }
                    else
                    {
                        MakeToken(SemanticTokenType.Identifier);
                    }

                    break;
                case StateType.Digit:
                    DealNumber();
                    break;
                case StateType.Delimiter:
                    Cat();
                    switch (_ch)
                    {
                        case '.':
                            {
                                GetChar();
                                if (_ch == '.')
                                {
                                    Cat();
                                    MakeToken(DelimiterType.DoubleDots);
                                    break;
                                }

                                Retract();
                                if (IsDot())
                                {
                                    MakeToken(DelimiterType.Dot);
                                }
                                else
                                {
                                    MakeToken(DelimiterType.Period);
                                }
                            }
                            break;
                        case '\'':
                        case '\"':
                            {
                                // 重置_token，准备收集字符串内容
                                _token = new LinkedList<char>();

                                GetChar(); // 移动到下一个字符，即字符串的第一个字符
                                while (_ch != '\'' && _ch != '\"')
                                {
                                    Cat(); // 收集字符
                                    GetChar(); // 移动到下一个字符
                                    if (_ch == '\n' || _finish)
                                    {
                                        throw new LexemeException(LexemeErrorType.UnclosedStringLiteral, _line, _chPos, "The String is not closed.");
                                    }
                                }

                                MakeToken(SemanticTokenType.Character); // 或其它适用于字符串字面量的SemanticTokenType
                                _token = new LinkedList<char>(); // 重置_token

                                if (!(_ch == '\'' || _ch == '\"'))
                                {
                                    throw new LexemeException(LexemeErrorType.UnclosedStringLiteral, _line, _chPos, "The String is not closed.");
                                }
                            }
                            break;
                        case ',':
                            MakeToken(DelimiterType.Comma);
                            break;
                        case ':':
                            char nextChar = PeekNextChar();
                            if (nextChar == '=')
                            {
                                GetChar();
                                Cat();
                                MakeToken(OperatorType.Assign);
                            }
                            else
                            {
                                MakeToken(DelimiterType.Colon);
                            }

                            break;
                        case ';':
                            MakeToken(DelimiterType.Semicolon);
                            break;
                        case '(':
                            char next = PeekNextChar();
                            if (next == '*')
                            {
                                GetChar();
                                bool commentClosed = false;
                                while (!commentClosed)
                                {
                                    GetNbc();
                                    GetChar();
                                    while (_ch != '*')
                                    {
                                        GetNbc();
                                        GetChar();
                                        if (_finish)
                                        {
                                            throw new LexemeException(LexemeErrorType.UnclosedComment, _line, _chPos, "The comment is not closed.");
                                        }
                                    }

                                    GetChar();
                                    if (_finish)
                                    {
                                        throw new LexemeException(LexemeErrorType.UnclosedComment, _line, _chPos, "The comment is not closed.");
                                    }

                                    if (_ch == ')') commentClosed = true;
                                }
                            }
                            else
                            {
                                MakeToken(DelimiterType.LeftParenthesis);
                            }

                            break;
                        case ')':
                            MakeToken(DelimiterType.RightParenthesis);
                            break;
                        case '[':
                            MakeToken(DelimiterType.LeftSquareBracket);
                            break;
                        case ']':
                            MakeToken(DelimiterType.RightSquareBracket);
                            break;
                    }

                    break;
                case StateType.Operator:
                    DealOther();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        return _tokens;
    }

    private void DealNumber()
    {
        // 十六进制
        if (_ch == '$')
        {
            Cat();

            GetChar();
            while (!NumberShouldBreak())
            {
                // 假设IsHexDigit方法能够识别十六进制数字
                if (IsHexDigit())
                {
                    Cat();
                    GetChar();
                }
                else if(NumberShouldBreak())
                {
                    break;
                }
                else
                {
                    throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal hex numbers!");
                }
            }
            MakeToken(NumberType.Hex);
            return;
        }

        // 非十六进制
        if(IsDigit() || _ch == '.')
        {
            while (!NumberShouldBreak())
            {
                // 含小数部分
                if (_ch == '.')
                {
                    // 检查是否是符号 “..”
                    char next = PeekNextChar();
                    if (next == '.')
                    {
                        Retract();
                        break;
                    }

                    // 不是符号 “..”,进入小数点后的判断
                    Cat();  // 记录“.”

                    // “.”后不应为空，至少应该有一位小数
                    GetChar();
                    if (NumberShouldBreak())
                    {
                        throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal numbers!");
                    }

                    // 读取小数点后的数字
                    while (!NumberShouldBreak())
                    {
                        if (IsDigit())
                        {
                            Cat();
                            GetChar();
                        }
                        else if (_ch == 'e' || _ch == 'E')
                        {
                            DealE();
                            break;
                        }
                        else if(NumberShouldBreak())
                        {
                            break;
                        }
                        else
                        {
                            throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal number.");
                        }
                    }
                    MakeToken(NumberType.Real);
                    return;
                }

                // 不含小数部分，含科学计数法
                if (_ch == 'e' || _ch == 'E')
                {
                    DealE();
                    MakeToken(NumberType.Real);
                    return;
                }

                // 暂时为整数
                if (IsDigit())
                {
                    Cat();
                    GetChar();
                }
                else if(NumberShouldBreak())
                {
                    break;
                }
                else
                {
                    throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal number.");
                }
            }
            MakeToken(NumberType.Integer);
        }

    }

    private void DealE()
    {
        Cat();
        GetChar();
        if (IsDigit() || _ch == '+' || _ch == '-')
        {
            Cat();
        }
        else
        {
            throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal number.");
        }

        // 读取e后的数字
        GetChar();
        while (!NumberShouldBreak())
        {
            if (IsDigit())
            {
                Cat();
                GetChar();
            }
            else
            {
                throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal number.");
            }
        }
    }

    bool NumberShouldBreak()
    {
        if (_ch == ' ' || _ch == '\n' || _ch == '\t' || _ch == '\r' || (IsDelimiter() && _ch!='.') || IsOperator() || _finish)
        {
            Retract();
            return true;
        }

        return false;
    }

    private bool IsOperator()
    {
        foreach (var o in _operator)
        {
            if (o.Contains(_ch))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsDot()
    {
        if (_tokens.Count != 0)
        {
            SemanticToken tokenBefore = _tokens.Last();
            if (tokenBefore.TokenType == SemanticTokenType.Identifier) return true;
        }
        return false;
    }


    private void DealOther()
    {
        switch (_ch)
        {
            case '+': // 识别 +
                Cat();
                MakeToken(OperatorType.Plus);
                break;
            case '-': // 识别 -
                Cat();
                MakeToken(OperatorType.Minus);
                break;
            case '*': // 识别 *
                Cat();
                MakeToken(OperatorType.Multiply);
                break;
            case '/': // 识别 /
                Cat();
                MakeToken(OperatorType.Divide);
                break;
            case '=':
                Cat();
                MakeToken(OperatorType.Equal);
                break;
            case '<':
                Cat();
                GetChar();
                if (_ch == '=')
                {
                    // 识别 <=
                    Cat();
                    MakeToken(OperatorType.LessEqual);
                }
                else if(_ch == '>')
                {
                    // 识别 <>
                    Cat();
                    MakeToken(OperatorType.NotEqual);
                }
                else
                {
                    // 识别 <
                    Retract();
                    MakeToken(OperatorType.Less);
                }
                break;
            case '>':
                Cat();
                GetChar();
                if (_ch == '=')
                {
                    // 识别 >=
                    Cat();
                    MakeToken(OperatorType.GreaterEqual);
                }
                else
                {
                    // 识别 >
                    Retract();
                    MakeToken(OperatorType.Greater);
                }
                break;
            default:
                throw new LexemeException(LexemeErrorType.UnknownCharacterOrString, _line, _chPos, "Illegal lexeme.");
        }
    }

    private void MakeToken(SemanticTokenType tokenType)
    {
        SemanticToken? token;
        if (_token.First == null)
        {
            Console.WriteLine("11");
        }
        switch (tokenType)
        {
            case SemanticTokenType.Character:
                CharacterSemanticToken characterSemanticToken = new CharacterSemanticToken()
                {
                    LinePos = _line, CharacterPos = _chPos, LiteralValue = LinkedListToString(_token.First),
                };
                token = characterSemanticToken;
                break;
            case SemanticTokenType.Identifier:
                IdentifierSemanticToken identifierSemanticToken = new IdentifierSemanticToken()
                {
                    LinePos = _line, CharacterPos = _chPos, LiteralValue = LinkedListToString(_token.First),
                };
                token = identifierSemanticToken;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }

        if (token != null)
        {
            _tokens.Add(token);
            _tokenCount[tokenType]++;
            Console.WriteLine($"<{tokenType}>");
            Console.WriteLine(LinkedListToString(_token.First));
        }


    }

    private void MakeToken(KeywordType keywordType)
    {
        KeywordSemanticToken keywordSemanticToken = new KeywordSemanticToken
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = LinkedListToString(_token.First),
            KeywordType = keywordType
        };
        _tokens.Add(keywordSemanticToken);
        _tokenCount[SemanticTokenType.Keyword]++;
        Console.WriteLine($"<{SemanticTokenType.Keyword}> <{keywordType}>");
        Console.WriteLine(LinkedListToString(_token.First));
    }

    private void MakeToken(DelimiterType delimiterType)
    {
        DelimiterSemanticToken delimiterSemanticToken = new DelimiterSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = LinkedListToString(_token.First),
            DelimiterType = delimiterType
        };
        _tokens.Add(delimiterSemanticToken);
        _tokenCount[SemanticTokenType.Delimiter]++;
        Console.WriteLine($"<{SemanticTokenType.Delimiter}> <{delimiterType}>");
        Console.WriteLine(LinkedListToString(_token.First));
    }

    private void MakeToken(NumberType numberType)
    {
        string temp = LinkedListToString(_token.First);
        string result;
        if (numberType == NumberType.Hex)
        {
            result = string.Concat("0x", temp.AsSpan(1, temp.Length - 1));
        }
        else
        {
            result = temp;
        }

        NumberSemanticToken numberSemanticToken = new NumberSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = result,
            NumberType = numberType
        };
        _tokens.Add(numberSemanticToken);
        _tokenCount[SemanticTokenType.Number]++;
        Console.WriteLine($"<{SemanticTokenType.Number}> <{numberType}>");
        Console.WriteLine(LinkedListToString(_token.First));
    }

    private void MakeToken(OperatorType operatorType)
    {
        OperatorSemanticToken operatorSemanticToken = new OperatorSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = LinkedListToString(_token.First),
            OperatorType = operatorType
        };
        _tokens.Add(operatorSemanticToken);
        _tokenCount[SemanticTokenType.Operator]++;
        Console.WriteLine($"<{SemanticTokenType.Operator}> <{operatorType}>");
        Console.WriteLine(LinkedListToString(_token.First));
    }

    // 读取字符操作
    void GetChar() {
        if (_fwdPos >= 0 && _fwdPos < source.Length)
        {
            _ch = source[_fwdPos];
            _chPos++;
            _fwdPos++;
        }
        else if (_fwdPos == source.Length)
        {
            _ch = '\0';
            _chPos++;
            _finish = true;
        }
    }

    private void GetNbc() {
        while (_ch == ' ' || _ch == '\n' || _ch == '\t' || _ch == '\r') {
            if (_ch == '\n') {
                _line++;
                _chPos = 0;
            }
            GetChar();
        }
    }

    private void Retract() {
        _fwdPos -= 2;
        _chPos -= 2;
        GetChar();
    }

    private void Cat()
    {
        _token.AddLast(_ch);
        // cout << "加入" << ch << endl;
    }

    private string LinkedListToString(LinkedListNode<char> first)
    {
        // 使用 StringBuilder 来构建字符串
        StringBuilder sb = new StringBuilder();
        for (LinkedListNode<char> node = first; node != null; node = node.Next)
        {
            sb.Append(node.Value);
        }

        // 将 StringBuilder 的内容转换为字符串
        string result = sb.ToString();

        return result;
    }

    // 判断字符
    private bool IsDigit() {
        if (_ch >= '0' && _ch <= '9') return true;
        return false;
    }

    private bool IsHexDigit()
    {
        if ((_ch >= '0' && _ch <= '9') || (_ch<= 'F' && _ch >= 'A')) return true;
        return false;
    }

    private bool IsLetter() {
        if ((_ch >= 'A' && _ch <= 'Z') || (_ch >= 'a' && _ch <= 'z' || _ch == '_')) {
            return true;
        }
        return false;
    }

    private bool IsKeyword()
    {
        string tokenString = LinkedListToString(_token.First);

        foreach (var t in _keywords)
        {
            if (string.Equals(tokenString, t, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }


    private bool IsDelimiter()
    {
        foreach (var delimiter in _delimiter)
        {
            if (delimiter.Contains(_ch))
            {
                return true;
            }
        }
        return false;
    }

    private char PeekNextChar()
    {
        // 确认下一个位置是否仍在buffer的范围内
        if (_fwdPos < source.Length)
        {
            return source[_fwdPos];
        }
        return '\0';

    }



    private void PrintToken(SemanticTokenType type, LinkedListNode<char> token, uint line)
    {
        string tokenString = LinkedListToString(token);
        string typeName = Enum.GetName(typeof(SemanticTokenType), type) ?? "Unknown";
        Console.WriteLine($"{line} <{typeName.ToUpperInvariant()},{tokenString}>");
    }

    // PrintToken(SemanticTokenType.Keyword, "if", 42); // 假设'if'是token，42是行号

    private void PrintError(int type, LinkedListNode<char> token, uint line)
    {
        string tokenString = LinkedListToString(token);
        switch (type)
        {
            case 0:
                Console.WriteLine($"{line} <ERROR,{tokenString}>");
                break;
            case 1:
                Console.WriteLine($"{line} <ERROR,@>");
                break;
        }
    }

    // PrintError(0, "unexpected symbol", 42); // 假设 "unexpected symbol" 是错误的 token，42 是行号

    private void PrintResult()
    {
        Console.WriteLine(_line);
        foreach (var pair in _tokenCount)
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
    }
}

