using System.Text;
using Canon.Core.Enums;

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

    private readonly string[] _delimiter = [";", ",", ":", ".", "(", ")", "[", "]","'","\"",".."];

    // 状态机
    private StateType _state;
    private char _ch;

    private LinkedList<char> _token = new LinkedList<char>();
    // bool save;
    // int saved_state;
    bool _finish;
    private bool eof;

    //缓冲区
    private readonly char[] _buffer = new char[2048];
    // int start_pos;
    private int _fwdPos;

    // 计数器
    private uint _line = 1;
    private uint _chPos;
    private int _sourcePos;
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
        FillLeftBuffer();

        // 状态机
        _finish = false;

        while (!_finish) {
            GetChar();
            GetNbc();

            _token = new LinkedList<char>();

            if (IsLetter()) {
                _state = StateType.Word;
            }
            else if (IsDigit()) {
                _state = StateType.Digit;
            }
            else if (IsDelimiter()) {
                _state = StateType.Delimiter;
            }
            else
            {
                _state = StateType.Other;
            }

            switch (_state)
            {
            case StateType.Word: {
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
                else {
                    MakeToken(SemanticTokenType.Identifier);
                }
                break;
            }

            case StateType.Digit:
                {
                    bool error = false;
                    bool tag = false; // 用于标记是否已经处理过科学记数法的指数部分
                    bool doubleDot = false;
                    NumberType numberType = NumberType.Integer;

                    while (IsDigit() || _ch == '.' || _ch == 'E' || _ch == '+' || _ch == '-' || _ch == 'e' || IsLetter()) {
                        if (_ch != '.')
                        {
                            Cat();
                        }


                        if (_ch == '0' && !tag) {
                            GetChar();
                            if (_ch == 'x' || _ch == 'X') {
                                numberType = NumberType.Hex;    // 标识十六进制
                                Cat();
                                while (IsHexDigit()) { // 假设IsHexDigit方法能够识别十六进制数字
                                    Cat();
                                }
                                break;
                            }
                            Retract(); // 如果不是'x'或'X'，回退一个字符
                        }
                        else if (_ch == '.') {
                            GetChar();
                            if (_ch == '.') {
                                Retract(); // 回退到第一个'.'
                                Retract(); // 回退到'.'之前的数字
                                doubleDot = true;
                                break;
                            }
                            Retract();
                            Cat();
                            numberType = NumberType.Real;
                        }
                        else if ((_ch == 'e' || _ch == 'E') && !tag) {
                            GetChar();
                            if (IsDigit() || _ch == '+' || _ch == '-') {
                                Cat();
                                tag = true; // 已处理指数部分
                                continue;
                            }
                            error = true; // 错误的科学记数法
                            break;
                        }

                        GetChar();
                    }

                    if (!error) {
                        MakeToken(numberType);
                        if (doubleDot)
                        {
                            break;
                        }
                        Retract();
                    }
                    else
                    {
                        Retract();
                        PrintError(0,_token.First,_line);
                        _tokenCount[SemanticTokenType.Error]++;
                    }
                    break;
                }

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
                        if (IsPeriod())
                        {

                        }else if (IsDot())
                        {

                        }
                    }
                    break;
                case '\'':
                case '\"':
                    {
                        if(_ch == '\'') MakeToken(DelimiterType.SingleQuotation);
                        else if(_ch == '\"') MakeToken(DelimiterType.DoubleQuotation);

                        // 重置_token，准备收集字符串内容
                        _token = new LinkedList<char>();

                        GetChar(); // 移动到下一个字符，即字符串的第一个字符
                        while (_ch != '\'' && _ch != '\"')
                        {
                            Cat(); // 收集字符
                            GetChar(); // 移动到下一个字符
                        }

                        // 在退出循环时，_ch为'或EOF，此时_token包含字符串内容
                        // 创建字符内容的token，注意这里使用SemanticTokenType.String表示字符串字面量
                        MakeToken(SemanticTokenType.Character); // 或其它适用于字符串字面量的SemanticTokenType
                        _token = new LinkedList<char>(); // 重置_token

                        if (_ch == '\'' && _ch != '\n')
                        {
                            // 识别并创建最后一个单引号的token
                            Cat();
                            MakeToken(DelimiterType.SingleQuotation);
                        }
                        else if (_ch == '\"')
                        {
                            Cat();
                            MakeToken(DelimiterType.DoubleQuotation);
                        }
                        else
                        {
                            // 这里处理遇到EOF但没有闭合单引号的情况，例如：'字符串结尾没有单引号
                            // 可以添加错误处理代码
                            PrintError(0, _token.First, _line); // 假设这个方法用于打印错误
                        }
                    }
                    break;
                case ',':
                    MakeToken(DelimiterType.Comma);
                    break;
                case ':':
                    MakeToken(DelimiterType.Colon);
                    break;
                case ';':
                    MakeToken(DelimiterType.Semicolon);
                    break;
                case '(':
                    MakeToken(DelimiterType.LeftParenthesis);
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

            case StateType.Other:
                DealOther();
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }

        }
        PrintResult();
        return _tokens;
    }

    private bool IsDot()
    {
        SemanticToken tokenBefore = _tokens.Last();
        if (tokenBefore.TokenType == SemanticTokenType.Identifier) return true;
        return false;
    }

    private bool IsPeriod()
    {
        SemanticToken tokenBefore = _tokens.Last();
        if (tokenBefore.TokenType == SemanticTokenType.Keyword) return true;
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
            case ':':
                Cat();
                GetChar();
                if (_ch == '=')
                {
                    // 识别 :=
                    Cat();
                    MakeToken(OperatorType.Assign);
                }
                else
                {
                    // 这里应该被识别为delimiter逻辑上
                    Cat();
                    PrintError(1, _token.First, _line);
                    _tokenCount[SemanticTokenType.Error]++;
                }
                break;
            default:
                Cat();
                PrintError(1, _token.First, _line);
                _tokenCount[SemanticTokenType.Error]++;
                break;
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
            case SemanticTokenType.Error:
                ErrorSemanticToken errorSemanticToken = new ErrorSemanticToken()
                {
                    LinePos = _line, CharacterPos = _chPos, LiteralValue = LinkedListToString(_token.First),
                };
                token = errorSemanticToken;
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

    private void MakeToken(NumberType numberType)
    {
        NumberSemanticToken numberSemanticToken = new NumberSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = LinkedListToString(_token.First),
            NumberType = numberType
        };
        _tokens.Add(numberSemanticToken);
        _tokenCount[SemanticTokenType.Number]++;
        Console.WriteLine($"<{SemanticTokenType.Number}> <{numberType}>");
        Console.WriteLine(LinkedListToString(_token.First));
    }

    // 填充buffer操作
    private void FillLeftBuffer() {
        //cout << "fill left" << endl;
        for (int i = 0; i < _buffer.Length / 2; i++) {
            _buffer[i] = '$';
        }

        // 确保source字符串足够长，避免超出范围
        int lengthToCopy = Math.Min(_buffer.Length / 2 - 1, source.Length - _sourcePos);

        // 使用Array.Copy方法
        Array.Copy(source.ToCharArray(), _sourcePos, _buffer, 0, lengthToCopy);

        _sourcePos += lengthToCopy;

        if (_sourcePos == source.Length) {
            eof = true;
        }
    }

    private void FillRightBuffer() {
        //cout << "fill right" << endl;
        for (int i = _buffer.Length / 2; i < _buffer.Length; i++) {
            _buffer[i] = '$';
        }

        // 确保source字符串足够长，避免超出范围
        int lengthToCopy = Math.Min(_buffer.Length / 2 - 1, source.Length - _sourcePos);

        // 使用Array.Copy方法
        Array.Copy(source.ToCharArray(), _sourcePos, _buffer, _buffer.Length / 2, lengthToCopy);

        _sourcePos += lengthToCopy;

        if (_sourcePos == source.Length) {
            eof = true;
        }
    }

    private void PrintBuffer() {
        for (int i = 0; i < _buffer.Length; i++) {
            Console.WriteLine($"[{i}] {_buffer[i]}");
        }
    }

    void DealEof() {
        if (eof) _finish = true;
        else if (_fwdPos < _buffer.Length / 2) {
            FillRightBuffer();
            _fwdPos = _buffer.Length / 2;
        }
        else {
            FillLeftBuffer();
            // start_pos = 0;
            _fwdPos = 0;
        }
    }

    // 读取buffer操作
    void GetChar() {
        if (_fwdPos >= 0 && _fwdPos < _buffer.Length) _ch = _buffer[_fwdPos];
        _chPos++;
        if (_ch == '$') {
            DealEof();
            if (_fwdPos >= 0 && _fwdPos < _buffer.Length) _ch = _buffer[_fwdPos];
        }
        if (_fwdPos < _buffer.Length) _fwdPos++;
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
                if (_ch != ':')
                {
                    return true;
                }

                GetChar();
                if (_ch == '=')
                {
                    Retract();
                    return false;
                }

                return true;
            }
        }
        return false;
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

