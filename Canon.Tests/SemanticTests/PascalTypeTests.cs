using Canon.Core.SemanticParser;

namespace Canon.Tests.SemanticTests;

public class PascalTypeTests
{
    [Fact]
    public void PascalBasicTypeTests()
    {
        PascalType integer = PascalBasicType.Integer;
        PascalType boolean = PascalBasicType.Boolean;
        PascalType character = PascalBasicType.Character;
        PascalType real = PascalBasicType.Real;
        PascalType voidType = PascalBasicType.Void;

        Assert.Equal(integer, PascalBasicType.Integer);
        Assert.Equal(boolean, PascalBasicType.Boolean);

        Assert.NotEqual(integer, character);
        Assert.NotEqual(boolean, real);
        Assert.NotEqual(character, voidType);
    }

    [Fact]
    public void PascalArrayTypeTests()
    {
        PascalType array1 = new PascalArrayType(PascalBasicType.Integer, 0, 10);
        PascalType array2 = new PascalArrayType(PascalBasicType.Integer, 0, 10);

        Assert.Equal(array1, array2);

        PascalType array3 = new PascalArrayType(PascalBasicType.Integer, -9, -3);
        Assert.NotEqual(array1, array3);
    }

    [Fact]
    public void PascalFunctionTypeTests()
    {
        PascalType function1 = new PascalFunctionType([new PascalParameterType(PascalBasicType.Integer, "a")],
            PascalBasicType.Void);
        PascalType function2 = new PascalFunctionType([new PascalParameterType(PascalBasicType.Integer, "a")],
            PascalBasicType.Void);

        Assert.Equal(function1, function2);

        PascalType function3 = new PascalFunctionType([new PascalParameterType(PascalBasicType.Real, "a")],
            PascalBasicType.Integer);
        Assert.NotEqual(function1, function3);
    }
}
