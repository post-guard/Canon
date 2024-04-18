using System.Diagnostics.CodeAnalysis;
using Canon.Core.Abstractions;

namespace Canon.Tests.Utils;

/// <summary>
/// 从字符串中读取源代码
/// </summary>
public sealed class StringSourceReader(string source) : ISourceReader
{
    private int _pos = -1;

    private uint _lastPos;

    public uint Line { get; private set; } = 1;

    public uint Pos { get; private set; }

    public string FileName => "string";

    public char Current
    {
        get
        {
            if (_pos == -1)
            {
                throw new InvalidOperationException("Reader at before the start.");
            }
            else
            {
                return source[_pos];
            }
        }
    }

    public bool Retract()
    {
        if (_pos <= 0)
        {
            return false;
        }

        _pos -= 1;
        if (Current == '\n')
        {
            Line -= 1;
            // TODO: 如果一直回退就完蛋了
            Pos = _lastPos;
        }
        else
        {
            Pos -= 1;
        }
        return true;
    }

    public bool MoveNext()
    {
        if (_pos >= source.Length - 1)
        {
            return false;
        }

        if (_pos != -1 && Current == '\n')
        {
            Line += 1;
            _lastPos = Pos;
            Pos = 0;
        }

        _pos += 1;
        Pos += 1;
        return true;
    }

    public bool TryPeekChar([NotNullWhen(true)] out char? c)
    {
        if (_pos >= source.Length - 1)
        {
            c = null;
            return false;
        }

        c = source[_pos + 1];
        return true;
    }
}
