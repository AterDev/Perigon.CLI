using Xunit;
using Core.Utils;
using System;

namespace StudioMod.Tests.Helper;

public class CSharpCovertHelperTests
{
    [Theory]
    [InlineData("FirstName", "firstName")]
    [InlineData("LastName", "lastName")]
    [InlineData("FullName", "fullName")]
    [InlineData("id", "id")]
    [InlineData("TestProp", "testProp")]
    public void ToCamelCase_ShouldConvertToCamelCase(string input, string expected)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("firstName", "FirstName")]
    [InlineData("lastName", "LastName")]
    [InlineData("fullName", "FullName")]
    [InlineData("id", "Id")]
    [InlineData("testProp", "TestProp")]
    public void ToPascalCase_ShouldConvertToPascalCase(string input, string expected)
    {
        // Act
        var result = input.ToPascalCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("FirstName", "first_name")]
    [InlineData("LastName", "last_name")]
    [InlineData("ID", "id")]
    [InlineData("TestProp", "test_prop")]
    public void ToSnakeLower_ShouldConvertToSnakeCase(string input, string expected)
    {
        // Act
        var result = input.ToSnakeLower();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("FirstName", "first-name")]
    [InlineData("LastName", "last-name")]
    [InlineData("ID", "id")]
    [InlineData("TestProp", "test-prop")]
    public void ToHyphen_ShouldConvertToHyphenCase(string input, string expected)
    {
        // Act
        var result = input.ToHyphen();

        // Assert
        Assert.Equal(expected, result);
    }
}
