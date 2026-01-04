using CodeGenerator.Helper;
using Entity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

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

    [Fact]
    public async Task LoadEntityDbContext_ShouldAssignDbContextInfo_WhenDbSetExists()
    {
        // Arrange entity file
        var entityFilePath = Path.Combine(_testPath, "TestEntity.cs");
        File.WriteAllText(entityFilePath, "namespace TestNamespace; public class TestEntity {}");

        var helper = new EntityParseHelper(entityFilePath);
        var entityInfo = await helper.ParseEntityAsync();
        Assert.NotNull(entityInfo);

        // Arrange EntityFramework project with compiled DbContext containing DbSet<TestEntity>
        var efProjectPath = Path.Combine(_testPath, "EntityFramework");
        Directory.CreateDirectory(efProjectPath);
        File.WriteAllText(
            Path.Combine(efProjectPath, "EntityFramework.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>"
        );

        var efBinDir = Path.Combine(efProjectPath, "bin", "Debug");
        Directory.CreateDirectory(efBinDir);
        var efDllPath = Path.Combine(efBinDir, "EntityFramework.dll");

        var dbContextSource = @"
namespace EntityFramework.AppDbContext;

public class DbSet<T> { }
public abstract class DbContext { }
public abstract class ContextBase : DbContext { }
public class SampleDbContext : ContextBase
{
    public DbSet<TestEntity> TestEntities { get; set; } = new();
}
public class TestEntity { }
";
        var syntaxTree = CSharpSyntaxTree.ParseText(dbContextSource);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };
        var compilation = CSharpCompilation.Create(
            "EntityFramework",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var emitResult = compilation.Emit(efDllPath);
        Assert.True(emitResult.Success, string.Join(";", emitResult.Diagnostics));

        // Act
        helper.LoadEntityDbContext(efProjectPath, entityInfo!);

        // Assert
        Assert.Equal("SampleDbContext", entityInfo!.DbContextName);
        Assert.Equal($"{ConstVal.EntityFrameworkName}.{ConstVal.AppDbContextName}", entityInfo.DbContextSpaceName);
    }
}
