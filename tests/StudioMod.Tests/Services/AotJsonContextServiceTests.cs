using CoreMod.Services;
using Xunit;

namespace CoreMod.Tests.Services;

public sealed class AotJsonContextServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "perigon-aot-json-context-" + Guid.NewGuid().ToString("N"));

    public AotJsonContextServiceTests()
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
    public async Task GenerateAsync_ShouldGenerateEachServiceContextAndPreserveExplicitRoots()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteProject("AdminService", "AdminEndpoints",
            "Task.FromResult(new CommonMod.Models.AdminDtos.AdminDto())");
        WriteModuleModels();
        await RestoreProjectsAsync();

        var service = new AotJsonContextService();

        var generated = await service.GenerateAsync(_root, TestContext.Current.CancellationToken);

        Assert.Equal(2, generated.Count);
        var contexts = Directory.GetFiles(
                Path.Combine(_root, "src", "Services"),
                "ApiJsonSerializerContext.cs",
                SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();
        Assert.Equal(2, contexts.Length);
        Assert.Contains(contexts, text => text.Contains("UserDto", StringComparison.Ordinal));
        Assert.Contains(contexts, text => text.Contains("AdminDto", StringComparison.Ordinal));
        Assert.Contains(contexts, text => text.Contains("List<", StringComparison.Ordinal)
            && text.Contains("UserDto", StringComparison.Ordinal));
        var userContext = Assert.Single(contexts, text => text.Contains("UserDto", StringComparison.Ordinal));
        Assert.Contains("using CommonMod.Models.UserDtos;", userContext, StringComparison.Ordinal);
        Assert.DoesNotContain("AdminDto", userContext, StringComparison.Ordinal);
        var adminContext = Assert.Single(contexts, text => text.Contains("AdminDto", StringComparison.Ordinal));
        Assert.DoesNotContain("UserDto", adminContext, StringComparison.Ordinal);
        Assert.All(contexts, text => Assert.DoesNotContain("global::CommonMod.Models", text, StringComparison.Ordinal));
        Assert.All(contexts, text => Assert.Contains("Dictionary<string, string>", text, StringComparison.Ordinal));
        Assert.All(contexts, text => Assert.Contains("ProblemDetails", text, StringComparison.Ordinal));
        Assert.All(contexts, text =>
        {
            Assert.Equal(1, CountOccurrences(text, "// <perigon-aot-json:generated>"));
            Assert.Equal(1, CountOccurrences(text, "// </perigon-aot-json:generated>"));
            Assert.Equal(1, CountOccurrences(text, "JsonSerializable(typeof(Dictionary<string, string>))"));
            Assert.Equal(1, CountOccurrences(text, "JsonSerializable(typeof(ProblemDetails))"));
        });
        Assert.DoesNotContain(contexts, text => text.Contains("UnusedDto", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GenerateAsync_ShouldIncludeJsonBodiesButExcludeQueryAndUploadBinding()
    {
        WriteProject("UserService", "UserEndpoints", "TypedResults.Ok(1)");
        WriteModuleModels();
        File.WriteAllText(
            Path.Combine(_root, "src", "Services", "UserService", "Endpoints", "BindingEndpoints.cs"),
            """
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Http.HttpResults;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Routing;

            namespace UserService.Endpoints;

            public static class BindingEndpoints
            {
                public static void MapEndpoints(IEndpointRouteBuilder endpoints)
                {
                    endpoints.MapPost("/json", Write);
                    endpoints.MapPost("/file", Upload);
                    endpoints.MapPost("/documented-file", Upload).Accepts<IFormFile>("multipart/form-data");
                }

                private static Ok<JsonResponse> Write(
                    [AsParameters] QueryArgs query, [FromBody] Dictionary<string, JsonRequest> body) =>
                    TypedResults.Ok(new JsonResponse());

                private static NoContent Upload(IFormFile file) => TypedResults.NoContent();
            }

            public sealed record QueryArgs(int Page, SearchStatus? Status);
            public enum SearchStatus { Ready }
            public sealed record JsonRequest(string Value);
            public sealed record JsonResponse;
            """);
        await RestoreProjectsAsync();

        var path = Assert.Single(await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken));
        var content = File.ReadAllText(path);
        Assert.Contains("Dictionary<string, JsonRequest>", content, StringComparison.Ordinal);
        Assert.Contains("typeof(JsonResponse)", content, StringComparison.Ordinal);
        Assert.DoesNotContain("QueryArgs", content, StringComparison.Ordinal);
        Assert.Contains("typeof(IFormFile)", content, StringComparison.Ordinal);
        Assert.Contains("typeof(SearchStatus?)", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_ShouldBeIdempotent()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteModuleModels();
        await RestoreProjectsAsync();
        var service = new AotJsonContextService();

        await service.GenerateAsync(_root, TestContext.Current.CancellationToken);
        var contextPath = Directory.GetFiles(
                Path.Combine(_root, "src", "Services"),
                "ApiJsonSerializerContext.cs",
                SearchOption.AllDirectories)
            .Single();
        var first = File.ReadAllText(contextPath);
        var firstWrite = File.GetLastWriteTimeUtc(contextPath);

        await Task.Delay(20, TestContext.Current.CancellationToken);
        await service.GenerateAsync(_root, TestContext.Current.CancellationToken);

        Assert.Equal(first, File.ReadAllText(contextPath));
        Assert.Equal(firstWrite, File.GetLastWriteTimeUtc(contextPath));
    }

    [Fact]
    public async Task GenerateAsync_ShouldNotRegisterNonGenericAsyncWrappers()
    {
        WriteProject("UserService", "UserEndpoints", "Task.CompletedTask");
        WriteProject("ValueTaskService", "ValueTaskEndpoints", "ValueTask.CompletedTask");
        WriteModuleModels();
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(_root, TestContext.Current.CancellationToken);

        var contexts = Directory.GetFiles(
                Path.Combine(_root, "src", "Services"),
                "ApiJsonSerializerContext.cs",
                SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();
        Assert.Equal(2, contexts.Length);
        Assert.DoesNotContain(contexts, text => text.Contains("JsonSerializable(typeof(Task))", StringComparison.Ordinal));
        Assert.DoesNotContain(contexts, text => text.Contains("JsonSerializable(typeof(ValueTask))", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GenerateAsync_ShouldDiscoverDirectJsonSerializerCallsOutsideEndpoints()
    {
        WriteProject("UserService", "UserEndpoints", "TypedResults.Ok(1)");
        WriteModuleModels(includeGenericModel: true);
        File.WriteAllText(
            Path.Combine(_root, "src", "Services", "UserService", "JsonCalls.cs"),
            """
            using System.Text.Json;
            using CommonMod.Models;
            using CommonMod.Special;
            namespace UserService;
            public static class JsonCalls
            {
                public static string Write(Envelope<PayloadDto> value) => JsonSerializer.Serialize(value);
                public static Dictionary<string, PayloadDto>? Read(string value) =>
                    JsonSerializer.Deserialize<Dictionary<string, PayloadDto>>(value);
            }
            """);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(
            Path.Combine(_root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs"));
        Assert.Contains("Envelope<PayloadDto>", content, StringComparison.Ordinal);
        Assert.Contains("Dictionary<string, PayloadDto>", content, StringComparison.Ordinal);
        Assert.Contains("JsonSerializable(typeof(PayloadDto))", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_ShouldIgnoreUnrelatedMethodsNamedSerialize()
    {
        WriteProject("UserService", "UserEndpoints", "TypedResults.Ok(1)");
        WriteModuleModels();
        File.WriteAllText(
            Path.Combine(_root, "src", "Services", "UserService", "Unrelated.cs"),
            """
            namespace UserService;
            public static class Unrelated
            {
                public static string Serialize<T>(T value) => value?.ToString() ?? string.Empty;
            }
            """);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(
            Path.Combine(_root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs"));
        Assert.DoesNotContain("Unrelated", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_ShouldRegisterNestedCustomGenericArguments()
    {
        WriteProject(
            "UserService",
            "UserEndpoints",
            "TypedResults.Json(new CommonMod.Models.Envelope<CommonMod.Special.PayloadDto>())");
        WriteModuleModels(includeGenericModel: true);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(_root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs"));
        Assert.Contains("Envelope<", content, StringComparison.Ordinal);
        Assert.Contains("PayloadDto", content, StringComparison.Ordinal);
        Assert.Contains("JsonSerializable(typeof(PayloadDto))", content, StringComparison.Ordinal);
        await BuildProjectsAsync();
    }

    [Fact]
    public async Task GenerateAsync_ShouldRegisterNestedDictionaryArguments()
    {
        WriteProject(
            "UserService",
            "UserEndpoints",
            "TypedResults.Json(new Dictionary<string, CommonMod.Special.PayloadDto>())");
        WriteModuleModels(includeGenericModel: true);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(_root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs"));
        Assert.Contains("Dictionary<", content, StringComparison.Ordinal);
        Assert.Contains("JsonSerializable(typeof(PayloadDto))", content, StringComparison.Ordinal);
        await BuildProjectsAsync();
    }

    [Fact]
    public async Task GenerateAsync_ShouldRejectAnExistingNonContextFileWithoutOverwritingIt()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteModuleModels();
        var contextPath = Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs");
        const string original = "namespace UserService; public partial class ApiJsonSerializerContext { }";
        File.WriteAllText(contextPath, original);
        await RestoreProjectsAsync();

        var service = new AotJsonContextService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateAsync(_root, TestContext.Current.CancellationToken));
        Assert.Equal(original, File.ReadAllText(contextPath));
    }

    [Fact]
    public async Task GenerateAsync_ShouldPreserveFullJsonSerializableAttributeWithoutDuplicatingIt()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteModuleModels();
        var contextPath = Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs");
        File.WriteAllText(
            contextPath,
            """
            using System.Text.Json.Serialization;
            using Microsoft.AspNetCore.Mvc;
            namespace UserService;
            [JsonSerializableAttribute(typeof(CommonMod.Models.UserDtos.UserDto))]
            [JsonSerializableAttribute(typeof(Dictionary<string, string>))]
            [JsonSerializableAttribute(typeof(ProblemDetails))]
            public partial class ApiJsonSerializerContext : JsonSerializerContext { }
            """);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(contextPath);
        Assert.Equal(1, CountOccurrences(
            content,
            "JsonSerializableAttribute(typeof(CommonMod.Models.UserDtos.UserDto))"));
        Assert.Contains("JsonSerializableAttribute(typeof(Dictionary<string, string>))", content);
        Assert.Contains("JsonSerializableAttribute(typeof(ProblemDetails))", content);
    }

    [Fact]
    public async Task GenerateAsync_ShouldCreateContextWithProjectRootNamespaceWhenContextIsMissing()
    {
        WriteProject(
            "SalesService",
            "SalesEndpoints",
            "Task.FromResult(new CommonMod.Models.AdminDtos.AdminDto())",
            includeContext: false,
            rootNamespace: "Company.Sales.Api");
        WriteModuleModels();
        var protectedPath = Path.Combine(
            _root, "src", "Services", "SalesService", "README.md");
        const string protectedContent = "keep this file unchanged";
        File.WriteAllText(protectedPath, protectedContent);
        await RestoreProjectsAsync();

        var generated = await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var contextPath = Path.Combine(
            _root, "src", "Services", "SalesService", "Serialization", "ApiJsonSerializerContext.cs");
        Assert.Contains(contextPath, generated);
        var context = File.ReadAllText(contextPath);
        Assert.Contains("namespace Company.Sales.Api;", context);
        Assert.Equal(protectedContent, File.ReadAllText(protectedPath));

        await BuildProjectsAsync();
    }

    [Fact]
    public async Task GenerateAsync_ShouldUseImportsAndBuildWhenModelsShareATypeName()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteModuleModels(includeDuplicateUserDto: true);
        WriteDuplicateDtoEndpoint();
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var contextPath = Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs");
        var content = File.ReadAllText(contextPath);

        Assert.Contains("= CommonMod.Models.UserDtos.UserDto;", content, StringComparison.Ordinal);
        Assert.Contains("CommonMod.Models.AdminDtos", content, StringComparison.Ordinal);
        Assert.DoesNotContain("global::CommonMod.Models", content, StringComparison.Ordinal);

        // Both same-named models are discovered, and the generated aliases/imports compile.
        Assert.Contains("UserDto", content, StringComparison.Ordinal);
        await BuildProjectsAsync();
    }

    [Fact]
    public async Task GenerateAsync_ShouldPreserveHandwrittenUsingAndShortTypeWhenNamesCollide()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteModuleModels(includeDuplicateUserDto: true);
        WriteDuplicateDtoEndpoint();

        var contextPath = Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs");
        File.WriteAllText(
            contextPath,
            """
            using System.Text.Json.Serialization;
            using Microsoft.AspNetCore.Mvc;
            using CommonMod.Models.UserDtos;
            namespace UserService;
            [JsonSerializable(typeof(UserDto))]
            [JsonSerializable(typeof(Dictionary<string, string>))]
            [JsonSerializable(typeof(ProblemDetails))]
            public partial class ApiJsonSerializerContext : JsonSerializerContext { }
            """);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(contextPath);
        Assert.Equal(1, CountOccurrences(content, "using CommonMod.Models.UserDtos;"));
        Assert.Equal(1, CountOccurrences(content, "[JsonSerializable(typeof(UserDto))]"));
        Assert.Contains("CommonMod.Models.AdminDtos", content, StringComparison.Ordinal);
        Assert.DoesNotContain("global::CommonMod.Models", content, StringComparison.Ordinal);

        await BuildProjectsAsync();
    }

    [Fact]
    public async Task GenerateAsync_ShouldAliasGeneratedTypeAgainstExistingUnregisteredUsing()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new List<CommonMod.Models.UserDtos.UserDto>())");
        WriteModuleModels();
        File.WriteAllText(
            Path.Combine(_root, "src", "Services", "UserService", "OtherModels.cs"),
            "namespace Other.Models; public sealed class UserDto { public string? Name { get; set; } }");

        var contextPath = Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs");
        File.WriteAllText(
            contextPath,
            """
            using System.Text.Json.Serialization;
            using Microsoft.AspNetCore.Mvc;
            using Other.Models;
            namespace UserService;
            [JsonSerializable(typeof(Dictionary<string, string>))]
            [JsonSerializable(typeof(ProblemDetails))]
            public partial class ApiJsonSerializerContext : JsonSerializerContext { }
            """);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var content = File.ReadAllText(contextPath);
        Assert.Contains("using Other.Models;", content, StringComparison.Ordinal);
        Assert.Contains("Other.Models", content, StringComparison.Ordinal);
        Assert.Contains("CommonMod.Models.UserDtos", content, StringComparison.Ordinal);
        Assert.DoesNotContain("global::CommonMod.Models", content, StringComparison.Ordinal);

        await BuildProjectsAsync();
    }

    [Fact]
    public async Task GenerateAsync_ShouldCompileNestedPublicModelTypes()
    {
        WriteProject("UserService", "UserEndpoints",
            "TypedResults.Json(new CommonMod.Models.Outer.Inner())");
        WriteModuleModels(includeNestedModel: true);
        await RestoreProjectsAsync();

        await new AotJsonContextService().GenerateAsync(
            _root, TestContext.Current.CancellationToken);

        var contextPath = Path.Combine(
            _root, "src", "Services", "UserService", "Serialization", "ApiJsonSerializerContext.cs");
        var content = File.ReadAllText(contextPath);
        Assert.Contains("Outer.Inner", content, StringComparison.Ordinal);
        Assert.DoesNotContain("global::CommonMod.Models", content, StringComparison.Ordinal);

        await BuildProjectsAsync();
    }

    private void WriteDuplicateDtoEndpoint()
    {
        File.WriteAllText(
            Path.Combine(_root, "src", "Services", "UserService", "Endpoints", "OtherEndpoint.cs"),
            """
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;
            namespace UserService.Endpoints;
            public static class OtherEndpoint
            {
                public static void MapEndpoints(IEndpointRouteBuilder endpoints) =>
                    endpoints.MapGet("/other", static () => new CommonMod.Models.AdminDtos.UserDto());
            }
            """);
    }

    private void WriteProject(
        string serviceName,
        string endpointName,
        string responseExpression,
        bool includeContext = true,
        string? rootNamespace = null)
    {
        var serviceRoot = Path.Combine(_root, "src", "Services", serviceName);
        Directory.CreateDirectory(Path.Combine(serviceRoot, "Endpoints"));
        Directory.CreateDirectory(Path.Combine(serviceRoot, "Serialization"));
        var rootNamespaceElement = string.IsNullOrWhiteSpace(rootNamespace)
            ? string.Empty
            : $"<RootNamespace>{rootNamespace}</RootNamespace>";
        File.WriteAllText(
            Path.Combine(serviceRoot, $"{serviceName}.csproj"),
            $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <OutputType>Library</OutputType>
                {{rootNamespaceElement}}
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              <ItemGroup><ProjectReference Include="../../Modules/CommonMod/CommonMod.csproj" /></ItemGroup>
            </Project>
            """);
        File.WriteAllText(
            Path.Combine(serviceRoot, "Endpoints", endpointName + ".cs"),
            $$"""
            using CommonMod.Models;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Routing;
            namespace {{serviceName}}.Endpoints;
            public static class {{endpointName}}
            {
                public static void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapGet("/probe", Get);
                public static {{GetReturnType(responseExpression)}} Get() => {{responseExpression}};
            }
            """);
        if (includeContext)
        {
            File.WriteAllText(
                Path.Combine(serviceRoot, "Serialization", "ApiJsonSerializerContext.cs"),
                $$"""
            using System.Text.Json.Serialization;
            using Microsoft.AspNetCore.Mvc;
            namespace {{serviceName}};
            [JsonSerializable(typeof(Dictionary<string, string>))]
            [JsonSerializable(typeof(ProblemDetails))]
            public partial class ApiJsonSerializerContext : JsonSerializerContext { }
            """);
        }
    }

    private static string GetReturnType(string expression) => expression switch
    {
        "Task.CompletedTask" => "Task",
        "ValueTask.CompletedTask" => "ValueTask",
        _ when expression.StartsWith("Task.", StringComparison.Ordinal) => "Task<CommonMod.Models.AdminDtos.AdminDto>",
        _ when expression.StartsWith("TypedResults.Json(new List<", StringComparison.Ordinal)
            => "Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<List<CommonMod.Models.UserDtos.UserDto>>",
        _ when expression.StartsWith("TypedResults.Json(new CommonMod.Models.Envelope<", StringComparison.Ordinal)
            => "Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<CommonMod.Models.Envelope<CommonMod.Special.PayloadDto>>",
        _ when expression.StartsWith("TypedResults.Json(new Dictionary<", StringComparison.Ordinal)
            => "Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<Dictionary<string, CommonMod.Special.PayloadDto>>",
        "TypedResults.Json(new CommonMod.Models.Outer.Inner())"
            => "Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<CommonMod.Models.Outer.Inner>",
        _ => "Microsoft.AspNetCore.Http.HttpResults.Ok<int>",
    };

    private static int CountOccurrences(string text, string value) =>
        text.Split(value, StringSplitOptions.None).Length - 1;

    private async Task RestoreProjectsAsync()
    {
        foreach (var project in Directory.GetFiles(
                     Path.Combine(_root, "src", "Services"),
                     "*.csproj",
                     SearchOption.AllDirectories))
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"restore \"{project}\" --nologo",
                    WorkingDirectory = Path.GetDirectoryName(project)!,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            Assert.True(process.Start(), $"Unable to start restore for {project}.");
            await process.WaitForExitAsync();
            _ = await process.StandardError.ReadToEndAsync();
            Assert.Equal(0, process.ExitCode);
        }
    }

    private async Task BuildProjectsAsync()
    {
        foreach (var project in Directory.GetFiles(
                     Path.Combine(_root, "src", "Services"),
                     "*.csproj",
                     SearchOption.AllDirectories))
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build \"{project}\" --no-restore --nologo",
                    WorkingDirectory = Path.GetDirectoryName(project)!,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            Assert.True(process.Start(), $"Unable to start build for {project}.");
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            Assert.True(process.ExitCode == 0, $"Build failed: {await stdout}\n{await stderr}");
        }
    }

    private void WriteModuleModels(
        bool includeDuplicateUserDto = false,
        bool includeNestedModel = false,
        bool includeGenericModel = false)
    {
        var moduleRoot = Path.Combine(_root, "src", "Modules", "CommonMod");
        Directory.CreateDirectory(Path.Combine(moduleRoot, "Models", "UserDtos"));
        Directory.CreateDirectory(Path.Combine(moduleRoot, "Models", "AdminDtos"));
        File.WriteAllText(
            Path.Combine(moduleRoot, "CommonMod.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><FrameworkReference Include=\"Microsoft.AspNetCore.App\" /></ItemGroup></Project>");
        File.WriteAllText(
            Path.Combine(moduleRoot, "ModuleExtensions.cs"),
            "using Microsoft.Extensions.Hosting; namespace CommonMod; public static class ModuleExtensions { public static IHostApplicationBuilder AddCommonMod(this IHostApplicationBuilder builder) => builder; }");
        File.WriteAllText(
            Path.Combine(moduleRoot, "Models", "UserDtos", "UserDto.cs"),
            "namespace CommonMod.Models.UserDtos; public sealed class UserDto { public string? Name { get; set; } }");
        File.WriteAllText(
            Path.Combine(moduleRoot, "Models", "AdminDtos", "AdminDto.cs"),
            "namespace CommonMod.Models.AdminDtos; public sealed class AdminDto { public string? Name { get; set; } }");
        if (includeDuplicateUserDto)
        {
            File.WriteAllText(
                Path.Combine(moduleRoot, "Models", "AdminDtos", "UserDto.cs"),
                "namespace CommonMod.Models.AdminDtos; public sealed class UserDto { public string? Name { get; set; } }");
        }
        if (includeNestedModel)
        {
            File.WriteAllText(
                Path.Combine(moduleRoot, "Models", "Outer.cs"),
                "namespace CommonMod.Models; public sealed class Outer { public sealed class Inner { public string? Name { get; set; } } }");
        }
        if (includeGenericModel)
        {
            File.WriteAllText(
                Path.Combine(moduleRoot, "Models", "Envelope.cs"),
                "namespace CommonMod.Models; public sealed class Envelope<T> { public T? Value { get; set; } }");
            Directory.CreateDirectory(Path.Combine(moduleRoot, "Special"));
            File.WriteAllText(
                Path.Combine(moduleRoot, "Special", "PayloadDto.cs"),
                "namespace CommonMod.Special; public sealed class PayloadDto { public string? Value { get; set; } }");
        }
        File.WriteAllText(
            Path.Combine(moduleRoot, "Models", "UnusedDto.cs"),
            "namespace CommonMod.Models; public sealed class UnusedDto { public string? Name { get; set; } }");
    }
}
