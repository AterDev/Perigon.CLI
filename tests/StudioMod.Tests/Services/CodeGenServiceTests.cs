using CodeGenerator;
using CodeGenerator.Models;
using CoreMod.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Share;
using Share.Models;
using Share.Services;
using Xunit;

namespace CoreMod.Tests.Services;

public class CodeGenServiceTests
{
    [Fact]
    public async Task GenerateWebRequestAsync_ShouldPreserveBaseServiceByDefault_AndReplaceItWhenRequested()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"CodeGenServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputPath);

        try
        {
            var fixturePath = Path.Combine(
                AppContext.BaseDirectory,
                "Generate",
                "Fixtures",
                "request-client-special.openapi.json"
            );
            var baseServicePath = Path.Combine(outputPath, "services", "demo", "base.service.ts");
            Directory.CreateDirectory(Path.GetDirectoryName(baseServicePath)!);
            const string customContent = "// custom base service";
            await File.WriteAllTextAsync(baseServicePath, customContent, TestContext.Current.CancellationToken);

            var projectContext = new SolutionContext();
            var cache = new CacheService(new MemoryCache(new MemoryCacheOptions()));
            var codeGenService = new CodeGenService(
                NullLogger<CodeGenService>.Instance,
                projectContext,
                cache
            );

            var files = await codeGenService.GenerateWebRequestAsync(
                fixturePath,
                outputPath,
                RequestClientType.Axios
            );
            codeGenService.GenerateFiles(files, false);

            Assert.Equal(
                customContent,
                await File.ReadAllTextAsync(baseServicePath, TestContext.Current.CancellationToken)
            );

            files = await codeGenService.GenerateWebRequestAsync(
                fixturePath,
                outputPath,
                RequestClientType.Axios,
                coverBaseService: true
            );
            codeGenService.GenerateFiles(files, false);

            var replacedContent = await File.ReadAllTextAsync(
                baseServicePath,
                TestContext.Current.CancellationToken
            );
            Assert.NotEqual(customContent, replacedContent);
            Assert.Contains("VITE_APP_SERVER_URL", replacedContent);
        }
        finally
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    [Fact]
    public async Task GenerateControllerAsync_ShouldUseDefaultConfig_WhenSolutionConfigIsMissing()
    {
        var servicePath = Path.Combine(Path.GetTempPath(), $"CodeGenServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(servicePath);

        try
        {
            var serviceName = Path.GetFileName(servicePath);
            await File.WriteAllTextAsync(
                Path.Combine(servicePath, $"{serviceName}.csproj"),
                "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>",
                TestContext.Current.CancellationToken
            );

            var projectContext = new SolutionContext();
            var cache = new CacheService(new MemoryCache(new MemoryCacheOptions()));
            var codeGenService = new CodeGenService(
                NullLogger<CodeGenService>.Instance,
                projectContext,
                cache
            );
            var entityInfo = new EntityInfo
            {
                Name = "Product",
                NamespaceName = "Store.Domain.Entities",
                ModuleName = "StoreMod",
                FilePath = "Product.cs",
            };

            var files = await codeGenService.GenerateControllerAsync(
                entityInfo,
                servicePath
            );

            Assert.NotNull(projectContext.SolutionConfig);
            Assert.Equal("SystemMod", projectContext.SolutionConfig.SystemModName);
            Assert.Contains(files, file => file.Name == "ProductController.cs");
            Assert.Contains(files, file => file.Name == ConstVal.GlobalUsingsFile);
        }
        finally
        {
            if (Directory.Exists(servicePath))
            {
                Directory.Delete(servicePath, true);
            }
        }
    }
}
