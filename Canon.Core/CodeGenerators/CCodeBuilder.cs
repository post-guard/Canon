using System.Text;
using Canon.Core.SemanticParser;

namespace Canon.Core.CodeGenerators;

/// <summary>
/// 构建C语言代码
/// </summary>
public class CCodeBuilder
{
    private readonly StringBuilder _builder = new();

    private int _scopeCount = 0;
    private string _scopeEmpty = string.Empty;

    public void AddString(string code)
    {
        _builder.Append(code);
    }

    public void AddLine(string code)
    {
        foreach (string line in code.Split('\n'))
        {
            _builder.Append(_scopeEmpty);
            _builder.Append(line);
            _builder.Append('\n');
        }
    }

    /// <summary>
    /// 开始一段代码块
    /// </summary>
    public void BeginScope()
    {
        _builder.Append(_scopeEmpty).Append("{\n");

        _scopeCount += 1;
        string scopeEmpty = string.Empty;

        for (int i = 0; i < _scopeCount; i++)
        {
            scopeEmpty += "    ";
        }

        _scopeEmpty = scopeEmpty;
    }

    /// <summary>
    /// 结束一段代码块
    /// </summary>
    public void EndScope()
    {
        if (_scopeCount <= 0)
        {
            throw new InvalidOperationException("The scope has been closed!");
        }

        _scopeCount -= 1;

        string scopeEmpty = string.Empty;

        for (int i = 0; i < _scopeCount; i++)
        {
            scopeEmpty += "    ";
        }

        _scopeEmpty = scopeEmpty;

        _builder.Append(_scopeEmpty).Append("}\n");
    }

    public string Build()
    {
        return _builder.ToString();
    }
}
