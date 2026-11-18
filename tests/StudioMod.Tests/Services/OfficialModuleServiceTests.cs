using CoreMod.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Share;
using System.Net;
using System.Text;
using Xunit;

namespace CoreMod.Tests.Services;

public class OfficialModuleServiceTests
{
    [Theory]
    [InlineData("Perigon.SystemMod", true, "SystemMod")]
    [InlineData("Perigon.SystemMod.zip", true, "SystemMod")]
    [InlineData("SystemMod.zip", false, "SystemMod")]
    public void OfficialPackageHelpers_ShouldRecognizeAndNormalize(
        string packageName,
        bool isOfficial,
        string expectedModuleName
    )
    {
        Assert.Equal(isOfficial, OfficialModuleService.IsOfficialPackageName(packageName));
        Assert.Equal(expectedModuleName, OfficialModuleService.NormalizeOfficialModuleName(packageName));
    }

    [Fact]
    public async Task GetOfficialModulesAsync_ShouldDeserializeAndSortModules()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(
            new StubHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsoluteUri == "https://raw.githubusercontent.com/AterDev/Perigon.Modules/main/modules.json")
                {
                    var metadataJson = """
                        [
                          {
                            "ModuleName": "SystemMod",
                            "Author": "Perigon",
                            "DisplayName": "SystemMod",
                            "Description": "System module",
                            "PackageType": 0,
                            "Version": "1.0.0"
                          },
                          {
                            "ModuleName": "CMSMod",
                            "Author": "Perigon",
                            "DisplayName": "CMSMod",
                            "Description": "CMS module",
                            "PackageType": 0,
                            "Version": "1.0.1"
                          }
                        ]
                        """;

                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(metadataJson, Encoding.UTF8, "application/json")
                        }
                    );
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            })
        );

        var modules = await service.GetOfficialModulesAsync(cancellationToken);

        Assert.Equal(2, modules.Count);
        Assert.Equal("CMSMod", modules[0].ModuleName);
        Assert.Equal("SystemMod", modules[1].ModuleName);
        Assert.Equal("1.0.1", modules[0].Version);
    }

    [Fact]
    public async Task GetOfficialModulesAsync_ShouldUseOneMinuteCache()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var requestCount = 0;
        var service = CreateService(
            new StubHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsoluteUri == "https://raw.githubusercontent.com/AterDev/Perigon.Modules/main/modules.json")
                {
                    requestCount++;
                    var metadataJson = """
                        [
                          {
                            "ModuleName": "SystemMod",
                            "Author": "Perigon",
                            "DisplayName": "SystemMod",
                            "Description": "System module",
                            "PackageType": 0,
                            "Version": "1.0.0"
                          }
                        ]
                        """;

                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(metadataJson, Encoding.UTF8, "application/json")
                        }
                    );
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            })
        );

        var first = await service.GetOfficialModulesAsync(cancellationToken);
        var second = await service.GetOfficialModulesAsync(cancellationToken);

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(1, requestCount);
    }

    [Fact]
    public async Task GetOfficialModulesAsync_ShouldPropagateHttpRequestException()
    {
        var expected = new HttpRequestException("network failure");
        var service = CreateService(
            new StubHttpMessageHandler(_ => throw expected)
        );

        var actual = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GetOfficialModulesAsync(TestContext.Current.CancellationToken)
        );

        Assert.Same(expected, actual);
    }

    [Theory]
    [InlineData(false, "main")]
    [InlineData(true, "aot")]
    public async Task DownloadOfficialModulePackageAsync_ShouldPersistZipFile(bool isAot, string branch)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var zipBytes = Encoding.UTF8.GetBytes("fake-zip-content");
        var service = CreateService(
            new StubHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsoluteUri == $"https://raw.githubusercontent.com/AterDev/Perigon.Modules/{branch}/modules.json")
                {
                    var metadataJson = """
                        [
                          {
                            "ModuleName": "SystemMod",
                            "Author": "Perigon",
                            "DisplayName": "SystemMod",
                            "Description": "System module",
                            "PackageType": 0,
                            "Version": "1.0.0"
                          }
                        ]
                        """;

                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(metadataJson, Encoding.UTF8, "application/json")
                        }
                    );
                }

                if (request.RequestUri?.AbsoluteUri == $"https://raw.githubusercontent.com/AterDev/Perigon.Modules/{branch}/package_modules/SystemMod.zip")
                {
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(zipBytes)
                        }
                    );
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            })
        );

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var packagePath = await service.DownloadOfficialModulePackageAsync(
                "Perigon.SystemMod",
                tempDir,
                isAot,
                cancellationToken
            );

            Assert.True(File.Exists(packagePath));
            Assert.Equal(zipBytes, await File.ReadAllBytesAsync(packagePath, cancellationToken));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    private static OfficialModuleService CreateService(HttpMessageHandler handler)
    {
        var stringLocalizer = new Mock<IStringLocalizer<Localizer>>();
        stringLocalizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        stringLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var localizer = new Localizer(stringLocalizer.Object);
        return new OfficialModuleService(
            new HttpClient(handler),
            localizer,
            NullLogger<OfficialModuleService>.Instance
        );
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return handler(request);
        }
    }
}
