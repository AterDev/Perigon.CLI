using CommandLine.Commands;
using Microsoft.Extensions.Localization;
using Moq;
using Share;
using Xunit;

namespace CoreMod.Tests.Commands;

[CollectionDefinition("CurrentDirectory", DisableParallelization = true)]
public sealed class CurrentDirectoryCollection
{
}

[Collection("CurrentDirectory")]
public sealed class UpdateCommandTests
{
    [Fact]
    public void TrySetSolution_ShouldUseTheSolutionRootFromANestedDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"perigon project {Guid.NewGuid():N}");
        var nestedDirectory = Path.Combine(root, PathConst.ServicesPath, "DemoService");
        Directory.CreateDirectory(nestedDirectory);
        var originalDirectory = Environment.CurrentDirectory;

        try
        {
            File.WriteAllText(Path.Combine(root, "Demo.slnx"), "<Solution />");
            Environment.CurrentDirectory = nestedDirectory;
            var context = new SolutionContext();

            Assert.True(CommandSolutionHelper.TrySetSolution(context, CreateLocalizer()));
            Assert.Equal(root, context.SolutionPath);
            Assert.Equal(Path.Combine(root, PathConst.EntityPath), context.EntityPath);
            Assert.Equal(Path.Combine(root, PathConst.ModulesPath), context.ModulesPath);
            Assert.Equal(Path.Combine(root, PathConst.ServicesPath), context.ServicesPath);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFailOutsideASolutionDirectory()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"perigon-update-command-{Guid.NewGuid():N}"
        );
        Directory.CreateDirectory(temporaryDirectory);
        var originalDirectory = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = temporaryDirectory;
            var projectContext = new SolutionContext();
            var command = new UpdateCommand(
                null!,
                projectContext,
                CreateLocalizer()
            );

            var exitCode = await command.ExecuteAsync(cancellationToken);

            Assert.Equal(1, exitCode);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static Localizer CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<Localizer>>();
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] _) => new LocalizedString(name, name));
        return new Localizer(localizer.Object);
    }
}
