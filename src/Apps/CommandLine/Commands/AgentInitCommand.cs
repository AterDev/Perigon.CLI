using Share;
using Share.Helper;
using System.IO.Compression;

namespace CommandLine.Commands;

/// <summary>
/// Installs the non-MCP agent skills and loop-spec files shipped with the CLI.
/// </summary>
public sealed class AgentInitCommand(Localizer localizer)
{
    private const string PerigonSkillAction = "Perigon Skill";
    private const string LoopSpecAction = "Loop spec";

    private static readonly ZipExtractionRule[] PerigonSkillEntries =
    [
        new(".agents/skills/perigon", OverwriteFiles: true)
    ];

    private static readonly ZipExtractionRule[] LoopSpecEntries =
    [
        new(".agents/skills/code-review", OverwriteFiles: true),
        new(".agents/skills/commit-message", OverwriteFiles: true),
        new(".agents/skills/delivery-loop", OverwriteFiles: true),
        new(".agents/skills/docs", OverwriteFiles: true),
        new(".agents/skills/test", OverwriteFiles: true),
        new("docs", OverwriteFiles: false)
    ];

    private sealed record ZipExtractionRule(string RootPath, bool OverwriteFiles);

    public Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string currentDirectory = Directory.GetCurrentDirectory();

        IReadOnlyList<string> selectedActions;
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            selectedActions = [PerigonSkillAction, LoopSpecAction];
        }
        else
        {
            selectedActions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(localizer.Get(Localizer.AgentInitSelectActions))
                    .AddChoices(GetAgentSetupChoices(localizer))
                    .NotRequired()
            );
        }

        if (selectedActions.Count == 0)
        {
            OutputHelper.Info(localizer.Get(Localizer.AgentInitNoActions));
            return Task.FromResult(0);
        }

        try
        {
            if (IsActionSelected(selectedActions, PerigonSkillAction))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyPerigonSkill(currentDirectory);
            }

            if (IsActionSelected(selectedActions, LoopSpecAction))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyLoopSpec(currentDirectory);
            }

            OutputHelper.Success(localizer.Get(Localizer.AgentInitCompleted));
            return Task.FromResult(0);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            OutputHelper.Error(localizer.Get(Localizer.AgentInitFailed, ex.Message));
            return Task.FromResult(-1);
        }
    }

    private static string[] GetAgentSetupChoices(Localizer localizer) =>
    [
        $"{PerigonSkillAction} - {localizer.Get(Localizer.AgentInitSkillChoiceDescription)}",
        $"{LoopSpecAction} - {localizer.Get(Localizer.AgentInitLoopSpecChoiceDescription)}"
    ];

    private static bool IsActionSelected(IEnumerable<string> selectedActions, string actionName) =>
        selectedActions.Any(action => action.StartsWith(actionName, StringComparison.OrdinalIgnoreCase));

    internal static void ApplyPerigonSkill(string currentDirectory) =>
        ExtractAgentEntries(currentDirectory, PerigonSkillEntries);

    internal static void ApplyLoopSpec(string currentDirectory) =>
        ExtractAgentEntries(currentDirectory, LoopSpecEntries);

    private static void ExtractAgentEntries(
        string currentDirectory,
        IReadOnlyList<ZipExtractionRule> extractionRules
    )
    {
        string? agentZipPath = FindAgentZipPath(currentDirectory);
        if (string.IsNullOrWhiteSpace(agentZipPath))
        {
            throw new FileNotFoundException(null, "agent.zip");
        }

        using ZipArchive archive = ZipFile.OpenRead(agentZipPath);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryPath = NormalizeZipEntryPath(entry.FullName);
            if (string.IsNullOrEmpty(entryPath))
            {
                continue;
            }

            ZipExtractionRule? matchingRule = extractionRules.FirstOrDefault(rule =>
                IsPathUnderRoot(entryPath, rule.RootPath));
            if (matchingRule is null)
            {
                continue;
            }

            string destinationPath = GetSafeDestinationPath(currentDirectory, entryPath);
            bool isDirectory = entry.FullName.EndsWith('/', StringComparison.Ordinal)
                || entry.FullName.EndsWith('\\', StringComparison.Ordinal);

            if (isDirectory)
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            if (!matchingRule.OverwriteFiles && File.Exists(destinationPath))
            {
                continue;
            }

            string destinationDirectory = Path.GetDirectoryName(destinationPath) ?? currentDirectory;
            Directory.CreateDirectory(destinationDirectory);
            entry.ExtractToFile(destinationPath, matchingRule.OverwriteFiles);
        }
    }

    private static string? FindAgentZipPath(string currentDirectory)
    {
        string[] localCandidates =
        [
            Path.Combine(currentDirectory, "agent.zip"),
            Path.Combine(AppContext.BaseDirectory, "agent.zip")
        ];
        foreach (string path in localCandidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        for (DirectoryInfo? directory = new(currentDirectory); directory is not null; directory = directory.Parent)
        {
            string projectArchive = Path.Combine(directory.FullName, "agent.zip");
            if (File.Exists(projectArchive))
            {
                return projectArchive;
            }

            string sourceArchive = Path.Combine(directory.FullName, "src", "Apps", "CommandLine", "agent.zip");
            if (File.Exists(sourceArchive))
            {
                return sourceArchive;
            }
        }

        return null;
    }

    private static bool IsPathUnderRoot(string path, string rootPath) =>
        path.Equals(rootPath, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith($"{rootPath}/", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeZipEntryPath(string entryPath)
    {
        if (string.IsNullOrWhiteSpace(entryPath))
        {
            return string.Empty;
        }

        if (entryPath.StartsWith('/', StringComparison.Ordinal)
            || entryPath.StartsWith('\\', StringComparison.Ordinal)
            || Path.IsPathRooted(entryPath))
        {
            throw new InvalidDataException($"The agent archive contains an unsafe path: {entryPath}");
        }

        string[] segments = entryPath
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(segment => segment is "." or ".." || segment.Contains(':')))
        {
            throw new InvalidDataException($"The agent archive contains an unsafe path: {entryPath}");
        }

        return string.Join('/', segments);
    }

    private static string GetSafeDestinationPath(string currentDirectory, string relativePath)
    {
        string rootPath = Path.GetFullPath(currentDirectory);
        string destinationPath = Path.GetFullPath(
            Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        string rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!destinationPath.Equals(rootPath, comparison)
            && !destinationPath.StartsWith(rootPrefix, comparison))
        {
            throw new InvalidDataException($"The agent archive contains an unsafe path: {relativePath}");
        }

        return destinationPath;
    }
}
