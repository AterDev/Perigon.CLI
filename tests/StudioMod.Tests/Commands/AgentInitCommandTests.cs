using CommandLine.Commands;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace CoreMod.Tests.Commands;

public sealed class AgentInitCommandTests
{
    [Fact]
    public void ApplyPerigonSkill_ExtractsOnlyPerigonSkill()
    {
        string directory = CreateTempDirectory();
        try
        {
            CreateAgentArchive(directory,
                (".agents/skills/perigon/SKILL.md", "perigon-v2"),
                (".agents/skills/code-review/SKILL.md", "code-review"),
                ("docs/Development/ProjectTracking.md", "tracking"));

            AgentInitCommand.ApplyPerigonSkill(directory);

            Assert.Equal("perigon-v2", ReadFile(directory, ".agents/skills/perigon/SKILL.md"));
            Assert.False(File.Exists(Path.Combine(directory, ".agents", "skills", "code-review", "SKILL.md")));
            Assert.False(File.Exists(Path.Combine(directory, "docs", "Development", "ProjectTracking.md")));
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    [Fact]
    public void ApplyLoopSpec_ExtractsSkillsAndPreservesExistingDocs()
    {
        string directory = CreateTempDirectory();
        string existingTracking = Path.Combine(directory, "docs", "Development", "ProjectTracking.md");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(existingTracking)!);
            File.WriteAllText(existingTracking, "project-owned tracking");
            CreateAgentArchive(directory,
                (".agents/skills/code-review/SKILL.md", "code-review"),
                (".agents/skills/commit-message/SKILL.md", "commit-message"),
                (".agents/skills/delivery-loop/SKILL.md", "delivery-loop"),
                (".agents/skills/docs/SKILL.md", "docs-skill"),
                (".agents/skills/test/SKILL.md", "test"),
                (".agents/skills/perigon/SKILL.md", "perigon"),
                ("docs/Development/ProjectTracking.md", "template tracking"),
                ("docs/UserStory/Demand.md", "template demand"));

            AgentInitCommand.ApplyLoopSpec(directory);

            Assert.Equal("code-review", ReadFile(directory, ".agents/skills/code-review/SKILL.md"));
            Assert.Equal("commit-message", ReadFile(directory, ".agents/skills/commit-message/SKILL.md"));
            Assert.Equal("delivery-loop", ReadFile(directory, ".agents/skills/delivery-loop/SKILL.md"));
            Assert.Equal("docs-skill", ReadFile(directory, ".agents/skills/docs/SKILL.md"));
            Assert.Equal("test", ReadFile(directory, ".agents/skills/test/SKILL.md"));
            Assert.False(File.Exists(Path.Combine(directory, ".agents", "skills", "perigon", "SKILL.md")));
            Assert.Equal("project-owned tracking", File.ReadAllText(existingTracking));
            Assert.Equal("template demand", ReadFile(directory, "docs/UserStory/Demand.md"));
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    private static string CreateTempDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"perigon-agent-init-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void CreateAgentArchive(string directory, params (string Path, string Content)[] files)
    {
        using ZipArchive archive = ZipFile.Open(
            Path.Combine(directory, "agent.zip"),
            ZipArchiveMode.Create);
        foreach ((string path, string content) in files)
        {
            ZipArchiveEntry entry = archive.CreateEntry(path);
            using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
            writer.Write(content);
        }
    }

    private static string ReadFile(string directory, string relativePath) =>
        File.ReadAllText(Path.Combine(directory, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }
}
