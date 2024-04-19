using System.Diagnostics.CodeAnalysis;
using Canon.Core.Abstractions;

namespace Canon.Server.Models;

public class CodeReader(SourceCode code) : ISourceReader
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
                return code.Code[_pos];
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
        if (_pos >= code.Code.Length - 1)
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
        if (_pos >= code.Code.Length - 1)
        {
            c = null;
            return false;
        }

        c = code.Code[_pos + 1];
        return true;
    }
}
