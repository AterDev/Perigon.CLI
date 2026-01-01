using Moq;
using Xunit;
using StudioMod.Managers;
using CodeGenerator;
using Microsoft.Extensions.Logging;
using Share.Services;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Collections.Generic;
using Share.Models;
using Share;

namespace StudioMod.Tests.Managers;

public class EntityInfoManagerTests : IDisposable
{
    private readonly Mock<ILogger<EntityInfoManager>> _mockLogger;
    private readonly Mock<IProjectContext> _mockProjectContext;
    private readonly EntityInfoManager _manager;
    private readonly string _testPath;

    public EntityInfoManagerTests()
    {
        _mockLogger = new Mock<ILogger<EntityInfoManager>>();
        _mockProjectContext = new Mock<IProjectContext>();
        
        var mockCodeGenLogger = new Mock<ILogger<CodeGenService>>();
        var mockCache = new Mock<IMemoryCache>();
        var cacheEntry = new Mock<ICacheEntry>();
        mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry.Object);

        var cacheService = new CacheService(mockCache.Object);
        var codeGenService = new CodeGenService(mockCodeGenLogger.Object, _mockProjectContext.Object, cacheService);

        _manager = new EntityInfoManager(_mockLogger.Object, codeGenService, _mockProjectContext.Object);
        
        _testPath = Path.Combine(Path.GetTempPath(), "EntityInfoManagerTests_" + Guid.NewGuid());
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
    public void GetEntityFiles_ShouldReturnFiles_WhenFilesExist()
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
        
        _mockProjectContext.Setup(p => p.EntityPath).Returns(entityPath);

        // Act
        var result = _manager.GetEntityFiles(entityPath);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, e => e.Name == "TestEntity.cs");
    }
}
