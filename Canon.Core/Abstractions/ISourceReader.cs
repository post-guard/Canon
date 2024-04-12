using System.Diagnostics.CodeAnalysis;

namespace Canon.Core.Abstractions;

/// <summary>
/// 读取源代码的接口
/// </summary>
public interface ISourceReader
{
    /// <summary>
    /// 尝试读取下一个字符
    /// </summary>
    /// <param name="c">读取到的字符</param>
    /// <returns>是否成功读取</returns>
    public bool TryReadChar([NotNullWhen(true)] out char? c);

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
}
