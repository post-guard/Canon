using Canon.Core.Abstractions;

namespace Canon.Tests.Utils;

public class StringSourceReaderTests
{
    [Fact]
    public void LineFeedTest()
    {
        ISourceReader reader = new StringSourceReader("program Main;\nbegin\nend.\n");
        reader.MoveNext();

        CheckLine(reader, "program Main;", 1);
        reader.MoveNext();
        CheckLine(reader, "begin", 2);
        reader.MoveNext();
        CheckLine(reader, "end.", 3);
    }

    [Fact]
    public void CarriageReturnLineFeedTest()
    {
        ISourceReader reader = new StringSourceReader("program Main;\r\nbegin\r\nend.\r\n");
        reader.MoveNext();

        CheckLine(reader, "program Main;", 1);
        reader.MoveNext();
        reader.MoveNext();
        CheckLine(reader, "begin", 2);
        reader.MoveNext();
        reader.MoveNext();
        CheckLine(reader, "end.", 3);
    }

    [Fact]
    public void RetractTest()
    {
        ISourceReader reader = new StringSourceReader("test");
        reader.MoveNext();

        Assert.Equal('t', reader.Current);
        Assert.True(reader.MoveNext());
        Assert.Equal('e', reader.Current);
        Assert.True(reader.Retract());
        Assert.Equal('t', reader.Current);
        Assert.False(reader.Retract());
    }

    [Fact]
    public void PeekTest()
    {
        ISourceReader reader = new StringSourceReader("peek");
        reader.MoveNext();

        Assert.Equal('p', reader.Current);
        Assert.True(reader.TryPeekChar(out char? c));
        Assert.Equal('e', c);
        Assert.Equal('p', reader.Current);
    }

    private static void CheckLine(ISourceReader reader, string line, uint lineNumber)
    {
        foreach ((char value, uint index) in line.WithIndex())
        {
            Assert.Equal(value, reader.Current);
            Assert.Equal(lineNumber, reader.Line);
            Assert.Equal(index + 1, reader.Pos);
            reader.MoveNext();
        }
    }
}
