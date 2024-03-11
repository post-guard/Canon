namespace Canon.Core.LexicalParser;

public class Lexer
{
    private readonly LinkedList<char> _source;
    private LinkedListNode<char>? _currentNode;
    private uint _line = 1;
    private uint _charPosition;
    private readonly List<SemanticToken> _tokens = [];

    public Lexer(string source)
    {
        // 将字符串转换为LinkedList<char>
        _source = new LinkedList<char>(source);
        _currentNode = _source.First;
    }

    public List<SemanticToken> Tokenize()
    {
        while (_currentNode != null)
        {
            _charPosition = 0; // 重置字符位置
            SkipWhitespace();

            if (_currentNode == null) break; // 如果跳过空格后到达了末尾，则退出循环

            SemanticToken? token = null;

            // 尝试解析各种类型的词法单元
            if (DelimiterSemanticToken.TryParse(_line, _charPosition, _currentNode, out var delimiterToken))
            {
                token = delimiterToken;
            }
            else if (CharacterSemanticToken.TryParse(_line, _charPosition, _currentNode, out var characterToken))
            {
                token = characterToken;
            }
            else if (KeywordSemanticToken.TryParse(_line, _charPosition, _currentNode, out var keywordToken))
            {
                token = keywordToken;
            }
            else if (OperatorSemanticToken.TryParse(_line, _charPosition, _currentNode, out var operatorToken))
            {
                token = operatorToken;
            }
            else if (NumberSemanticToken.TryParse(_line, _charPosition, _currentNode, out var numberToken))
            {
                token = numberToken;
            }
            else if (IdentifierSemanticToken.TryParse(_line, _charPosition, _currentNode, out var identifierToken))
            {
                token = identifierToken;
            }

            if (token != null)
            {
                _tokens.Add(token);
                // 根据词法单元的长度移动currentNode
                MoveCurrentNode(token.LiteralValue.Length);
            }
            else
            {
                // 未能识别的字符，跳过
                MoveCurrentNode(1);
            }
        }

        // tokens.Add(new EOFToken(line, charPosition)); // 添加EOF标记
        return _tokens;
    }

    private void SkipWhitespace()
    {
        while (_currentNode != null && char.IsWhiteSpace(_currentNode.Value))
        {
            if (_currentNode.Value == '\n')
            {
                _line++;
                _charPosition = 0;
            }
            _currentNode = _currentNode.Next;
        }
    }

    private void MoveCurrentNode(int steps)
    {
        for (int i = 0; i < steps && _currentNode != null; i++)
        {
            _currentNode = _currentNode.Next;
        }
    }
}
