using System.IO.Compression;
using System.Text.Json;
using Share.Helper;
using Share.Models;

namespace Share.Services;

/// <summary>
/// Module installation service
/// </summary>
public class ModuleInstallService(
    IProjectContext projectContext,
    Localizer localizer,
    ILogger<ModuleInstallService> logger
)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly Localizer _localizer = localizer;
    private readonly ILogger<ModuleInstallService> _logger = logger;

    /// <summary>
    /// Install a module package
    /// </summary>
    /// <param name="packagePath">Path to the package zip file</param>
    /// <param name="serviceName">Service name to install controllers</param>
    /// <returns>True if installation succeeded</returns>
    public async Task<bool> InstallModuleAsync(string packagePath, string serviceName)
    {
        try
        {
            // Validate package exists
            if (!File.Exists(packagePath))
            {
                OutputHelper.Error(_localizer.Get(Localizer.PackageFileNotFound, packagePath));
                return false;
            }

            // Validate service exists
            var servicePath = Path.Combine(_projectContext.ServicesPath!, serviceName);
            if (!Directory.Exists(servicePath))
            {
                OutputHelper.Error(_localizer.Get(Localizer.ServiceNotFound, serviceName));
                return false;
            }

            // Extract and validate package
            var metadata = await ExtractPackageAsync(packagePath, serviceName);
            if (metadata == null)
            {
                return false;
            }

            OutputHelper.Success(_localizer.Get(Localizer.PackageInstalled, metadata.ModuleName));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing module from {PackagePath}", packagePath);
            OutputHelper.Error($"Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Extract package and install files
    /// </summary>
    private async Task<ModulePackageMetadata?> ExtractPackageAsync(
        string packagePath,
        string serviceName
    )
    {
        // Create temp directory for extraction
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Extract package
            ZipFile.ExtractToDirectory(packagePath, tempDir);

            // Read metadata
            var metadataPath = Path.Combine(tempDir, "metadata.json");
            if (!File.Exists(metadataPath))
            {
                OutputHelper.Error(_localizer.Get(Localizer.InvalidPackageStructure));
                return null;
            }

            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<ModulePackageMetadata>(metadataJson);
            if (metadata == null)
            {
                OutputHelper.Error(_localizer.Get(Localizer.InvalidPackageStructure));
                return null;
            }

            // Install Entity files
            var entitySourceDir = Path.Combine(tempDir, "Entity", metadata.ModuleName);
            if (Directory.Exists(entitySourceDir))
            {
                var entityTargetDir = Path.Combine(
                    _projectContext.EntityPath!,
                    metadata.ModuleName
                );
                await CopyDirectoryAsync(entitySourceDir, entityTargetDir);
            }

            // Install Module files
            var moduleSourceDir = Path.Combine(tempDir, "Module", metadata.ModuleName);
            if (Directory.Exists(moduleSourceDir))
            {
                var moduleTargetDir = Path.Combine(
                    _projectContext.ModulesPath!,
                    metadata.ModuleName
                );
                await CopyDirectoryAsync(moduleSourceDir, moduleTargetDir);
            }

            // Install Controller files
            var controllerSourceDir = Path.Combine(tempDir, "Controller", metadata.ModuleName);
            if (Directory.Exists(controllerSourceDir))
            {
                var controllerTargetDir = Path.Combine(
                    _projectContext.ServicesPath!,
                    serviceName,
                    ConstVal.ControllersDir,
                    metadata.ModuleName
                );
                await CopyDirectoryAsync(controllerSourceDir, controllerTargetDir);
            }

            return metadata;
        }
        finally
        {
            // Clean up temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// Copy directory recursively
    /// </summary>
    private async Task CopyDirectoryAsync(string sourceDir, string targetDir)
    {
        // Create target directory
        Directory.CreateDirectory(targetDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetPath = Path.Combine(targetDir, relativePath);

            // Create subdirectories if needed
            var targetFileDir = Path.GetDirectoryName(targetPath);
            if (targetFileDir != null && !Directory.Exists(targetFileDir))
            {
                Directory.CreateDirectory(targetFileDir);
            }

            // Copy file
            File.Copy(file, targetPath, overwrite: true);
        }

        await Task.CompletedTask;
    }
}
