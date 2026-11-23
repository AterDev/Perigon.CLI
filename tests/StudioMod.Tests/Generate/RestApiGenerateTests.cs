using Xunit;
using CodeGenerator.Generate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Share.Models;

namespace CoreMod.Tests.Generate;

public class RestApiGenerateTests
{
    [Fact]
    public void GetGlobalUsings_ShouldIncludeRequiredNamespaces()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "Product",
            NamespaceName = "Domain.Entities",
            FilePath = "Product.cs"
        };

        var solutionConfig = new SolutionConfig();
        var dtoDict = new ReadOnlyDictionary<string, DtoInfo>(new Dictionary<string, DtoInfo>());

        var generator = new RestApiGenerate(entityInfo, solutionConfig, dtoDict);

        // Act
        var usings = generator.GetGlobalUsings();

        // Assert
        Assert.NotEmpty(usings);
        Assert.Contains(usings, u => u.Contains("Microsoft.AspNetCore.Mvc"));
        Assert.Contains(usings, u => u.Contains("Microsoft.Extensions.DependencyInjection"));
        Assert.Contains(usings, u => u.Contains("Microsoft.AspNetCore.Authorization"));
    }

    [Fact]
    public void GetGlobalUsings_ShouldIncludeEntityNamespace()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "Category",
            NamespaceName = "Store.Domain.Entities",
            FilePath = "Category.cs"
        };

        var solutionConfig = new SolutionConfig();
        var dtoDict = new ReadOnlyDictionary<string, DtoInfo>(new Dictionary<string, DtoInfo>());

        var generator = new RestApiGenerate(entityInfo, solutionConfig, dtoDict);

        // Act
        var usings = generator.GetGlobalUsings();

        // Assert
        Assert.Contains(usings, u => u.Contains("Store.Domain.Entities"));
    }

    [Fact]
    public void GetRestApiContent_ShouldGenerateControllerContent_WithDefaultConfig()
    {
        var entityInfo = new EntityInfo
        {
            Name = "Product",
            NamespaceName = "Store.Domain.Entities",
            ModuleName = "StoreMod",
            FilePath = "Product.cs",
        };
        var generator = new RestApiGenerate(
            entityInfo,
            new SolutionConfig(),
            new ReadOnlyDictionary<string, DtoInfo>(new Dictionary<string, DtoInfo>())
        );

        var content = generator.GetRestApiContent("StoreService");

        Assert.Contains("namespace StoreService", content);
        Assert.Contains("ProductController", content);
    }
}
