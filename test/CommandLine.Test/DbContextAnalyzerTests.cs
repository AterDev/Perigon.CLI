using System.Collections.Generic;
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
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\EntityFramework";

        var analyzer = new DbContextAnalyzer(entityFrameworkPath);
        var models = analyzer.GetDbContextModels();
        Assert.NotNull(models);
        Assert.IsType<Dictionary<string, IModel>>(models);
    }

    [Fact]
    public void Shoud_parse_entity()
    {
        var entityFrameworkPath =
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\EntityFramework";
        var entityPath =
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\Entity";
        var service = new DbContextParseHelper(entityPath, entityFrameworkPath);

        service.LoadEntity(
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\Entity\CMSMod\Blog.cs"
        );
        var entityInfo = service.GetEntityInfo();
        Assert.NotNull(entityInfo);
        Assert.Equal("Blog", entityInfo.Name);
        Assert.Equal("Entity.CMSMod", entityInfo.NamespaceName);
    }
}
