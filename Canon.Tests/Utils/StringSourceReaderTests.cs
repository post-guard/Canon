using Canon.Core.Abstractions;

namespace Canon.Tests.Utils;

public class StringSourceReaderTests
{
    [Fact]
    public void LineFeedTest()
    {
        ISourceReader reader = new StringSourceReader("program Main;\nbegin\nend.\n");

        Assert.Equal(0u, reader.Pos);
        Assert.Equal(1u, reader.Line);

        // program
        Assert.True(reader.TryReadChar(out char? c));
        Assert.Equal('p', c);
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out c));
        Assert.Equal(' ', c);

        // main;
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));
        Assert.True(reader.TryReadChar(out char? _));

        Assert.True(reader.TryReadChar(out c));
        Assert.Equal('\n', c);

        // begin
        for (uint i = 1; i <= 5; i++)
        {
            Assert.True(reader.TryReadChar(out char? _));
            Assert.Equal(i, reader.Pos);
            Assert.Equal(2u, reader.Line);
        }

        // \n
        Assert.True(reader.TryReadChar(out c));
        Assert.Equal('\n', c);

        // end.
        foreach (char i in "end.")
        {
            Assert.True(reader.TryReadChar(out c));
            Assert.Equal(i, c);
        }
    }

    [Fact]
    public void CarriageReturnLineFeedTest()
    {
        ISourceReader reader = new StringSourceReader("program Main;\r\nbegin\r\nend.\r\n");

        // program Main;
        foreach ((char value, uint index) in
                 "program Main;".Select((value, index) => (value, (uint)index)))
        {
            Assert.True(reader.TryReadChar(out char? c));
            Assert.Equal(value, c);
            Assert.Equal(index + 1, reader.Pos);
            Assert.Equal(1u, reader.Line);
        }

        Assert.True(reader.TryReadChar(out _));
        Assert.True(reader.TryReadChar(out _));

        // begin
        foreach ((char value, uint index) in
                 "begin".Select((value, index) => (value, (uint)index)))
        {
            Assert.True(reader.TryReadChar(out char? c));
            Assert.Equal(value, c);
            Assert.Equal(index + 1, reader.Pos);
            Assert.Equal(2u, reader.Line);
        }
    }
}
