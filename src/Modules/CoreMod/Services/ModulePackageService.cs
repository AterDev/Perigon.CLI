using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO.Compression;

namespace CoreMod.Services;

/// <summary>
/// Module packaging service
/// </summary>
public class ModulePackageService(
    SolutionContext projectContext,
    Localizer localizer,
    ILogger<ModulePackageService> logger
)
{
    private readonly SolutionContext _projectContext = projectContext;
    private readonly Localizer _localizer = localizer;
    private readonly ILogger<ModulePackageService> _logger = logger;

    /// <summary>
    /// Package a module
    /// </summary>
    /// <param name="moduleName">Module name (with Mod suffix)</param>
    /// <param name="serviceName">Service name</param>
    /// <param name="frontPath">Optional frontend module path</param>
    /// <param name="version">Optional package version</param>
    /// <returns>Path to the created package</returns>
    public async Task<string?> PackageModuleAsync(
        string moduleName,
        string serviceName,
        string? frontPath = null,
        string? version = null
    )
    {
        try
        {
            var apiDirectory = SolutionService.IsAOT(_projectContext.SolutionPath)
                ? ConstVal.EndpointsDir
                : ConstVal.ControllersDir;

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

            if (!string.IsNullOrWhiteSpace(version))
            {
                metadata.Version = version;
            }

            // Analyze dependencies
            if (!await ValidateDependenciesAsync(moduleName, serviceName, apiDirectory))
            {
                return null;
            }

            // Create package
            var frontendPath = GetFrontendPath(frontPath);
            if (frontPath != null && frontendPath == null)
            {
                return null;
            }

            return await CreatePackageAsync(moduleName, serviceName, metadata, frontendPath, apiDirectory);
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
        var moduleExtensionsPath = Path.Combine(modulePath, ConstVal.ModuleExtensionFile);
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
    private async Task<PackageMetadata?> GetModuleMetadataAsync(string moduleName)
    {
        var moduleExtensionsPath = Path.Combine(
            _projectContext.ModulesPath!,
            moduleName,
            ConstVal.ModuleExtensionFile
        );

        var content = await File.ReadAllTextAsync(moduleExtensionsPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = await tree.GetRootAsync();

        var moduleExtensionType = root
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault(type =>
                string.Equals(type.Identifier.Text, "ModuleExtensions", StringComparison.Ordinal)
            );

        if (moduleExtensionType == null)
        {
            OutputHelper.Debug($"ModuleExtensions type not found in {moduleExtensionsPath}");
            return null;
        }

        // Find DisplayName attribute
        var displayNameAttr = moduleExtensionType
            .AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(a => IsAttributeNamed(a, "DisplayName"));

        if (displayNameAttr == null)
        {
            OutputHelper.Debug($"DisplayName attribute not found on ModuleExtensions for module {moduleName}");
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
        var descriptionAttr = moduleExtensionType
            .AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(a => IsAttributeNamed(a, "Description"));

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

        OutputHelper.Debug(
            $"Module metadata result => ModuleName={moduleName}, Author={author ?? "<null>"}, DisplayName={displayName ?? "<null>"}, Description={description ?? "<null>"}"
        );


        return new PackageMetadata
        {
            ModuleName = moduleName,
            Author = author,
            DisplayName = displayName,
            Description = description
        };
    }

    private static bool IsAttributeNamed(AttributeSyntax attribute, string expectedName)
    {
        var actualName = attribute.Name switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            _ => attribute.Name.ToString().Split('.').Last().Split(':').Last()
        };

        return string.Equals(
            NormalizeAttributeName(actualName),
            NormalizeAttributeName(expectedName),
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string NormalizeAttributeName(string attributeName)
    {
        return attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase)
            ? attributeName[..^"Attribute".Length]
            : attributeName;
    }

    /// <summary>
    /// Validate module dependencies
    /// </summary>
    private async Task<bool> ValidateDependenciesAsync(string moduleName, string serviceName, string apiDirectory)
    {
        var hasErrors = false;

        // Validate Entity dependencies
        var entityModulePath = Path.Combine(_projectContext.EntityPath!, moduleName);
        if (!await ValidateDirectoryDependenciesAsync(
                entityModulePath,
                moduleName,
                ConstVal.EntityName,
                ["Share"]
            ))
        {
            hasErrors = true;
        }

        // Validate Module dependencies
        var modulePath = Path.Combine(_projectContext.ModulesPath!, moduleName);
        if (!await ValidateDirectoryDependenciesAsync(
                modulePath,
                moduleName,
                ConstVal.ModulesDir,
                ["Share"]
            ))
        {
            hasErrors = true;
        }

        // Validate service API dependencies
        var apiPath = Path.Combine(
            _projectContext.ServicesPath!,
            serviceName,
            apiDirectory,
            moduleName
        );
        if (Directory.Exists(apiPath) &&
            !await ValidateDirectoryDependenciesAsync(
                apiPath,
                moduleName,
                apiDirectory == ConstVal.ControllersDir ? "Controller" : "Endpoint",
                [moduleName, "CommonMod", "Share"]
            ))
        {
            hasErrors = true;
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
            var invalidDependencies = root
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(usingDirective => usingDirective.Name?.ToString())
                .Where(namespaceName => namespaceName != null && namespaceName.Contains("."))
                .Select(namespaceName =>
                {
                    var namespaceParts = namespaceName!.Split('.');
                    return namespaceParts.Any(part => part.EndsWith("Mod"))
                        ? namespaceParts.First(part => part.EndsWith("Mod"))
                        : null;
                })
                .Where(referencedModule =>
                    referencedModule != null &&
                    !allowedDependencies.Contains(referencedModule) &&
                    referencedModule != moduleName
                )
                .Select(referencedModule => $"{referencedModule} in {Path.GetFileName(file)}");

            foreach (var dependency in invalidDependencies)
            {
                OutputHelper.Error(
                    _localizer.Get(
                        Localizer.ModuleDependencyError,
                        componentType,
                        dependency
                    )
                );
                hasErrors = true;
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
        PackageMetadata metadata,
        string? frontendPath,
        string apiDirectory
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
            if (frontendPath != null)
            {
                metadata.Frontend = await GetFrontendTypeAsync(frontendPath);
            }

            // Add metadata
            var metadataEntry = archive.CreateEntry("metadata.json");
            using (var writer = new StreamWriter(metadataEntry.Open()))
            {
                var json = JsonSerializer.Serialize(
                    metadata,
                    PerigonJsonSerializerContext.ModuleMetadata.PackageMetadata
                );
                await writer.WriteAsync(json);
            }

            // Add Entity files
            var entityModulePath = Path.Combine(_projectContext.EntityPath!, moduleName);
            AddDirectoryToArchive(
                archive,
                entityModulePath,
                $"{ConstVal.EntityName}/{moduleName}"
            );

            // Add Module files
            var modulePath = Path.Combine(_projectContext.ModulesPath!, moduleName);
            AddDirectoryToArchive(archive, modulePath, $"{ConstVal.ModulesDir}/{moduleName}");

            // Add service API files
            var apiPath = Path.Combine(
                _projectContext.ServicesPath!,
                serviceName,
                apiDirectory,
                moduleName
            );
            if (Directory.Exists(apiPath))
            {
                AddDirectoryToArchive(
                    archive,
                    apiPath,
                    $"{apiDirectory}/{moduleName}"
                );
            }

            if (frontendPath != null)
            {
                var frontendModuleName = Path.GetFileName(frontendPath);
                AddDirectoryToArchive(
                    archive,
                    frontendPath,
                    Path.Combine(ConstVal.FrontendDir, frontendModuleName)
                );

                var frontendSharePath = Path.Combine(
                    Directory.GetParent(frontendPath)!.FullName,
                    "share"
                );
                AddDirectoryToArchive(
                    archive,
                    frontendSharePath,
                    Path.Combine(ConstVal.FrontendDir, "share")
                );
            }
        }

        OutputHelper.Success(_localizer.Get(Localizer.PackageCreated, packagePath));
        return packagePath;
    }

    private string? GetFrontendPath(string? frontPath)
    {
        if (string.IsNullOrWhiteSpace(frontPath))
        {
            return null;
        }

        var resolvedPath = Path.IsPathRooted(frontPath)
            ? Path.GetFullPath(frontPath)
            : Path.GetFullPath(Path.Combine(_projectContext.SolutionPath!, frontPath));

        if (Directory.Exists(resolvedPath))
        {
            return resolvedPath;
        }

        OutputHelper.Error(_localizer.Get(Localizer.FrontendPathNotFound, frontPath));
        return null;
    }

    private static async Task<string> GetFrontendTypeAsync(string frontendPath)
    {
        var packageJsonPath = FindFrontendRoot(frontendPath);
        if (packageJsonPath == null)
        {
            return "Else";
        }

        await using var stream = File.OpenRead(packageJsonPath);
        using var packageJson = await JsonDocument.ParseAsync(stream);
        var packageNames = packageJson.RootElement
            .EnumerateObject()
            .Where(property => property.Name is "dependencies" or "devDependencies")
            .Where(property => property.Value.ValueKind == JsonValueKind.Object)
            .SelectMany(property => property.Value.EnumerateObject())
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (packageNames.Contains("@angular/core"))
        {
            return "Angular";
        }

        if (packageNames.Contains("react") || packageNames.Contains("react-dom"))
        {
            return "React";
        }

        return packageNames.Contains("vue") ? "Vue" : "Else";
    }

    private static string? FindFrontendRoot(string frontendPath)
    {
        var directory = new DirectoryInfo(frontendPath);
        while (directory != null)
        {
            var packageJsonPath = Path.Combine(directory.FullName, ConstVal.NodeProjectFile);
            if (File.Exists(packageJsonPath))
            {
                return packageJsonPath;
            }

            directory = directory.Parent;
        }

        return null;
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
        if (!Directory.Exists(sourceDir))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);

            // Skip files that are under 'bin' or 'obj' folders
            var segments = relativePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(s => string.Equals(s, "bin", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var entryName = Path.Combine(entryPrefix, relativePath).Replace("\\", "/");

            archive.CreateEntryFromFile(file, entryName);
        }
    }
}
