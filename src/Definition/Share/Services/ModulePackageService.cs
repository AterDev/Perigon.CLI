using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodeGenerator.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Share.Helper;
using Share.Models;

namespace Share.Services;

/// <summary>
/// Module packaging service
/// </summary>
public class ModulePackageService(
    IProjectContext projectContext,
    Localizer localizer,
    ILogger<ModulePackageService> logger
)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly Localizer _localizer = localizer;
    private readonly ILogger<ModulePackageService> _logger = logger;

    /// <summary>
    /// Package a module
    /// </summary>
    /// <param name="moduleName">Module name (with Mod suffix)</param>
    /// <param name="serviceName">Service name</param>
    /// <returns>Path to the created package</returns>
    public async Task<string?> PackageModuleAsync(string moduleName, string serviceName)
    {
        try
        {
            // Validate inputs
            if (!await ValidateModuleAsync(moduleName, serviceName))
            {
                return null;
            }

            // Get module metadata
            var metadata = await GetModuleMetadataAsync(moduleName);
            if (metadata == null)
            {
                OutputHelper.Error(_localizer.Get(Localizer.DisplayNameAttributeNotFound));
                return null;
            }

            // Analyze dependencies
            if (!await ValidateDependenciesAsync(moduleName, serviceName))
            {
                return null;
            }

            // Create package
            return await CreatePackageAsync(moduleName, serviceName, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error packaging module {ModuleName}", moduleName);
            OutputHelper.Error($"Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validate module existence
    /// </summary>
    private Task<bool> ValidateModuleAsync(string moduleName, string serviceName)
    {
        // Check module directory
        var modulePath = Path.Combine(_projectContext.ModulesPath!, moduleName);
        if (!Directory.Exists(modulePath))
        {
            OutputHelper.Error(_localizer.Get(Localizer.ModuleNotFound, moduleName));
            return Task.FromResult(false);
        }

        // Check service directory
        var servicePath = Path.Combine(_projectContext.ServicesPath!, serviceName);
        if (!Directory.Exists(servicePath))
        {
            OutputHelper.Error(_localizer.Get(Localizer.ServiceNotFound, serviceName));
            return Task.FromResult(false);
        }

        // Check Entity module directory
        var entityModulePath = Path.Combine(_projectContext.EntityPath!, moduleName);
        if (!Directory.Exists(entityModulePath))
        {
            OutputHelper.Error(_localizer.Get(Localizer.EntityModuleNotFound, moduleName));
            return Task.FromResult(false);
        }

        // Check ModuleExtensions.cs
        var moduleExtensionsPath = Path.Combine(modulePath, "ModuleExtensions.cs");
        if (!File.Exists(moduleExtensionsPath))
        {
            OutputHelper.Error(_localizer.Get(Localizer.ModuleExtensionsNotFound));
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    /// Get module metadata from ModuleExtensions.cs
    /// </summary>
    private async Task<ModulePackageMetadata?> GetModuleMetadataAsync(string moduleName)
    {
        var moduleExtensionsPath = Path.Combine(
            _projectContext.ModulesPath!,
            moduleName,
            "ModuleExtensions.cs"
        );

        var content = await File.ReadAllTextAsync(moduleExtensionsPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = await tree.GetRootAsync();

        // Find DisplayName attribute
        var displayNameAttr = root
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .FirstOrDefault(a =>
                a.Name.ToString().Contains("DisplayName")
                || a.Name.ToString().Contains("Display")
            );

        if (displayNameAttr == null)
        {
            return null;
        }

        // Parse DisplayName value
        string? author = null;
        string? displayName = null;

        if (displayNameAttr.ArgumentList?.Arguments.Count > 0)
        {
            var value = displayNameAttr.ArgumentList.Arguments[0].Expression.ToString().Trim('"');
            if (value.Contains("::"))
            {
                var parts = value.Split("::", 2);
                author = parts[0].Trim();
                displayName = parts[1].Trim();
            }
            else
            {
                author = value;
                displayName = moduleName;
            }
        }

        // Find Description attribute
        var descriptionAttr = root
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .FirstOrDefault(a => a.Name.ToString().Contains("Description"));

        string? description = null;
        if (
            descriptionAttr?.ArgumentList?.Arguments.Count > 0
        )
        {
            description = descriptionAttr
                .ArgumentList.Arguments[0]
                .Expression.ToString()
                .Trim('"');
        }

        return new ModulePackageMetadata
        {
            ModuleName = moduleName,
            Author = author,
            DisplayName = displayName,
            Description = description,
        };
    }

    /// <summary>
    /// Validate module dependencies
    /// </summary>
    private async Task<bool> ValidateDependenciesAsync(string moduleName, string serviceName)
    {
        var hasErrors = false;

        // Validate Entity dependencies
        var entityModulePath = Path.Combine(_projectContext.EntityPath!, moduleName);
        if (!await ValidateDirectoryDependenciesAsync(
                entityModulePath,
                moduleName,
                "Entity",
                ["CommonMod", "Share"]
            ))
        {
            hasErrors = true;
        }

        // Validate Module dependencies
        var modulePath = Path.Combine(_projectContext.ModulesPath!, moduleName);
        if (!await ValidateDirectoryDependenciesAsync(
                modulePath,
                moduleName,
                "Module",
                ["CommonMod", "Share"]
            ))
        {
            hasErrors = true;
        }

        // Validate Controller dependencies
        var controllerPath = Path.Combine(
            _projectContext.ServicesPath!,
            serviceName,
            ConstVal.ControllersDir,
            moduleName
        );
        if (Directory.Exists(controllerPath))
        {
            if (!await ValidateDirectoryDependenciesAsync(
                    controllerPath,
                    moduleName,
                    "Controller",
                    [moduleName, "CommonMod", "Share"]
                ))
            {
                hasErrors = true;
            }
        }

        return !hasErrors;
    }

    /// <summary>
    /// Validate dependencies in a directory
    /// </summary>
    private async Task<bool> ValidateDirectoryDependenciesAsync(
        string directoryPath,
        string moduleName,
        string componentType,
        List<string> allowedDependencies
    )
    {
        if (!Directory.Exists(directoryPath))
        {
            return true;
        }

        var hasErrors = false;
        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content);
            var root = await tree.GetRootAsync();

            // Check using directives
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            foreach (var usingDirective in usingDirectives)
            {
                var namespaceName = usingDirective.Name?.ToString();
                if (namespaceName == null)
                {
                    continue;
                }

                // Check if it's a module reference (ends with Mod)
                if (namespaceName.Contains("."))
                {
                    var namespaceParts = namespaceName.Split('.');
                    if (namespaceParts.Any(part => part.EndsWith("Mod")))
                    {
                        var referencedModule = namespaceParts.First(part => part.EndsWith("Mod"));
                        
                        // Check if it's an allowed dependency
                        if (!allowedDependencies.Contains(referencedModule) && referencedModule != moduleName)
                        {
                            OutputHelper.Error(
                                _localizer.Get(
                                    Localizer.ModuleDependencyError,
                                    componentType,
                                    $"{namespaceName} in {Path.GetFileName(file)}"
                                )
                            );
                            hasErrors = true;
                        }
                    }
                }
            }
        }

        return !hasErrors;
    }

    /// <summary>
    /// Create package zip file
    /// </summary>
    private async Task<string> CreatePackageAsync(
        string moduleName,
        string serviceName,
        ModulePackageMetadata metadata
    )
    {
        // Create package_modules directory
        var packagesDir = Path.Combine(_projectContext.SolutionPath!, "package_modules");
        if (!Directory.Exists(packagesDir))
        {
            Directory.CreateDirectory(packagesDir);
        }

        var packagePath = Path.Combine(packagesDir, $"{moduleName}.zip");

        // Delete existing package
        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }

        using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
        {
            // Add metadata
            var metadataEntry = archive.CreateEntry("metadata.json");
            using (var writer = new StreamWriter(metadataEntry.Open()))
            {
                var json = JsonSerializer.Serialize(
                    metadata,
                    new JsonSerializerOptions { WriteIndented = true }
                );
                await writer.WriteAsync(json);
            }

            // Add Entity files
            var entityModulePath = Path.Combine(_projectContext.EntityPath!, moduleName);
            AddDirectoryToArchive(
                archive,
                entityModulePath,
                $"Entity/{moduleName}"
            );

            // Add Module files
            var modulePath = Path.Combine(_projectContext.ModulesPath!, moduleName);
            AddDirectoryToArchive(archive, modulePath, $"Module/{moduleName}");

            // Add Controller files
            var controllerPath = Path.Combine(
                _projectContext.ServicesPath!,
                serviceName,
                ConstVal.ControllersDir,
                moduleName
            );
            if (Directory.Exists(controllerPath))
            {
                AddDirectoryToArchive(
                    archive,
                    controllerPath,
                    $"Controller/{moduleName}"
                );
            }
        }

        OutputHelper.Success(_localizer.Get(Localizer.PackageCreated, packagePath));
        return packagePath;
    }

    /// <summary>
    /// Add directory to archive recursively
    /// </summary>
    private void AddDirectoryToArchive(
        ZipArchive archive,
        string sourceDir,
        string entryPrefix
    )
    {
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var entryName = Path.Combine(entryPrefix, relativePath).Replace("\\", "/");

            archive.CreateEntryFromFile(file, entryName);
        }
    }
}
