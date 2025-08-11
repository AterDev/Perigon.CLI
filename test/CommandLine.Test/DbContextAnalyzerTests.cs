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
    public async Task Should_parse_entityAsync()
    {
        var entityFrameworkPath =
            @"D:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\EntityFramework";
        var entityPath =
            @"D:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\Entity";
        var service = new DbContextParseHelper(entityPath, entityFrameworkPath);

        await service.LoadEntityAsync(
            @"D:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\Entity\CMSMod\Blog.cs"
        );
        var entityInfo = service.GetEntityInfo();
        Assert.NotNull(entityInfo);
        //Assert.Equal("User", entityInfo.Name);
        //Assert.Equal("Entity.CMSMod", entityInfo.NamespaceName);
    }
}
