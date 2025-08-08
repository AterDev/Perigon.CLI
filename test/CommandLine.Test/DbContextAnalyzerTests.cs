using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Share.Helper;
using Xunit;

namespace CommandLine.Test;

public class DbContextAnalyzerTests
{
    [Fact]
    public void GetDbContextModels_ShouldReturnDictionary()
    {
        var entityFrameworkPath =
            @"D:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\EntityFramework";

        // 搜索EntityFramework.dll
        var entityFrameworkDllPath = Directory
            .GetFiles(entityFrameworkPath, "EntityFramework.dll", SearchOption.AllDirectories)
            .FirstOrDefault();

        // Arrange
        var analyzer = new ExternalDbContextAnalyzer(entityFrameworkDllPath);
        // Act
        var models = analyzer.GetDbContextModels("EntityFramework.DBProvider.ContextBase");
        // Assert
        Assert.NotNull(models);
        Assert.IsType<Dictionary<string, IModel>>(models);
    }
}
