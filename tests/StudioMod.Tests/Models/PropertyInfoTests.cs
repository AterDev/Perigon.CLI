using Xunit;
using CodeGenerator.Models;
using System;
using System.Collections.Generic;

namespace StudioMod.Tests.Models;

public class PropertyInfoTests
{
    [Fact]
    public void PropertyInfo_ShouldInitializeWithDefaults()
    {
        // Act
        var prop = new PropertyInfo { Name = "TestProp", Type = "string" };

        // Assert
        Assert.Equal("TestProp", prop.Name);
        Assert.Equal("string", prop.Type);
        Assert.True(prop.IsPublic);
        Assert.False(prop.IsRequired);
        Assert.False(prop.IsNullable);
        Assert.False(prop.IsList);
        Assert.False(prop.IsEnum);
    }

    [Fact]
    public void PropertyInfo_ShouldAllowSettingAttributes()
    {
        // Arrange
        var prop = new PropertyInfo
        {
            Name = "Email",
            Type = "string",
            IsRequired = true,
            MaxLength = 100
        };

        // Act & Assert
        Assert.Equal("Email", prop.Name);
        Assert.Equal("string", prop.Type);
        Assert.True(prop.IsRequired);
        Assert.Equal(100, prop.MaxLength);
    }

    [Fact]
    public void PropertyInfo_ShouldTrackNullability()
    {
        // Arrange & Act
        var propNullable = new PropertyInfo { Name = "Email", Type = "string?" };
        propNullable.IsNullable = true;

        var propRequired = new PropertyInfo { Name = "Age", Type = "int" };

        // Assert
        Assert.True(propNullable.IsNullable);
        Assert.False(propRequired.IsNullable);
    }
}
