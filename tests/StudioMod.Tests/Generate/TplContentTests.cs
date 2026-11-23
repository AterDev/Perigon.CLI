using CodeGenerator;
using Xunit;

namespace CoreMod.Tests.Generate;

public class TplContentTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnumPipeTpl_ShouldPreserveAngularDecorators(bool isNgModule)
    {
        var content = TplContent.EnumPipeTpl("case 'status': break;", isNgModule);

        Assert.Contains("from '@angular/core'", content);
        Assert.Contains("@Pipe(", content);
        Assert.Contains("@Injectable(", content);
        Assert.Contains("case 'status': break;", content);
        Assert.Equal(isNgModule, content.Contains("@NgModule("));
    }

    [Fact]
    public void ModuleExtension_ShouldNotGenerateUseModuleServicesMethod()
    {
        var content = TplContent.ModuleExtension("SampleMod");

        Assert.Contains("AddSampleMod", content);
        Assert.DoesNotContain("UseSampleModServices", content);
    }

    [Fact]
    public void ServiceProjectFile_ShouldUseSourceGenerationPackage()
    {
        var content = TplContent.ServiceProjectFileTpl();

        Assert.Contains(
            "<PackageReference Include=\"Perigon.AspNetCore.SourceGeneration\" PrivateAssets=\"all\" />",
            content
        );
        Assert.DoesNotContain("SourceGeneration.csproj", content);
    }
}
