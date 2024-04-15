using System.Diagnostics.CodeAnalysis;
using Canon.Core.Abstractions;

namespace Canon.Tests.Utils;

/// <summary>
/// 从字符串中读取源代码
/// </summary>
public sealed class StringSourceReader(string source) : ISourceReader, IDisposable
{
    private readonly IEnumerator<char> _enumerator =
        source.GetEnumerator();

    public uint Line { get; private set; } = 1;

    public uint Pos { get; private set; }

    public string FileName => "string";

    public bool TryReadChar([NotNullWhen(true)] out char? c)
    {
        if (Pos != 0 || Line != 1)
        {
            // 不是第一次读取
            if (_enumerator.Current == '\n')
            {
                Pos = 0;
                Line += 1;
            }
        }

        if (!_enumerator.MoveNext())
        {
            c = null;
            return false;
        }

        Pos += 1;
        c = _enumerator.Current;
        return true;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }
}
