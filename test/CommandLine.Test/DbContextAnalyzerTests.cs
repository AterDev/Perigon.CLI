using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async Task Shoud_parse_entityAsync()
    {
        var entityFrameworkPath =
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\EntityFramework";
        var entityPath =
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\Entity";
        var service = new DbContextParseHelper(entityPath, entityFrameworkPath);

        service.LoadEntity(
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\Entity\CMSMod\Blog.cs"
        );
        var entityInfo = await service.GetEntityInfo();
        Assert.NotNull(entityInfo);
        Assert.Equal("Blog", entityInfo.Name);
        Assert.Equal("Entity.CMSMod", entityInfo.NamespaceName);
    }
}
