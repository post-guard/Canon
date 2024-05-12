namespace Canon.Core.SemanticParser;

public class PascalParameterType(PascalType parameterType, string parameterName) : PascalType
{
    public PascalType ParameterType { get; } = parameterType;

    public string ParameterName { get; } = parameterName;

    public override string TypeName => $"{ParameterType}_{ParameterName}";

    public override PascalType ToReferenceType()
    {
        throw new InvalidOperationException("The parameter type can not be reference.");
    }

    public override bool Equals(PascalType? other)
    {
        if (other is not PascalParameterType parameterType)
        {
            return false;
        }

        return ParameterType == parameterType.ParameterType && base.Equals(parameterType);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ ParameterType.GetHashCode();
    }
}
