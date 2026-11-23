using CoreMod.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Share;
using Xunit;

namespace CoreMod.Tests.Services;

public sealed class TemplateUpdateServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"perigon-template-update-{Guid.NewGuid():N}"
    );

    public TemplateUpdateServiceTests()
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
    public async Task CompareAsync_ShouldOnlyReturnSupportedNewAndChangedFiles()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "old");
        await WriteAsync(templateRoot, "src/Perigon/Changed.cs", "new");
        await WriteAsync(templateRoot, "src/Perigon/Added.cs", "added");
        await WriteAsync(projectRoot, "src/Perigon/LineEndings.cs", "a\r\nb\r\n");
        await WriteAsync(templateRoot, "src/Perigon/LineEndings.cs", "a\nb\n");
        await WriteAsync(projectRoot, "src/Perigon/ProjectOnly.cs", "keep");

        await WriteAsync(projectRoot, "src/Definition/ServiceDefaults/Existing.cs", "old");
        await WriteAsync(templateRoot, "src/Definition/ServiceDefaults/Existing.cs", "new");
        await WriteAsync(templateRoot, "src/Definition/Share/Added.cs", "added");
        await WriteAsync(templateRoot, "src/Definition/EntityFramework/Extensions.cs", "new");
        await WriteAsync(templateRoot, "src/Definition/EntityFramework/Migrations/Initial.cs", "must ignore");
        await WriteAsync(templateRoot, "src/Definition/EntityFramework/DefaultDbContext.cs", "must ignore");
        await WriteAsync(templateRoot, "src/Definition/EntityFramework/AppDbContext/AnalysisDbContext.cs", "must ignore");
        await WriteAsync(templateRoot, "src/Definition/EntityFramework/AppDbContext/ReadonlyDbContext.cs", "must ignore");
        await WriteAsync(templateRoot, "src/Services/NotManaged.cs", "must ignore");
        await WriteAsync(templateRoot, "src/Perigon/Perigon.AspNetCore/Constants/WebConst.cs", "must ignore");
        await WriteAsync(templateRoot, "scripts/New.ps1", "new script");
        await WriteAsync(projectRoot, "scripts/Existing.ps1", "project only");
        await WriteAsync(templateRoot, ".agents/skills/update/SKILL.md", "new skill");

        var changes = await new TemplateComparisonService().CompareAsync(
            projectRoot,
            templateRoot,
            cancellationToken
        );
        var paths = changes.Select(change => change.RelativePath).ToArray();

        Assert.Equal(
            [
                ".agents/skills/update/SKILL.md",
                "scripts/New.ps1",
                "src/Definition/EntityFramework/Extensions.cs",
                "src/Definition/ServiceDefaults/Existing.cs",
                "src/Definition/Share/Added.cs",
                "src/Perigon/Added.cs",
                "src/Perigon/Changed.cs",
            ],
            paths
        );
        Assert.True(changes.Single(change => change.RelativePath == "src/Perigon/Added.cs").IsNew);
        Assert.False(changes.Single(change => change.RelativePath == "src/Perigon/Changed.cs").IsNew);
    }

    [Fact]
    public async Task CompareAsync_ShouldIgnoreWhitespaceOnlyDifferences()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        await WriteAsync(
            projectRoot,
            "src/Perigon/Formatting.cs",
            "  first  \r\n\t\t\r\n    second\t\r\n  third  value \r\n"
        );
        await WriteAsync(
            templateRoot,
            "src/Perigon/Formatting.cs",
            "first\nsecond\nthird value\n"
        );

        var changes = await new TemplateComparisonService().CompareAsync(
            projectRoot,
            templateRoot,
            cancellationToken
        );

        Assert.Empty(changes);
    }

    [Fact]
    public async Task CompareAsync_ShouldIncludeSingularAgentSkillsDirectory()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        await WriteAsync(templateRoot, ".agent/skills/update/SKILL.md", "new skill");

        var changes = await new TemplateComparisonService().CompareAsync(
            projectRoot,
            templateRoot,
            cancellationToken
        );

        var change = Assert.Single(changes);
        Assert.Equal(".agent/skills/update/SKILL.md", change.RelativePath);
    }

    [Fact]
    public void TemplateFileChange_ShouldReportMeaningfulLineStats()
    {
        var change = new TemplateFileChange(
            "src/Perigon/Changed.cs",
            "same\nold\nkeep",
            "same\nnew\nadded\nkeep"
        );

        Assert.Equal(2, change.DiffStats.AddedLines);
        Assert.Equal(1, change.DiffStats.RemovedLines);
        Assert.Contains("- old", change.Diff);
        Assert.Contains("+ new", change.Diff);
    }

    [Fact]
    public async Task ApplyAsync_ShouldCopyOnlyThePreparedChanges()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "old");
        await WriteAsync(templateRoot, "src/Perigon/Changed.cs", "new");
        await WriteAsync(templateRoot, "src/Perigon/Added.cs", "added");
        await WriteAsync(projectRoot, "src/Services/ProjectOnly.cs", "keep");
        await WriteAsync(projectRoot, "src/Definition/EntityFramework/DefaultDbContext.cs", "keep context");
        await WriteAsync(templateRoot, "src/Definition/EntityFramework/DefaultDbContext.cs", "template context");

        var comparison = new TemplateComparisonService();
        var changes = await comparison.CompareAsync(projectRoot, templateRoot, cancellationToken);
        await comparison.ApplyAsync(projectRoot, templateRoot, changes, cancellationToken);

        Assert.Equal("new", await ReadAsync(projectRoot, "src/Perigon/Changed.cs"));
        Assert.Equal("added", await ReadAsync(projectRoot, "src/Perigon/Added.cs"));
        Assert.Equal("keep", await ReadAsync(projectRoot, "src/Services/ProjectOnly.cs"));
        Assert.Equal(
            "keep context",
            await ReadAsync(projectRoot, "src/Definition/EntityFramework/DefaultDbContext.cs")
        );
    }

    [Fact]
    public async Task TemplateUpdateService_ShouldApplyOnlySelectedChanges()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "old");
        await WriteAsync(templateRoot, "src/Perigon/Changed.cs", "new");
        await WriteAsync(templateRoot, "src/Perigon/Added.cs", "added");

        var comparison = new TemplateComparisonService();
        var changes = await comparison.CompareAsync(projectRoot, templateRoot, cancellationToken);
        using var plan = new TemplateUpdatePlan(
            projectRoot,
            templateRoot,
            changes,
            Path.Combine(_root, "working")
        );
        var context = new SolutionContext()
        {
            SolutionPath = projectRoot,
        };
        var service = new TemplateUpdateService(
            context,
            comparison,
            new RecordingCommandRunner(),
            NullLogger<TemplateUpdateService>.Instance
        );

        var selectedChanges = changes
            .Where(change => change.RelativePath == "src/Perigon/Changed.cs")
            .ToArray();
        await service.ApplyAsync(plan, selectedChanges, cancellationToken);

        Assert.Equal("new", await ReadAsync(projectRoot, "src/Perigon/Changed.cs"));
        Assert.False(File.Exists(Path.Combine(projectRoot, "src", "Perigon", "Added.cs")));
    }

    [Fact]
    public async Task ApplyAsync_ShouldPreserveBinaryAgentFiles()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        var currentBytes = new byte[] { 0x00, 0x01, 0x02 };
        var templateBytes = new byte[] { 0x00, 0xFF, 0x10, 0x20 };
        await WriteBytesAsync(projectRoot, ".agents/assets/icon.bin", currentBytes);
        await WriteBytesAsync(templateRoot, ".agents/assets/icon.bin", templateBytes);

        var comparison = new TemplateComparisonService();
        var changes = await comparison.CompareAsync(projectRoot, templateRoot, cancellationToken);

        var change = Assert.Single(changes);
        Assert.True(change.IsBinary);
        Assert.Contains("binary content differs", change.Diff);

        await comparison.ApplyAsync(projectRoot, templateRoot, changes, cancellationToken);

        Assert.Equal(
            templateBytes,
            await File.ReadAllBytesAsync(
                Path.Combine(projectRoot, ".agents", "assets", "icon.bin"),
                cancellationToken
            )
        );
    }

    [Fact]
    public async Task ApplyAsync_ShouldRejectFilesChangedAfterComparison()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);

        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "old");
        await WriteAsync(templateRoot, "src/Perigon/Changed.cs", "new");

        var comparison = new TemplateComparisonService();
        var changes = await comparison.CompareAsync(projectRoot, templateRoot, cancellationToken);
        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "edited after compare");

        await Assert.ThrowsAsync<IOException>(
            () => comparison.ApplyAsync(projectRoot, templateRoot, changes, cancellationToken)
        );
        Assert.Equal("edited after compare", await ReadAsync(projectRoot, "src/Perigon/Changed.cs"));
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldInstallLatestTemplateBeforeCreatingComparisonProject()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        Directory.CreateDirectory(projectRoot);
        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "old");

        var runner = new RecordingCommandRunner();
        var context = new SolutionContext()
        {
            SolutionPath = projectRoot,
        };
        var service = new TemplateUpdateService(
            context,
            new TemplateComparisonService(),
            runner,
            NullLogger<TemplateUpdateService>.Instance
        );

        using var plan = await service.CreatePlanAsync(cancellationToken);

        Assert.Equal(
            "new install Perigon.templates",
            string.Join(' ', runner.Calls[0].Arguments)
        );
        Assert.Equal("new perigon-webapi", string.Join(' ', runner.Calls[1].Arguments.Take(2)));
        Assert.Contains(
            plan.Changes,
            change => change.RelativePath == "src/Perigon/Changed.cs" && !change.IsNew
        );
        Assert.Contains(
            plan.Changes,
            change => change.RelativePath == "src/Perigon/Added.cs" && change.IsNew
        );
    }

    [Fact]
    public async Task CreatePlanAsync_ShouldPreserveMiniApiAndAngularOptions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projectRoot = Path.Combine(_root, "project");
        Directory.CreateDirectory(projectRoot);
        await WriteAsync(projectRoot, ".config/perigon.config.toml", "isAOT = true");
        Directory.CreateDirectory(Path.Combine(projectRoot, "src", "app"));
        await WriteAsync(projectRoot, "src/Perigon/Changed.cs", "old");

        var runner = new RecordingCommandRunner();
        var context = new SolutionContext()
        {
            SolutionPath = projectRoot,
        };
        var service = new TemplateUpdateService(
            context,
            new TemplateComparisonService(),
            runner,
            NullLogger<TemplateUpdateService>.Instance
        );

        using var plan = await service.CreatePlanAsync(cancellationToken);

        Assert.Equal("new perigon-miniapi", string.Join(' ', runner.Calls[1].Arguments.Take(2)));
        Assert.Equal(
            ["--frontType", "Angular"],
            runner.Calls[1].Arguments.SkipWhile(argument => argument != "--frontType")
                .Take(2)
                .ToArray()
        );
    }

    [Fact]
    public async Task BuildAsync_ShouldBuildTheUpdatedSolution()
    {
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);
        var runner = new RecordingCommandRunner();
        var context = new SolutionContext()
        {
            SolutionPath = projectRoot,
        };
        var service = new TemplateUpdateService(
            context,
            new TemplateComparisonService(),
            runner,
            NullLogger<TemplateUpdateService>.Instance
        );
        using var plan = new TemplateUpdatePlan(projectRoot, templateRoot, [], _root);

        await service.BuildAsync(plan, TestContext.Current.CancellationToken);

        var call = Assert.Single(runner.Calls);
        Assert.Equal("dotnet", call.Command);
        Assert.Equal(["build"], call.Arguments);
        Assert.Equal(projectRoot, call.WorkingDirectory);
    }

    [Fact]
    public async Task BuildAsync_ShouldReportBuildOutputWhenCompilationFails()
    {
        var projectRoot = Path.Combine(_root, "project");
        var templateRoot = Path.Combine(_root, "template");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(templateRoot);
        var context = new SolutionContext()
        {
            SolutionPath = projectRoot,
        };
        var service = new TemplateUpdateService(
            context,
            new TemplateComparisonService(),
            new FailedBuildCommandRunner(),
            NullLogger<TemplateUpdateService>.Instance
        );
        using var plan = new TemplateUpdatePlan(projectRoot, templateRoot, [], _root);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.BuildAsync(plan, TestContext.Current.CancellationToken)
        );

        Assert.Contains("compiler error", exception.Message);
    }

    private static async Task WriteAsync(string root, string relativePath, string content)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, content);
    }

    private static async Task WriteBytesAsync(string root, string relativePath, byte[] content)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(path, content);
    }

    private static Task<string> ReadAsync(string root, string relativePath)
    {
        return File.ReadAllTextAsync(
            Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar))
        );
    }

    private sealed class RecordingCommandRunner : ICommandRunner
    {
        public List<CommandCall> Calls { get; } = [];

        public Task<CommandExecutionResult> RunAsync(
            string command,
            IReadOnlyList<string> arguments,
            string? workingDirectory = null,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            Calls.Add(new(command, arguments.ToArray(), workingDirectory));

            if (arguments.SequenceEqual(["new", "install", ConstVal.TemplatePackageId]))
            {
                return Task.FromResult(new CommandExecutionResult(0, "installed"));
            }

            if (arguments.SequenceEqual(["build"]))
            {
                return Task.FromResult(new CommandExecutionResult(0, "build succeeded"));
            }

            if (arguments.Count >= 2
                && arguments[0] == "new"
                && (arguments[1] == ConstVal.WebApi || arguments[1] == ConstVal.Mini))
            {
                var outputIndex = arguments.ToList().IndexOf("--output");
                var outputPath = arguments[outputIndex + 1];
                WriteAsync(outputPath, "src/Perigon/Changed.cs", "new").GetAwaiter().GetResult();
                WriteAsync(outputPath, "src/Perigon/Added.cs", "added").GetAwaiter().GetResult();
                return Task.FromResult(new CommandExecutionResult(0, "created"));
            }

            throw new InvalidOperationException($"Unexpected command: {command} {string.Join(' ', arguments)}");
        }
    }

    private sealed class FailedBuildCommandRunner : ICommandRunner
    {
        public Task<CommandExecutionResult> RunAsync(
            string command,
            IReadOnlyList<string> arguments,
            string? workingDirectory = null,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(new CommandExecutionResult(1, "compiler error"));
    }

    private sealed record CommandCall(
        string Command,
        IReadOnlyList<string> Arguments,
        string? WorkingDirectory
    );
}
