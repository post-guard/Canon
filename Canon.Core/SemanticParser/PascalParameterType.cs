namespace Canon.Core.SemanticParser;

public class PascalParameterType(PascalType parameterType, bool isVar, string parameterName) : PascalType
{
    public PascalType ParameterType { get; } = parameterType;

    public bool IsVar { get; } = isVar;

    public string ParameterName { get; } = parameterName;

    public override string TypeName
    {
        get
        {
            if (IsVar)
            {
                return $"var_{ParameterType.TypeName}";
            }
            else
            {
                return ParameterType.TypeName;
            }
        }
    }

    public override bool Equals(PascalType? other)
    {
        if (other is not PascalParameterType parameterType)
        {
            return false;
        }

        return ParameterType == parameterType.ParameterType && IsVar == parameterType.IsVar;
    }

    public override int GetHashCode()
    {
        return ParameterType.GetHashCode() ^ IsVar.GetHashCode();
    }
}
