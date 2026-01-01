using Xunit;
using Share.Services;
using Share.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StudioMod.Tests.Services;

public class CodeAnalysisServiceTests : IDisposable
{
    private readonly string _testPath;

    public CodeAnalysisServiceTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "CodeAnalysisServiceTests_" + Guid.NewGuid());
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
    public void GetEntityFiles_ShouldReturnEntityFiles_WhenFilesAreValid()
    {
        // Arrange
        var entityPath = Path.Combine(_testPath, "Entities");
        Directory.CreateDirectory(entityPath);
        var filePath = Path.Combine(entityPath, "TestEntity.cs");
        var content = @"
using System;
using Entity;

namespace Entities;

/// <summary>
/// Test Entity
/// </summary>
public class TestEntity : EntityBase
{
    public string Name { get; set; }
}
";
        File.WriteAllText(filePath, content);
        var filePaths = new List<string> { filePath };

        // Act
        var result = CodeAnalysisService.GetEntityFiles(entityPath, filePaths);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, e => e.Name == "TestEntity.cs");
        Assert.Equal("Test Entity", result.First().Comment);
    }
}
