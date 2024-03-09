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
}
