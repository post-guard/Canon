namespace Canon.Core.SemanticParser;

public class PascalArrayType(PascalType elementType, int begin, int end) : PascalType
{
    public PascalType ElementType { get; } = elementType;

    public int Begin { get; } = begin;

    public int End { get; } = end;

    public override string TypeName => $"{ElementType.TypeName}_{Begin}_{End}";

    public override PascalType ToReferenceType()
    {
        throw new InvalidOperationException("Array type can not be reference.");
    }

    public override bool Equals(PascalType? other)
    {
        if (other is not PascalArrayType pascalArrayType)
        {
            return false;
        }

        if (ElementType != pascalArrayType.ElementType)
        {
            return false;
        }

        if (Begin != pascalArrayType.Begin || End != pascalArrayType.End)
        {
            return false;
        }

        return base.Equals(pascalArrayType);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode()
               ^ ElementType.GetHashCode()
               ^ Begin.GetHashCode()
               ^ End.GetHashCode();
    }
}
