using System.Text;
using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.Exceptions;

namespace Canon.Core.LexicalParser;

public class Lexer : ILexer
{
    // 记录token
    private SemanticToken? _semanticToken;
    private readonly StringBuilder _tokenBuilder = new();
    private List<SemanticToken> _tokens = [];

    // 状态机
    private StateType _state = StateType.Start;
    private char _ch;
    private bool _finish;

    // 文件读取
    private ISourceReader _reader;
    private uint _line = 1;
    private uint _chPos;

    public IEnumerable<SemanticToken> Tokenize(ISourceReader reader)
    {
        _reader = reader;
        _tokens = [];
        _state = StateType.Start;

        while (_state != StateType.Done)
        {
            switch (_state)
            {
                case StateType.Start:
                    HandleStartState();
                    break;
                case StateType.Comment:
                    if (_ch == '{')
                    {
                        HandleCommentStateBig();
                    }
                    else if (_ch == '*')
                    {
                        HandleCommentStateSmall();
                    }
                    else
                    {
                        HandleCommentSingleLine();
                    }

                    break;
                case StateType.Num:
                    HandleNumState();
                    break;
                case StateType.Word:
                    HandleWordState();
                    break;
                case StateType.Delimiter:
                    HandleDelimiterState();
                    break;
                case StateType.Operator:
                    HandleOperatorState();
                    break;
                case StateType.BreakPoint:
                    while (LexRules.IsBreakPoint(_ch))
                    {
                        GetChar();
                    }

                    Retract();
                    _state = StateType.Start;
                    break;
                case StateType.Unknown:
                    throw new LexemeException(LexemeErrorType.UnknownCharacterOrString, _line, _chPos,
                        "Illegal lexeme.");
                case StateType.Done:
                    break;
            }
        }

        _tokens.Add(SemanticToken.End);

        return _tokens;
    }

    private void HandleStartState()
    {
        // 初始化
        ResetTokenBuilder();

        // 读取首个字符
        GetChar();

        if (_finish)
        {
            _state = StateType.Done;
            return;
        }

        // 根据首个字符判断可能的情况
        if (_ch == '{') // 以 “{” 开头，为注释
        {
            _state = StateType.Comment;
        }
        else if (_ch == '(')
        {
            char nextChar = PeekNextChar();
            if (nextChar == '*')
            {
                GetChar();
                _state = StateType.Comment;
            }
            else
            {
                _state = StateType.Delimiter;
            }
        }
        else if (_ch == '/')
        {
            char nextChar = PeekNextChar();
            if (nextChar == '/')
            {
                GetChar();
                _state = StateType.Comment;
            }
            else
            {
                _state = StateType.Operator;
            }
        }
        else if (_ch == '.') // 以 “.” 开头，可能是数字或分隔符
        {
            char next = PeekNextChar();
            if (next is >= '0' and <= '9')
            {
                _state = StateType.Num;
            }
            else
            {
                _state = StateType.Delimiter;
            }
        }
        else if (LexRules.IsLetter(_ch)) // 以字母开头，为关键字或标识符
        {
            _state = StateType.Word;
        }
        else if (LexRules.IsDigit(_ch) || _ch == '$') // 以数字或 “$” 开头，为数值
        {
            _state = StateType.Num;
        }
        else if (LexRules.IsDelimiter(_ch)) // 为分隔符
        {
            _state = StateType.Delimiter;
        }
        else if (LexRules.IsOperator(_ch)) // 为运算符
        {
            _state = StateType.Operator;
        }
        else if (LexRules.IsBreakPoint(_ch))
        {
            _state = StateType.BreakPoint;
        }
        else
        {
            _state = StateType.Unknown;
        }
    }

    private void HandleCommentStateBig()
    {
        while (_ch != '}')
        {
            GetChar();

            if (_finish)
            {
                throw new LexemeException(LexemeErrorType.UnclosedComment, _line, _chPos,
                    "The comment is not closed.");
            }
        }

        _state = StateType.Start;
    }

    private void HandleCommentStateSmall()
    {
        bool commentClosed = false;
        while (!commentClosed)
        {
            GetChar();
            while (_ch != '*')
            {
                GetChar();
                if (_finish)
                {
                    throw new LexemeException(LexemeErrorType.UnclosedComment, _line, _chPos,
                        "The comment is not closed.");
                }
            }

            GetChar();
            if (_finish)
            {
                throw new LexemeException(LexemeErrorType.UnclosedComment, _line, _chPos,
                    "The comment is not closed.");
            }

            if (_ch == ')') commentClosed = true;
        }

        _state = StateType.Start;
    }

    private void HandleCommentSingleLine()
    {
        while (_ch != '\n')
        {
            GetChar();
        }

        _state = StateType.Start;
    }

    private void HandleWordState()
    {
        while (LexRules.IsDigit(_ch) || LexRules.IsLetter(_ch))
        {
            Cat();
            GetChar();
        }

        Retract();

        string tokenString = GetCurrentTokenString();
        if (LexRules.GetKeywordTypeByKeywprd(tokenString, out KeywordType keywordType))
        {

            _semanticToken = LexemeFactory.MakeToken(keywordType, tokenString, _line, _chPos);
        }
        else
        {
            _semanticToken = LexemeFactory.MakeToken(SemanticTokenType.Identifier, tokenString, _line, _chPos);
        }

        AddToTokens(_semanticToken);
        _state = StateType.Start;
    }

