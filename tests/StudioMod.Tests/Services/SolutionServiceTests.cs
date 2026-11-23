using CoreMod.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Share;
using Xunit;

namespace CoreMod.Tests.Services;

public sealed class SolutionServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"perigon cli solution-{Guid.NewGuid():N}"
    );

    public SolutionServiceTests()
    {
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void UpdateSolutionFile_ShouldBeIdempotentForExistingProject()
    {
        var shareProjectPath = Path.Combine(_root, "src", "Definition", "Share", "Share.csproj");
        var projectPath = Path.Combine(_root, "src", "Modules", "CMSMod", "CMSMod.csproj");
        Directory.CreateDirectory(Path.GetDirectoryName(shareProjectPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(projectPath)!);
        File.WriteAllText(shareProjectPath, CreateProjectFile());
        File.WriteAllText(
            projectPath,
            CreateProjectFile("<ProjectReference Include=\"..\\..\\Definition\\Share\\Share.csproj\" />")
        );

        var solutionPath = Path.Combine(_root, "Sample.slnx");
        File.WriteAllText(solutionPath, "<Solution />");

        var service = CreateSolutionService();

        Assert.True(service.UpdateSolutionFile(projectPath));
        Assert.True(service.UpdateSolutionFile(projectPath));
        Assert.Equal(
            1,
            File.ReadAllText(solutionPath).Split("CMSMod.csproj", StringSplitOptions.None).Length - 1
        );
        Assert.DoesNotContain("Definition/Share/Share.csproj", File.ReadAllText(solutionPath));
    }

    [Fact]
    public async Task AddProjectReferenceAsync_ShouldRecognizeFullPathAndRemainIdempotent()
    {
        var serviceProjectPath = Path.Combine(_root, "src", "Services", "Admin Service", "Admin Service.csproj");
        var moduleProjectPath = Path.Combine(_root, "src", "Modules", "Share", "Share.csproj");
        Directory.CreateDirectory(Path.GetDirectoryName(serviceProjectPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(moduleProjectPath)!);
        File.WriteAllText(serviceProjectPath, CreateProjectFile());
        File.WriteAllText(moduleProjectPath, CreateProjectFile());

        Assert.True(await SolutionService.AddProjectReferenceAsync(serviceProjectPath, moduleProjectPath));
        Assert.True(await SolutionService.HasProjectReferenceAsync(serviceProjectPath, moduleProjectPath));
        Assert.True(await SolutionService.HasProjectReferenceAsync(serviceProjectPath, "Share"));
        Assert.True(await SolutionService.AddProjectReferenceAsync(serviceProjectPath, moduleProjectPath));

        var content = await File.ReadAllTextAsync(
            serviceProjectPath,
            TestContext.Current.CancellationToken
        );
        Assert.Equal(1, content.Split("ProjectReference", StringSplitOptions.None).Length - 1);
    }

    private SolutionService CreateSolutionService()
    {
        var context = new SolutionContext()
        {
            SolutionPath = _root
        };
        return new SolutionService(context, NullLogger<SolutionService>.Instance);
    }

    private static string CreateProjectFile(string? itemGroup = null)
    {
        return $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>{(itemGroup is null ? string.Empty : $"<ItemGroup>{itemGroup}</ItemGroup>")}</Project>";
    }
}
