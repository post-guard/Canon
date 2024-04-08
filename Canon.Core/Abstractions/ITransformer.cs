using Canon.Core.GrammarParser;

namespace Canon.Core.Abstractions;

/// <summary>
/// 进行归约需要的信息
/// </summary>
/// <param name="Length">归约的长度</param>
/// <param name="Left">归约得到的左部符号</param>
public record ReduceInformation(int Length, NonTerminator Left)
{
    public string GenerateCode()
    {
        return $"new ReduceInformation({Length}, {Left.GenerateCode()})";
    }
}
/// <summary>
/// 状态的各种迁移信息
/// </summary>
public interface ITransformer
{
    public string Name { get; }

    /// <summary>
    /// 进行移进的信息
    /// </summary>
    public IDictionary<TerminatorBase, ITransformer> ShiftTable { get; }

    /// <summary>
    /// 进行归约的信息
    /// </summary>
    public IDictionary<Terminator, ReduceInformation> ReduceTable { get; }
}
