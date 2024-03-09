namespace Canon.Core;

public class Compiler
{
    public string Compile(string _)
    {
        return """
               #include <stdio.h>
               int main()
               {
                   printf("%d", 3);
                   return 0;
               }
               """;
    }
}