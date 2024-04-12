using System.Text;

namespace Canon.Core.CodeGenerators;

/// <summary>
/// 构建C语言代码
/// </summary>
public class CCodeBuilder
{
    private readonly StringBuilder _builder = new();

    public void AddString(string code)
    {
        _builder.Append(code);
    }


    public string Build()
    {
        return _builder.ToString();
    }
}