    private void HandleNumState()
    {
        NumberType numberType = NumberType.Integer;
        // 十六进制
        if (_ch == '$')
        {
            ProcessHex();
            numberType = NumberType.Hex;
        }
        // 非十六进制
        else if (LexRules.IsDigit(_ch) || _ch == '.')
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
                        _state = StateType.Delimiter;
                        break;
                    }

                    // 不是符号 “..”,进入小数点后的判断
                    Cat(); // 记录“.”

                    // “.”后不应为空，至少应该有一位小数
                    GetChar();
                    if (NumberShouldBreak())
                    {
                        throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos,
                            "Illegal numbers!");
                    }

                    // 读取小数点后的数字
                    while (!NumberShouldBreak())
                    {
                        if (LexRules.IsDigit(_ch))
                        {
                            Cat();
                            GetChar();
                        }
                        else if (_ch == 'e' || _ch == 'E')
                        {
                            ProcessE();
                            break;
                        }
                        else if (NumberShouldBreak())
                        {
                            break;
                        }
                        else
                        {
                            throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos,
                                "Illegal number.");
                        }
                    }

                    numberType = NumberType.Real;
                    break;
                }

                // 不含小数部分，含科学计数法
                if (_ch == 'e' || _ch == 'E')
                {
                    ProcessE();
                    numberType = NumberType.Real;
                    break;
                }

                // 暂时为整数
                if (LexRules.IsDigit(_ch))
                {
                    Cat();
                    GetChar();
                }
                else if (NumberShouldBreak())
                {
                    numberType = NumberType.Integer;
                    break;
                }
                else
                {
                    throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos, "Illegal number.");
                }
            }
        }

        _semanticToken = LexemeFactory.MakeToken(numberType, GetCurrentTokenString(),
            _line, _chPos);
        AddToTokens(_semanticToken);
        _state = StateType.Start;
    }

    private void ProcessHex()
    {
        Cat();
        GetChar();

        while (!NumberShouldBreak())
        {
            // 假设IsHexDigit方法能够识别十六进制数字
            if (LexRules.IsHexDigit(_ch))
            {
                Cat();
                GetChar();
            }
            else if (NumberShouldBreak())
            {
                break;
            }
            else
            {
                throw new LexemeException(LexemeErrorType.IllegalNumberFormat, _line, _chPos,
                    "Illegal hex numbers!");
            }
        }
    }

    private void ProcessE()
    {
        Cat();
        GetChar();
        if (LexRules.IsDigit(_ch) || _ch == '+' || _ch == '-')
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
            if (LexRules.IsDigit(_ch))
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
        if (_ch == ' ' || _ch == '\n' || _ch == '\t' || _ch == '\r' || (LexRules.IsDelimiter(_ch) && _ch != '.') ||
            LexRules.IsOperator(_ch) || _finish)
        {
            Retract();
            return true;
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

    private void HandleDelimiterState()
    {
        Cat();
        switch (_ch)
        {
            case '.':
                {
                    GetChar();
                    if (_ch == '.')
                    {
                        Cat();
                        _semanticToken = LexemeFactory.MakeToken(DelimiterType.DoubleDots, "..", _line, _chPos);
                        break;
                    }

                    Retract();
                    if (IsDot())
                    {
                        _semanticToken = LexemeFactory.MakeToken(DelimiterType.Dot, ".", _line, _chPos);
                    }
                    else
                    {
                        _semanticToken = LexemeFactory.MakeToken(DelimiterType.Period, ".", _line, _chPos);
                    }
                }
                break;
            case '\'':
                {
                    // 重置_token，准备收集字符串内容
                    ResetTokenBuilder();

                    GetChar(); // 移动到下一个字符，即字符串的第一个字符
                    while (_ch != '\'' && _ch != '\"')
                    {
                        Cat(); // 收集字符
                        GetChar(); // 移动到下一个字符
                        if (_ch == '\n' || _finish)
                        {
                            throw new LexemeException(LexemeErrorType.UnclosedStringLiteral, _line, _chPos,
                                "The String is not closed.");
                        }
                    }

                    string currentString = GetCurrentTokenString();
                    if (currentString.Length > 1)
                    {
                        _semanticToken = LexemeFactory.MakeToken(SemanticTokenType.String,
                            currentString, _line, _chPos);
                    }
                    else
                    {
                        _semanticToken = LexemeFactory.MakeToken(SemanticTokenType.Character,
                            currentString, _line, _chPos);
                    }


                    ResetTokenBuilder();

                    if (!(_ch == '\'' || _ch == '\"'))
                    {
                        throw new LexemeException(LexemeErrorType.UnclosedStringLiteral, _line, _chPos,
                            "The String is not closed.");
                    }
                }
                break;
            case ',':
                _semanticToken = LexemeFactory.MakeToken(DelimiterType.Comma, ",", _line, _chPos);

                break;
            case ':':
                char nextChar = PeekNextChar();
                if (nextChar == '=')
                {
                    GetChar();
                    Cat();
                    _semanticToken = LexemeFactory.MakeToken(OperatorType.Assign, ":=", _line, _chPos);
                }
                else
                {
                    _semanticToken = LexemeFactory.MakeToken(DelimiterType.Colon, ":", _line, _chPos);
                }

                break;
            case ';':
                _semanticToken = LexemeFactory.MakeToken(DelimiterType.Semicolon, ";", _line, _chPos);

                break;
            case '(':
                _semanticToken = LexemeFactory.MakeToken(DelimiterType.LeftParenthesis, "(", _line, _chPos);
                break;
            case ')':
                _semanticToken = LexemeFactory.MakeToken(DelimiterType.RightParenthesis, ")", _line, _chPos);

                break;
            case '[':
                _semanticToken = LexemeFactory.MakeToken(DelimiterType.LeftSquareBracket, "[", _line, _chPos);

                break;
            case ']':
                _semanticToken = LexemeFactory.MakeToken(DelimiterType.RightSquareBracket, "]", _line, _chPos);
                break;
        }

        if (_semanticToken is null)
        {
            throw new InvalidOperationException();
        }
        _tokens.Add(_semanticToken);
        _state = StateType.Start;
    }

    private void HandleOperatorState()
    {
        switch (_ch)
        {
            case '+': // 识别 +
                Cat();
                _semanticToken = LexemeFactory.MakeToken(OperatorType.Plus, "+", _line, _chPos);
                AddToTokens(_semanticToken);
                break;
            case '-': // 识别 -
                Cat();
                _semanticToken = LexemeFactory.MakeToken(OperatorType.Minus, "-", _line, _chPos);
                AddToTokens(_semanticToken);
                break;
            case '*': // 识别 *
                Cat();
                _semanticToken = LexemeFactory.MakeToken(OperatorType.Multiply, "*", _line, _chPos);
                AddToTokens(_semanticToken);
                break;
            case '/': // 识别 /
                Cat();
                _semanticToken = LexemeFactory.MakeToken(OperatorType.Divide, "/", _line, _chPos);
                AddToTokens(_semanticToken);
                break;
            case '=':
                Cat();
                _semanticToken = LexemeFactory.MakeToken(OperatorType.Equal, "=", _line, _chPos);
                AddToTokens(_semanticToken);
                break;
            case '<':
                Cat();
                GetChar();
                if (_ch == '=')
                {
                    // 识别 <=
                    Cat();
                    _semanticToken = LexemeFactory.MakeToken(OperatorType.LessEqual, "<=", _line, _chPos);
                    AddToTokens(_semanticToken);
                }
                else if (_ch == '>')
                {
                    // 识别 <>
                    Cat();
                    _semanticToken = LexemeFactory.MakeToken(OperatorType.NotEqual, ">", _line, _chPos);
                    AddToTokens(_semanticToken);
                }
                else
                {
                    // 识别 <
                    Retract();
                    _semanticToken = LexemeFactory.MakeToken(OperatorType.Less, "<", _line, _chPos);
                    AddToTokens(_semanticToken);
                }

                break;
            case '>':
                Cat();
                GetChar();
                if (_ch == '=')
                {
                    // 识别 >=
                    Cat();
                    _semanticToken = LexemeFactory.MakeToken(OperatorType.GreaterEqual, ">=", _line, _chPos);
                    AddToTokens(_semanticToken);
                }
                else
                {
                    // 识别 >
                    Retract();
                    _semanticToken = LexemeFactory.MakeToken(OperatorType.Greater, ">", _line, _chPos);
                    AddToTokens(_semanticToken);
                }

                break;
            default:
                throw new LexemeException(LexemeErrorType.UnknownCharacterOrString, _line, _chPos, "Illegal lexeme.");
        }

        _state = StateType.Start;
    }

    private void AddToTokens(SemanticToken semanticToken)
    {
        _tokens.Add(semanticToken);
    }

    private void Cat()
    {
        _tokenBuilder.Append(_ch); // 使用StringBuilder追加字符
    }

    private string GetCurrentTokenString()
    {
        return _tokenBuilder.ToString(); // 从StringBuilder获取当前记号的字符串
    }

    private void ResetTokenBuilder()
    {
        _tokenBuilder.Clear(); // 清空StringBuilder以复用
    }

    private char PeekNextChar()
    {
        // 确认下一个位置是否仍在buffer的范围内
        if (_reader.TryPeekChar(out char? c))
        {
            return c.Value;
        }
        else
        {
            return char.MinValue;
        }
    }

    void GetChar()
    {
        if (_finish)
        {
            return;
        }

        _finish = !_reader.MoveNext();

        if (_finish)
        {
            _ch = char.MinValue;
            return;
        }

        _ch = _reader.Current;
        _line = _reader.Line;
        _chPos = _reader.Pos;
    }

    void Retract()
    {
        _reader.Retract();
    }
}
