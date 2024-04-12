using Canon.Core.LexicalParser;

namespace Canon.Core.Abstractions;

/// <summary>
/// 词法分析器接口
/// </summary>
public interface ILexer
{
    public IEnumerable<SemanticToken> Tokenize(ISourceReader reader);
}
