using Xunit;
using CodeGenerator.Helper;
using System;
using System.IO;
using System.Linq;

namespace StudioMod.Tests.Helper;

public class CompilationHelperTests : IDisposable
{
    private readonly string _testPath;

    public CompilationHelperTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "CompilationHelperTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void GetAllEnumClasses_ShouldReturnList()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public class RegularClass
{
    public string Name { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var enums = helper.GetAllEnumClasses();

        // Assert
        Assert.IsType<List<string>>(enums);
    }

    [Fact]
    public void GetAllEnumClasses_ShouldCacheResult()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public class TestEntity
{
    public string Name { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var enums1 = helper.GetAllEnumClasses();
        var enums2 = helper.GetAllEnumClasses();

        // Assert - Should return same list instance due to caching
        Assert.Same(enums1, enums2);
    }

    [Fact]
    public void IsEntityClass_ShouldReturnTrueForNormalClass()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public class TestEntity
{
    public string Name { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var result = helper.IsEntityClass();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEntityClass_ShouldReturnFalseForStaticClass()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public static class Helper
{
    public static void DoSomething() { }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var result = helper.IsEntityClass();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEntityClass_ShouldReturnFalseForAbstractClass()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public abstract class BaseEntity
{
    public string Name { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var result = helper.IsEntityClass();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNamespace_ShouldReturnCorrectNamespace()
    {
        // Arrange
        var content = @"
namespace MyProject.Domain;

public class TestEntity
{
    public string Name { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var ns = helper.GetNamespace();

        // Assert
        Assert.Equal("MyProject.Domain", ns);
    }

    [Fact]
    public void PropertyExist_ShouldReturnTrueWhenPropertyExists()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public class TestEntity
{
    public string Name { get; set; }
    public int Age { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var result = helper.PropertyExist("Name");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PropertyExist_ShouldReturnFalseWhenPropertyNotExists()
    {
        // Arrange
        var content = @"
namespace TestNamespace;

public class TestEntity
{
    public string Name { get; set; }
}
";
        var helper = new CompilationHelper(_testPath);
        helper.LoadContent(content);

        // Act
        var result = helper.PropertyExist("NonExistent");

        // Assert
        Assert.False(result);
    }
}
