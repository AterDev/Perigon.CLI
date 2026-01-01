using Xunit;
using CodeGenerator.Helper;
using System;

namespace StudioMod.Tests.Helper;

public class CSharpAnalysisHelperTests
{
    [Theory]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(bool), "bool")]
    [InlineData(typeof(double), "double")]
    [InlineData(typeof(decimal), "decimal")]
    [InlineData(typeof(long), "long")]
    [InlineData(typeof(short), "short")]
    [InlineData(typeof(uint), "uint")]
    [InlineData(typeof(ulong), "ulong")]
    [InlineData(typeof(ushort), "ushort")]
    [InlineData(typeof(float), "float")]
    [InlineData(typeof(byte), "byte")]
    [InlineData(typeof(sbyte), "sbyte")]
    [InlineData(typeof(char), "char")]
    [InlineData(typeof(object), "object")]
    [InlineData(typeof(DateTime), "DateTime")]
    [InlineData(typeof(Guid), "Guid")]
    public void ToTypeName_ShouldConvertBuiltInTypes(Type type, string expected)
    {
        // Act
        var result = CSharpAnalysisHelper.ToTypeName(type);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToTypeName_ShouldConvertTypes()
    {
        // Arrange
        var listType = typeof(List<string>);

        // Act
        var result = CSharpAnalysisHelper.ToTypeName(listType);

        // Assert
        Assert.Contains("List", result);
    }

    [Fact]
    public void ToTypeName_ShouldHandleCustomTypes()
    {
        // Arrange
        var customType = typeof(CustomTestClass);

        // Act
        var result = CSharpAnalysisHelper.ToTypeName(customType);

        // Assert
        Assert.Contains("CustomTestClass", result);
    }
}

public class CustomTestClass
{
    public string? Name { get; set; }
}
