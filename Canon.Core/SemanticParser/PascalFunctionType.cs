using System.Text;

namespace Canon.Core.SemanticParser;

public class PascalFunctionType(List<PascalParameterType> parameters, PascalType returnType) : PascalType
{
    public List<PascalParameterType> Parameters { get; } = parameters;

    public PascalType ReturnType { get; } = returnType;

    /// <summary>
    /// Pascal核心库函数的类型
    /// </summary>
    public static PascalFunctionType CoreFuntionType => new([], PascalBasicType.Void);

    public override string TypeName
    {
        get
        {
            StringBuilder builder = new();

            foreach (PascalParameterType parameter in Parameters)
            {
                builder.Append(parameter.TypeName).Append('_');
            }

            builder.Append(ReturnType.TypeName);
            return builder.ToString();
        }
    }

    public override bool Equals(PascalType? other)
    {
        if (other is not PascalFunctionType functionType)
        {
            return false;
        }

        if (Parameters.Count != functionType.Parameters.Count || ReturnType != functionType.ReturnType)
        {
            return false;
        }

        for (int i = 0; i < Parameters.Count; i++)
        {
            if (Parameters[i] != functionType.Parameters[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        int code = ReturnType.GetHashCode();

        foreach (PascalParameterType parameter in Parameters)
        {
            code ^= parameter.GetHashCode();
        }

        return code;
    }
}
