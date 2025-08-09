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

        var analyzer = new ExternalDbContextAnalyzer(entityFrameworkPath);
        var models = analyzer.GetDbContextModels();
        Assert.NotNull(models);
        Assert.IsType<Dictionary<string, IModel>>(models);
    }
}
