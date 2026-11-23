using CommandLine.Commands;
using Share.Helper;
using Xunit;

namespace CoreMod.Tests.Commands;

public class EntityGenerateCommandTests
{
    [Fact]
    public void AotJsonContextSettings_ShouldUseCurrentDirectoryWhenPathIsOmitted()
    {
        var settings = new AotJsonContextSettings();

        Assert.Null(settings.Path);
        Assert.Equal("aot", SubCommand.Aot);
        Assert.Equal("jsonContext", SubCommand.JsonContext);
    }

    [Theory]
    [InlineData(typeof(EntityGenerateSettings))]
    [InlineData(typeof(GenerateControllerSettings))]
    [InlineData(typeof(GenerateEntitySettings))]
    public void GenerateSettings_ShouldBeConcreteAndInstantiable(Type settingsType)
    {
        Assert.False(settingsType.IsAbstract);
        Assert.NotNull(Activator.CreateInstance(settingsType));
        Assert.True(typeof(EntityGenerateSettings).IsAssignableFrom(settingsType)
            || settingsType == typeof(GenerateEntitySettings));
    }

    [Theory]
    [InlineData(typeof(GenerateDtoCommand))]
    [InlineData(typeof(GenerateManagerCommand))]
    [InlineData(typeof(GenerateControllerCommand))]
    public void GenerateCommands_ShouldUseConcreteSettings(Type commandType)
    {
        var settingsType = commandType.BaseType?.GetGenericArguments().Single();

        Assert.NotNull(settingsType);
        Assert.False(settingsType!.IsAbstract);
        Assert.True(typeof(EntityGenerateSettings).IsAssignableFrom(settingsType));
    }

    [Fact]
    public void GenerateEntityCommand_ShouldUseConcreteSettings()
    {
        var settings = new GenerateEntitySettings();

        Assert.NotNull(settings);
    }

    [Fact]
    public void RequestSettings_ShouldNotCoverBaseServiceByDefault()
    {
        var settings = new RequestSettings { OutputPath = string.Empty };

        Assert.False(settings.CoverBaseService);
    }
}
