using Canon.Core.GrammarParser;

namespace Canon.Tests;

public static class Utils
{
    public static LinkedList<char> GetLinkedList(string content)
    {
        LinkedList<char> list = [];

        foreach(char c in content)
        {
            list.AddLast(c);
        }

        return list;
    }

    /// <summary>
    /// 验证两棵语法树一致
    /// </summary>
    /// <param name="a">一棵语法树</param>
    /// <param name="b">另一棵语法树</param>
    public static void CheckSyntaxRoot(SyntaxNode a, SyntaxNode b)
    {
        int length = a.Count();
        Assert.Equal(length, b.Count());

        using IEnumerator<SyntaxNode> aIter = a.GetEnumerator(), bIter = b.GetEnumerator();

        for (int i = 0; i < length; i++)
        {
            Assert.True(aIter.MoveNext());
            Assert.True(bIter.MoveNext());

            Assert.Equal(aIter.Current, bIter.Current);
        }
    }
}
