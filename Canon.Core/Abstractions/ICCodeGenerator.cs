using Canon.Core.CodeGenerators;

namespace Canon.Core.Abstractions;

/// <summary>
/// 支持生成C语言代码的接口
/// </summary>
public interface ICCodeGenerator
{
    public void GenerateCCode(CCodeBuilder builder);
}
