using CommandLine.Commands;
using System.Text.Json;
using Xunit;

namespace StudioMod.Tests.Commands;

public class McpConfigCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPrintValidMcpServersJson()
    {
        // Arrange
        var command = new McpConfigCommand();
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            var code = await command.ExecuteAsync(null!, CancellationToken.None);
            var output = writer.ToString();

            // Assert
            Assert.Equal(0, code);
            using var doc = JsonDocument.Parse(output);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("mcpServers", out var mcpServers));
            Assert.True(mcpServers.TryGetProperty("perigon", out var perigonServer));
            Assert.Equal("perigon", perigonServer.GetProperty("command").GetString());

            var args = perigonServer.GetProperty("args").EnumerateArray().Select(v => v.GetString()).ToList();
            Assert.Equal(["mcp", "start"], args);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
