using System.Diagnostics.CodeAnalysis;

namespace Canon.Core.Abstractions;

/// <summary>
/// 读取源代码的接口
/// </summary>
public interface ISourceReader
{
    public char Current { get; }

    /// <summary>
    /// 源文件名称
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// 当前读取字符的行号
    /// </summary>
    public uint Line { get; }

    /// <summary>
    /// 当前读取字符的列号
    /// </summary>
    public uint Pos { get; }

    /// <summary>
    /// 回退一个字符
    /// </summary>
    /// <returns>回退是否成功</returns>
    public bool Retract();

    /// <summary>
    /// 前进一个字符
    /// </summary>
    /// <returns></returns>
    public bool MoveNext();

    /// <summary>
    /// 读取下一个字符但是移进
    /// </summary>
    /// <param name="c">读取到的下一个字符</param>
    /// <returns>是否能够读取下一个字符</returns>
    public bool TryPeekChar([NotNullWhen(true)] out char? c);
}
