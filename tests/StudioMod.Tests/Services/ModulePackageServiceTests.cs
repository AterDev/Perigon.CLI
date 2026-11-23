using CoreMod.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Share;
using Share.Models;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace CoreMod.Tests.Services;

public class ModulePackageServiceTests
{
    [Theory]
    [InlineData(false, "Controllers")]
    [InlineData(true, "Endpoints")]
    public async Task PackageModuleAsync_ShouldCreatePackage_ForSimpleAttributes(bool isAot, string apiDirectory)
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            [Description("包含内容管理相关功能")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }

                public static WebApplication UseCMSModServices(this WebApplication app)
                {
                    return app;
                }
            }
            """,
            isAot: isAot
        );

        Assert.Contains($"{apiDirectory}/CMSMod/Article.cs", result.EntryNames);
        Assert.NotNull(result.Metadata);
        Assert.Equal("CMSMod", result.Metadata!.ModuleName);
        Assert.Equal("Perigon", result.Metadata.Author);
        Assert.Equal("CMSMod", result.Metadata.DisplayName);
        Assert.Equal("包含内容管理相关功能", result.Metadata.Description);
        Assert.Equal(PackageMetadata.DefaultVersion, result.Metadata.Version);
        Assert.DoesNotContain("UseSelfServices", JsonSerializer.Serialize(result.Metadata));
        Assert.Null(result.Metadata.Frontend);
    }

    [Fact]
    public async Task PackageModuleAsync_ShouldUseExplicitVersion()
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }
            }
            """,
            version: "1.1.0"
        );

        Assert.NotNull(result.Metadata);
        Assert.Equal("1.1.0", result.Metadata!.Version);
    }

    [Fact]
    public async Task PackageModuleAsync_ShouldCreatePackage_ForQualifiedAttributes()
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [global::System.ComponentModel.DisplayNameAttribute("Perigon::CMSMod")]
            [global::System.ComponentModel.DescriptionAttribute("包含内容管理相关功能")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }

                public static WebApplication UseCMSModServices(this WebApplication app)
                {
                    return app;
                }
            }
            """
        );

        Assert.NotNull(result.Metadata);
        Assert.Equal("Perigon", result.Metadata!.Author);
        Assert.Equal("CMSMod", result.Metadata.DisplayName);
        Assert.Equal("包含内容管理相关功能", result.Metadata.Description);
    }

    [Fact]
    public async Task PackageModuleAsync_ShouldIncludeAngularFrontend_WhenFrontPathIsSpecified()
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }
            }
            """,
            frontendPackageJson: """{"dependencies":{"@angular/core":"20.0.0"}}"""
        );

        Assert.NotNull(result.Metadata);
        Assert.Equal("Angular", result.Metadata!.Frontend);
        Assert.Contains("Frontend/cms/cms.component.ts", result.EntryNames);
        Assert.Contains("Frontend/share/shared.component.ts", result.EntryNames);
    }

    [Theory]
    [InlineData("""{"dependencies":{"react":"19.0.0"}}""", "React")]
    [InlineData("""{"devDependencies":{"vue":"3.0.0"}}""", "Vue")]
    [InlineData("""{"dependencies":{"svelte":"5.0.0"}}""", "Else")]
    public async Task PackageModuleAsync_ShouldDetectFrontendType(string packageJson, string expectedFrontend)
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }
            }
            """,
            frontendPackageJson: packageJson
        );

        Assert.Equal(expectedFrontend, result.Metadata!.Frontend);
    }

    [Theory]
    [InlineData(false, "Controllers")]
    [InlineData(true, "Endpoints")]
    public async Task ExtractPackageAsync_ShouldRestoreApiAndFrontend(bool isAot, string apiDirectory)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var modulesPath = Path.Combine(root, PathConst.ModulesPath);
        var entityPath = Path.Combine(root, PathConst.EntityPath);
        var servicesPath = Path.Combine(root, PathConst.ServicesPath);
        var frontendProjectPath = Path.Combine(root, "frontend");
        var frontendModulesPath = Path.Combine(frontendProjectPath, PathConst.FrontendModulesPath);
        var frontendPath = Path.Combine(frontendModulesPath, "cms");
        var sharePath = Path.Combine(frontendModulesPath, "share");
        var packagePath = Path.Combine(root, "CMSMod.zip");

        try
        {
            Directory.CreateDirectory(Path.Combine(servicesPath, "AdminService"));
            Directory.CreateDirectory(sharePath);
            await File.WriteAllTextAsync(
                Path.Combine(sharePath, "shared.component.ts"),
                "existing",
                TestContext.Current.CancellationToken
            );

            using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
            {
                await WriteArchiveEntryAsync(
                    archive,
                    "metadata.json",
                    JsonSerializer.Serialize(new PackageMetadata { ModuleName = "CMSMod", Frontend = "Angular" })
                );
                await WriteArchiveEntryAsync(archive, $"{apiDirectory}/CMSMod/Article.cs", "api");
                await WriteArchiveEntryAsync(archive, "Frontend/cms/cms.component.ts", "module");
                await WriteArchiveEntryAsync(archive, "Frontend/share/shared.component.ts", "package");
                await WriteArchiveEntryAsync(archive, "Frontend/share/new.component.ts", "new");
            }

            var context = new SolutionContext()
            {
                SolutionPath = root,
                ModulesPath = modulesPath,
                EntityPath = entityPath,
                ServicesPath = servicesPath
            };
            var service = new ModuleInstallService(
                context,
                CreateLocalizer(),
                NullLogger<ModuleInstallService>.Instance,
                null!,
                null!
            );
            var extractMethod = typeof(ModuleInstallService).GetMethod(
                "ExtractPackageAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.NotNull(extractMethod);

            var task = (Task<PackageMetadata?>)extractMethod!.Invoke(
                service,
                [packagePath, "AdminService", "frontend", isAot]
            )!;
            var metadata = await task;

            Assert.NotNull(metadata);
            Assert.True(File.Exists(Path.Combine(servicesPath, "AdminService", apiDirectory, "CMSMod", "Article.cs")));
            Assert.Equal("module", await File.ReadAllTextAsync(
                Path.Combine(frontendPath, "cms.component.ts"),
                TestContext.Current.CancellationToken
            ));
            Assert.Equal("existing", await File.ReadAllTextAsync(
                Path.Combine(sharePath, "shared.component.ts"),
                TestContext.Current.CancellationToken
            ));
            Assert.Equal("new", await File.ReadAllTextAsync(
                Path.Combine(sharePath, "new.component.ts"),
                TestContext.Current.CancellationToken
            ));
            Assert.False(Directory.Exists(Path.Combine(frontendProjectPath, "cms")));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public async Task ExtractPackageAsync_ShouldRejectFrontendProjectWithoutAppModulesDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var modulesPath = Path.Combine(root, PathConst.ModulesPath);
        var entityPath = Path.Combine(root, PathConst.EntityPath);
        var servicesPath = Path.Combine(root, PathConst.ServicesPath);
        var frontendProjectPath = Path.Combine(root, "frontend");
        var packagePath = Path.Combine(root, "CMSMod.zip");

        try
        {
            Directory.CreateDirectory(Path.Combine(servicesPath, "AdminService"));
            Directory.CreateDirectory(frontendProjectPath);

            using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
            {
                await WriteArchiveEntryAsync(
                    archive,
                    "metadata.json",
                    JsonSerializer.Serialize(new PackageMetadata { ModuleName = "CMSMod", Frontend = "Angular" })
                );
                await WriteArchiveEntryAsync(archive, "Frontend/cms/cms.component.ts", "module");
            }

            var context = new SolutionContext()
            {
                SolutionPath = root,
                ModulesPath = modulesPath,
                EntityPath = entityPath,
                ServicesPath = servicesPath
            };
            var service = new ModuleInstallService(
                context,
                CreateLocalizer(),
                NullLogger<ModuleInstallService>.Instance,
                null!,
                null!
            );
            var extractMethod = typeof(ModuleInstallService).GetMethod(
                "ExtractPackageAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.NotNull(extractMethod);

            var task = (Task<PackageMetadata?>)extractMethod!.Invoke(
                service,
                [packagePath, "AdminService", "frontend", false]
            )!;
            var metadata = await task;

            Assert.Null(metadata);
            Assert.False(Directory.Exists(Path.Combine(frontendProjectPath, "cms")));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static async Task<PackageResult> RunPackageAsync(
        string moduleExtensionsContent,
        string? frontendPackageJson = null,
        string? version = null,
        bool isAot = false
    )
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var modulesPath = Path.Combine(root, PathConst.ModulesPath);
        var entityPath = Path.Combine(root, PathConst.EntityPath);
        var servicesPath = Path.Combine(root, PathConst.ServicesPath);

        Directory.CreateDirectory(Path.Combine(modulesPath, "CMSMod"));
        Directory.CreateDirectory(Path.Combine(entityPath, "CMSMod"));
        Directory.CreateDirectory(Path.Combine(servicesPath, "AdminService"));

        await File.WriteAllTextAsync(
            Path.Combine(modulesPath, "CMSMod", ConstVal.ModuleExtensionFile),
            moduleExtensionsContent,
            TestContext.Current.CancellationToken
        );

        if (isAot)
        {
            var configPath = Path.Combine(root, ".config");
            Directory.CreateDirectory(configPath);
            await File.WriteAllTextAsync(Path.Combine(configPath, "perigon.config.toml"), "isAOT = true", TestContext.Current.CancellationToken);
        }

        foreach (var apiDirectory in new[] { "Controllers", "Endpoints" })
        {
            var apiPath = Path.Combine(servicesPath, "AdminService", apiDirectory, "CMSMod");
            Directory.CreateDirectory(apiPath);
            await File.WriteAllTextAsync(Path.Combine(apiPath, "Article.cs"), "public class Article { }", TestContext.Current.CancellationToken);
        }

        string? frontendPath = null;
        if (frontendPackageJson != null)
        {
            frontendPath = Path.Combine(root, "frontend", "features", "cms");
            Directory.CreateDirectory(frontendPath);
            await File.WriteAllTextAsync(
                Path.Combine(frontendPath, "cms.component.ts"),
                "export class CmsComponent {}",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(root, "frontend", ConstVal.NodeProjectFile),
                frontendPackageJson,
                TestContext.Current.CancellationToken
            );
            var frontendSharePath = Path.Combine(root, "frontend", "features", "share");
            Directory.CreateDirectory(frontendSharePath);
            await File.WriteAllTextAsync(
                Path.Combine(frontendSharePath, "shared.component.ts"),
                "export class SharedComponent {}",
                TestContext.Current.CancellationToken
            );
        }

        await File.WriteAllTextAsync(
            Path.Combine(entityPath, "CMSMod", "ContentEntity.cs"),
            "public class ContentEntity { }",
            TestContext.Current.CancellationToken
        );

        var solutionContext = new SolutionContext()
        {
            SolutionPath = root,
            ModulesPath = modulesPath,
            EntityPath = entityPath,
            ServicesPath = servicesPath
        };

        var service = new ModulePackageService(
            solutionContext,
            CreateLocalizer(),
            NullLogger<ModulePackageService>.Instance
        );

        try
        {
            var packagePath = await service.PackageModuleAsync(
                "CMSMod",
                "AdminService",
                frontendPath,
                version
            );
            Assert.NotNull(packagePath);
            Assert.True(File.Exists(packagePath));

            using var archive = ZipFile.OpenRead(packagePath!);
            var metadataEntry = archive.GetEntry("metadata.json");
            Assert.NotNull(metadataEntry);

            using var reader = new StreamReader(metadataEntry!.Open());
            var metadataJson = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            var metadata = JsonSerializer.Deserialize<PackageMetadata>(
                metadataJson,
                ConstVal.DefaultJsonSerializerOptions
            );
            var entryNames = archive.Entries.Select(entry => entry.FullName).ToList();
            return new PackageResult(metadata, entryNames);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static Localizer CreateLocalizer()
    {
        var localizerMock = new Mock<IStringLocalizer<Localizer>>();
        localizerMock
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        localizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        return new Localizer(localizerMock.Object);
    }

    private static async Task WriteArchiveEntryAsync(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content.AsMemory(), TestContext.Current.CancellationToken);
    }

    private sealed record PackageResult(PackageMetadata? Metadata, List<string> EntryNames);
}
