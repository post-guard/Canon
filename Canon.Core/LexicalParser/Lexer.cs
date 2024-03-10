

namespace Canon.Core.LexicalParser
{
    public class Lexer
    {
        private readonly LinkedList<char> source;
        private LinkedListNode<char>? currentNode;
        private uint line = 1;
        private uint charPosition = 0;
        private List<SemanticToken> tokens = new List<SemanticToken>();

        public Lexer(string source)
        {
            // 将字符串转换为LinkedList<char>
            this.source = new LinkedList<char>(source);
            currentNode = this.source.First;
        }

        public List<SemanticToken> Tokenize()
        {
            while (currentNode != null)
            {
                charPosition = 0; // 重置字符位置
                SkipWhitespace();

                if (currentNode == null) break; // 如果跳过空格后到达了末尾，则退出循环

                SemanticToken? token = null;

                // 尝试解析各种类型的词法单元
                if (DelimiterSemanticToken.TryParse(line, charPosition, currentNode, out var delimiterToken))
                {
                    token = delimiterToken;
                }
                else if (CharacterSemanticToken.TryParse(line, charPosition, currentNode, out var characterToken))
                {
                    token = characterToken;
                }
                else if (KeywordSemanticToken.TryParse(line, charPosition, currentNode, out var keywordToken))
                {
                    token = keywordToken;
                }
                else if (OperatorSemanticToken.TryParse(line, charPosition, currentNode, out var operatorToken))
                {
                    token = operatorToken;
                }
                else if (NumberSemanticToken.TryParse(line, charPosition, currentNode, out var numberToken))
                {
                    token = numberToken;
                }
                else if (IdentifierSemanticToken.TryParse(line, charPosition, currentNode, out var identifierToken))
                {
                    token = identifierToken;
                }

                if (token != null)
                {
                    tokens.Add(token);
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
            return tokens;
        }

        private void SkipWhitespace()
        {
            while (currentNode != null && char.IsWhiteSpace(currentNode.Value))
            {
                if (currentNode.Value == '\n')
                {
                    line++;
                    charPosition = 0;
                }
                currentNode = currentNode.Next;
            }
        }

        private void MoveCurrentNode(int steps)
        {
            for (int i = 0; i < steps && currentNode != null; i++)
            {
                currentNode = currentNode.Next;
            }
        }
    }
}
