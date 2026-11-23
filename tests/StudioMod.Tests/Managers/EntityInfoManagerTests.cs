using CoreMod.Managers;
using CoreMod.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Share;
using Share.Models;
using Share.Services;
using Xunit;

namespace CoreMod.Tests.Managers;

public class EntityInfoManagerTests : IDisposable
{
    private readonly Mock<ILogger<EntityInfoManager>> _mockLogger;
    private readonly SolutionContext _projectContext;
    private readonly EntityInfoManager _manager;
    private readonly string _testPath;

    public EntityInfoManagerTests()
    {
        _mockLogger = new Mock<ILogger<EntityInfoManager>>();
        _projectContext = new SolutionContext();

        var mockCodeGenLogger = new Mock<ILogger<CodeGenService>>();
        var mockCache = new Mock<IMemoryCache>();
        var cacheEntry = new Mock<ICacheEntry>();
        mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntry.Object);

        var cacheService = new CacheService(mockCache.Object);
        var codeGenService = new CodeGenService(mockCodeGenLogger.Object, _projectContext, cacheService);

        _manager = new EntityInfoManager(_mockLogger.Object, codeGenService, _projectContext);

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

        _projectContext.EntityPath = entityPath;

        // Act
        var result = _manager.GetEntityFiles(entityPath);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, e => e.Name == "TestEntity.cs");
    }

    [Theory]
    [InlineData(CommandType.Dto)]
    [InlineData(CommandType.Manager)]
    [InlineData(CommandType.API)]
    public async Task GenerateAsync_ShouldRejectCodeGeneration_ForAotProjects(
        CommandType commandType
    )
    {
        // Arrange
        var configPath = Path.Combine(_testPath, ".config");
        Directory.CreateDirectory(configPath);
        await File.WriteAllTextAsync(
            Path.Combine(configPath, "perigon.config.toml"),
            "isAOT = true",
            TestContext.Current.CancellationToken
        );
        _projectContext.SolutionPath = _testPath;

        var dto = new GenerateDto
        {
            EntityPath = Path.Combine(_testPath, "TestEntity.cs"),
            CommandType = commandType,
        };

        // Act
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _manager.GenerateAsync(dto)
        );

        // Assert
        Assert.Equal(EntityInfoManager.AotCodeGenerationNotSupported, exception.Message);
    }
}
