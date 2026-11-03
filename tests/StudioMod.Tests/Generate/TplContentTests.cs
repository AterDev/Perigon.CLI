using CodeGenerator;
using Xunit;

namespace CoreMod.Tests.Generate;

public class TplContentTests
{
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
