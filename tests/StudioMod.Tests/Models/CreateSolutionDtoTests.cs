using Share.Models.CommandDtos;
using Xunit;

namespace StudioMod.Tests.Models;

public class CreateSolutionDtoTests
{
    [Fact]
    public void ApplyTemplateDefaults_ShouldForcePostgresAndClearModulesForLightTemplate()
    {
        var dto = new CreateSolutionDto
        {
            Name = "demo",
            Path = "./demo",
            IsLight = true,
            DBType = DBType.SQLServer,
            OfficialModules = ["Perigon.SystemMod"],
        };

        dto.ApplyTemplateDefaults();

        Assert.Equal(DBType.PostgreSQL, dto.DBType);
        Assert.Empty(dto.OfficialModules);
    }

    [Fact]
    public void GetAvailableDatabaseTypes_ShouldReturnOnlyPostgresForLightTemplate()
    {
        var dto = new CreateSolutionDto
        {
            Name = "demo",
            Path = "./demo",
            IsLight = true,
        };

        var availableTypes = dto.GetAvailableDatabaseTypes();

        Assert.Equal([DBType.PostgreSQL], availableTypes);
    }
}
