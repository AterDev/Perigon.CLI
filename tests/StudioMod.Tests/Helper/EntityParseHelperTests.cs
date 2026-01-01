using Xunit;
using CodeGenerator.Helper;
using System.IO;
using System.Linq;
using System;

namespace StudioMod.Tests.Helper;

public class EntityParseHelperTests : IDisposable
{
    private readonly string _testPath;
    private readonly string _projectPath;

    public EntityParseHelperTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "EntityParseHelperTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testPath);
        
        // Create a dummy project file
        _projectPath = Path.Combine(_testPath, "TestProject.csproj");
        File.WriteAllText(_projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void Parse_ShouldParseBasicProperties()
    {
        // Arrange
        var filePath = Path.Combine(_testPath, "TestEntity.cs");
        var content = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace TestNamespace;

/// <summary>
/// Test Summary
/// </summary>
public class TestEntity
{
    /// <summary>
    /// Name Property
    /// </summary>
    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    public int Age { get; set; }

    public int? OptionalAge { get; set; }
}
";
        File.WriteAllText(filePath, content);

        // Act
        var helper = new EntityParseHelper(filePath);
        helper.Parse();

        // Assert
        Assert.Equal("TestEntity", helper.Name);
        Assert.Equal("TestNamespace", helper.NamespaceName);
        Assert.Contains("Test Summary", helper.Comment);
        
        Assert.NotEmpty(helper.PropertiesInfo);
        var nameProp = helper.PropertiesInfo.FirstOrDefault(p => p.Name == "Name");
        Assert.NotNull(nameProp);
        Assert.Equal("string", nameProp.Type);
        Assert.True(nameProp.IsRequired);
        Assert.Equal(50, nameProp.MaxLength);
        Assert.Equal("Name Property", nameProp.CommentSummary);

        var ageProp = helper.PropertiesInfo.FirstOrDefault(p => p.Name == "Age");
        Assert.NotNull(ageProp);
        Assert.Equal("int", ageProp.Type);
        Assert.False(nameProp.IsNullable);

        var optAgeProp = helper.PropertiesInfo.FirstOrDefault(p => p.Name == "OptionalAge");
        Assert.NotNull(optAgeProp);
        Assert.True(optAgeProp.IsNullable);
    }
}
