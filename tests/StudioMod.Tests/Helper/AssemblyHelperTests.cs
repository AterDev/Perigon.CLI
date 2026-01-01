using Xunit;
using CodeGenerator.Helper;
using System;
using System.IO;
using System.Xml.Linq;

namespace StudioMod.Tests.Helper;

public class AssemblyHelperTests : IDisposable
{
    private readonly string _testPath;

    public AssemblyHelperTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "AssemblyHelperTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void FindProjectFile_ShouldFindCsprojInDirectory()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        File.WriteAllText(projectPath, "<Project></Project>");

        var dir = new DirectoryInfo(_testPath);

        // Act
        var result = AssemblyHelper.FindProjectFile(dir);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectPath, result.FullName);
    }

    [Fact]
    public void FindProjectFile_ShouldFindCsprojInParentDirectory()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        File.WriteAllText(projectPath, "<Project></Project>");

        var subDir = Path.Combine(_testPath, "SubDir");
        Directory.CreateDirectory(subDir);
        var dir = new DirectoryInfo(subDir);

        var rootDir = new DirectoryInfo(_testPath);

        // Act
        var result = AssemblyHelper.FindProjectFile(dir, rootDir);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectPath, result.FullName);
    }

    [Fact]
    public void FindProjectFile_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var dir = new DirectoryInfo(_testPath);

        // Act
        var result = AssemblyHelper.FindProjectFile(dir);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAssemblyName_ShouldReturnCustomAssemblyName()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <AssemblyName>CustomAssemblyName</AssemblyName>
              </PropertyGroup>
            </Project>
            """;
        File.WriteAllText(projectPath, csprojContent);

        var fileInfo = new FileInfo(projectPath);

        // Act
        var result = AssemblyHelper.GetAssemblyName(fileInfo);

        // Assert
        Assert.Equal("CustomAssemblyName", result);
    }

    [Fact]
    public void GetAssemblyName_ShouldReturnFileNameWhenNotSpecified()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;
        File.WriteAllText(projectPath, csprojContent);

        var fileInfo = new FileInfo(projectPath);

        // Act
        var result = AssemblyHelper.GetAssemblyName(fileInfo);

        // Assert
        Assert.Equal("TestProject", result);
    }

    [Fact]
    public void GetAssemblyName_ShouldReturnFileNameWhenUsingMSBuildVariable()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <AssemblyName>$(MSBuildProjectName)</AssemblyName>
              </PropertyGroup>
            </Project>
            """;
        File.WriteAllText(projectPath, csprojContent);

        var fileInfo = new FileInfo(projectPath);

        // Act
        var result = AssemblyHelper.GetAssemblyName(fileInfo);

        // Assert
        Assert.Equal("TestProject", result);
    }

    [Fact]
    public void FindFileInProject_ShouldFindFileInProject()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        File.WriteAllText(projectPath, "<Project></Project>");

        var fileName = "SomeFile.cs";
        var filePath = Path.Combine(_testPath, fileName);
        File.WriteAllText(filePath, "// content");

        // Act
        var result = AssemblyHelper.FindFileInProject(projectPath, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(filePath, result);
    }

    [Fact]
    public void FindFileInProject_ShouldFindFileInSubdirectory()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        File.WriteAllText(projectPath, "<Project></Project>");

        var subDir = Path.Combine(_testPath, "Models");
        Directory.CreateDirectory(subDir);

        var fileName = "*.cs";
        var filePath = Path.Combine(subDir, "TestModel.cs");
        File.WriteAllText(filePath, "// content");

        // Act
        var result = AssemblyHelper.FindFileInProject(projectPath, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("TestModel.cs", result);
    }

    [Fact]
    public void FindFileInProject_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var projectName = "TestProject.csproj";
        var projectPath = Path.Combine(_testPath, projectName);
        File.WriteAllText(projectPath, "<Project></Project>");

        // Act
        var result = AssemblyHelper.FindFileInProject(projectPath, "NonExistent.cs");

        // Assert
        Assert.Null(result);
    }
}
