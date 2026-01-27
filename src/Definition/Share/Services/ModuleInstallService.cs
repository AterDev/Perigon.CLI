using CodeGenerator.Helper;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO.Compression;

namespace Share.Services;

/// <summary>
/// Module installation service
/// </summary>
public class ModuleInstallService(
    IProjectContext projectContext,
    Localizer localizer,
    ILogger<ModuleInstallService> logger,
    SolutionService solutionService
)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly Localizer _localizer = localizer;
    private readonly ILogger<ModuleInstallService> _logger = logger;
    private readonly SolutionService _solutionService = solutionService;

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
                OutputHelper.Error("metadata is null");
                return false;
            }

            // Post-install adjustments (solution, project refs, global usings, DbContext)
            await PostInstallAdjustmentsAsync(metadata, serviceName);

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
    /// Handle solution/project/global usings updates after files are copied
    /// </summary>
    private async Task PostInstallAdjustmentsAsync(PackageMetadata metadata, string serviceName)
    {

        // Compose module project path
        var moduleProjectPath = Path.Combine(
            _projectContext.ModulesPath!,
            metadata.ModuleName,
            metadata.ModuleName + ConstVal.CSharpProjectExtension
        );

        // add module project to solution
        try
        {
            _solutionService.UpdateSolutionFile(moduleProjectPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update solution file for module {Module}", metadata.ModuleName);
        }

        // Add project reference from target service to module
        var servicePath = Path.Combine(_projectContext.ServicesPath!, serviceName);
        var serviceProjectPath = Directory
            .GetFiles(servicePath, $"*{ConstVal.CSharpProjectExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(serviceProjectPath))
        {
            try
            {
                // Ensure module project exists before adding reference
                if (File.Exists(moduleProjectPath))
                {
                    await SolutionService.AddProjectReferenceAsync(serviceProjectPath, moduleProjectPath);
                }
                else
                {
                    _logger.LogWarning("Module project file not found: {ModuleProjectPath}", moduleProjectPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add project reference from {Service} to {Module}", serviceName, metadata.ModuleName);
            }
            // use self services
            await SetSelfServices(metadata, servicePath);

            // Update service GlobalUsings.cs
            var globalUsingsPath = Path.Combine(servicePath, ConstVal.GlobalUsingsFile);
            var existingLines = new List<string>();
            if (File.Exists(globalUsingsPath))
            {
                existingLines = File.ReadAllLines(globalUsingsPath).ToList();
            }
            else
            {
                // Create parent directory if needed
                var serviceDir = Path.GetDirectoryName(globalUsingsPath);
                if (!string.IsNullOrEmpty(serviceDir) && !Directory.Exists(serviceDir))
                {
                    Directory.CreateDirectory(serviceDir);
                }
            }

            // Prepare namespaces to add
            var managerNs = $"global using {metadata.ModuleName}.{ConstVal.ManagersDir};";
            var entityNs = $"global using {ConstVal.EntityName}.{metadata.ModuleName};";

            // Models: examine actual Models subdirectories and files for the specific module
            var modelsUsings = new List<string>();
            var modelsDir = Path.Combine(_projectContext.ModulesPath!, metadata.ModuleName, ConstVal.ModelsDir);
            if (Directory.Exists(modelsDir))
            {
                // Add namespaces for immediate subdirectories under Models
                var subdirs = Directory.GetDirectories(modelsDir, "*", SearchOption.TopDirectoryOnly)
                    .Select(d => Path.GetFileName(d)!)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                if (subdirs.Count > 0)
                {
                    foreach (var sub in subdirs)
                    {
                        modelsUsings.Add($"global using {metadata.ModuleName}.{ConstVal.ModelsDir}.{sub};");
                    }
                }

                // Also add base Models namespace if there are .cs files directly under Models
                var directModelFiles = Directory.GetFiles(modelsDir, "*.cs", SearchOption.TopDirectoryOnly);
                if (directModelFiles.Length > 0 || subdirs.Count == 0)
                {
                    modelsUsings.Add($"global using {metadata.ModuleName}.{ConstVal.ModelsDir};");
                }
            }

            var toAdd = new List<string> { managerNs, entityNs };
            toAdd.AddRange(modelsUsings);

            var changed = false;
            foreach (var ns in toAdd)
            {
                if (!existingLines.Any(l => l.Trim().Equals(ns, StringComparison.OrdinalIgnoreCase)))
                {
                    existingLines.Add(ns);
                    changed = true;
                }
            }

            if (changed)
            {
                await File.WriteAllLinesAsync(globalUsingsPath, existingLines);
            }
        }

        // Ensure DefaultDbContext has DbSet for new entities
        await EnsureDbSetsForModuleEntitiesAsync(metadata);
    }

    private async Task SetSelfServices(PackageMetadata metadata, string servicePath)
    {
        if (metadata.UseSelfServices)
        {
            var programPath = Path.Combine(servicePath, "Program.cs");
            try
            {
                var content = await File.ReadAllTextAsync(programPath);
                var tree = CSharpSyntaxTree.ParseText(content);
                var root = await tree.GetRootAsync();

                // 查找并移除 builder.AddMiddlewareServices() 和 app.UseMiddlewareServices()
                var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
                var nodesToRemove = new List<SyntaxNode>();

                foreach (var invocation in invocations)
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var methodName = memberAccess.Name.Identifier.Text;
                        if (methodName == "AddMiddlewareServices" || methodName == "UseMiddlewareServices")
                        {
                            // 找到包含该调用的完整语句
                            var statement = invocation.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                            if (statement != null && !nodesToRemove.Contains(statement))
                            {
                                nodesToRemove.Add(statement);
                            }
                        }
                    }
                }

                // 移除找到的节点
                var newRoot = root;
                foreach (var node in nodesToRemove)
                {
                    newRoot = newRoot.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia)!;
                }

                // 查找 builder.Build() 调用
                var buildInvocation = newRoot.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .FirstOrDefault(inv =>
                    {
                        if (inv.Expression is MemberAccessExpressionSyntax ma)
                        {
                            return ma.Name.Identifier.Text == "Build" &&
                                   ma.Expression is IdentifierNameSyntax id &&
                                   id.Identifier.Text == "builder";
                        }
                        return false;
                    });

                if (buildInvocation != null)
                {
                    // 找到包含 Build() 调用的语句
                    var buildStatement = buildInvocation.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                    if (buildStatement != null)
                    {
                        // 创建新的调用语句: app.Use{ModuleName}Services();
                        var serviceCallStatement = SyntaxFactory.ParseStatement($"app.Use{metadata.ModuleName}Services();")
                            .WithLeadingTrivia(buildStatement.GetLeadingTrivia())
                            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                        // 在 buildStatement 之后插入新语句
                        newRoot = newRoot.InsertNodesAfter(buildStatement, new[] { serviceCallStatement })!;
                    }
                }

                // 如果有修改，保存文件
                if (newRoot != root)
                {
                    var newContent = newRoot.NormalizeWhitespace().ToFullString();
                    await File.WriteAllTextAsync(programPath, newContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update Program.cs for self services of module {Module}", metadata.ModuleName);
            }
        }
    }

    /// <summary>
    /// Ensure DefaultDbContext contains DbSet properties for entities of the module
    /// </summary>
    private async Task EnsureDbSetsForModuleEntitiesAsync(PackageMetadata metadata)
    {
        OutputHelper.Debug("Ensuring DbSet properties for module entities...");
        try
        {
            // Locate entity directory for the module
            var entityDir = Path.Combine(_projectContext.EntityPath!, metadata.ModuleName);
            if (!Directory.Exists(entityDir))
            {

                OutputHelper.Warning($"Entity directory not found for module {metadata.ModuleName}");
                return;
            }

            // Collect entity class names from .cs files
            var entityFiles = Directory.GetFiles(entityDir, "*.cs", SearchOption.AllDirectories);
            var entityNames = new List<string>();

            OutputHelper.Debug($"Found {entityFiles.Length} entity files for module {metadata.ModuleName}");
            foreach (var ef in entityFiles)
            {
                try
                {
                    var text = await File.ReadAllTextAsync(ef);
                    var tree = CSharpSyntaxTree.ParseText(text);
                    var root = await tree.GetRootAsync();
                    var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    if (classNode != null)
                    {
                        var name = classNode.Identifier.Text;
                        if (!string.IsNullOrWhiteSpace(name) && !entityNames.Contains(name))
                        {
                            entityNames.Add(name);
                        }
                    }
                }
                catch
                {
                    OutputHelper.Debug($"Failed to parse entity file: {ef}");
                    // ignore parse errors for individual files
                }
            }

            if (entityNames.Count == 0)
            {
                OutputHelper.Warning($"No entity classes found for module {metadata.ModuleName}");
                return;
            }

            // Locate DefaultDbContext.cs
            string? dbContextPath = null;

            // Preferred location: solutionPath + PathConst.EntityFrameworkPath + ConstVal.AppDbContextName
            var candidate = Path.Combine(
                _projectContext.SolutionPath!,
                PathConst.EntityFrameworkPath,
                ConstVal.AppDbContextName,
                "DefaultDbContext.cs"
            );

            if (File.Exists(candidate))
            {
                dbContextPath = candidate;
            }
            else
            {
                // fallback: search the solution for DefaultDbContext.cs
                var found = Directory.GetFiles(_projectContext.SolutionPath!, "DefaultDbContext.cs", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(found))
                {
                    dbContextPath = found;
                }
            }

            OutputHelper.Debug($"{dbContextPath}");

            if (string.IsNullOrEmpty(dbContextPath) || !File.Exists(dbContextPath))
            {

                OutputHelper.Warning("DefaultDbContext.cs not found");
                return;
            }

            try
            {
                // Use CompilationHelper to modify the DbContext class
                var helper = new CompilationHelper(_projectContext.SolutionPath!);
                var dbText = await File.ReadAllTextAsync(dbContextPath);
                helper.LoadContent(dbText);

                bool dbChanged = false;
                foreach (var entity in entityNames)
                {
                    // property name: pluralized entity name
                    var propName = entity.Pluralize();
                    if (!helper.PropertyExist(propName))
                    {
                        var propContent = $"public DbSet<{entity}> {propName} {{ get; set; }}";
                        helper.AddClassProperty(propContent);

                        OutputHelper.Debug($"add  prop {propContent}");
                        dbChanged = true;
                    }
                }

                if (dbChanged && helper.SyntaxRoot != null)
                {
                    var newDbText = helper.SyntaxRoot.NormalizeWhitespace().ToFullString();
                    OutputHelper.Debug($"{newDbText}");
                    await File.WriteAllTextAsync(dbContextPath, newDbText);
                }
            }
            catch (Exception ex)
            {
                OutputHelper.Error($"Error updating DefaultDbContext: {ex.Message}");
                _logger.LogWarning(ex, "Failed to update DefaultDbContext with new DbSet properties for module {Module}", metadata.ModuleName);
            }

            // Also ensure EntityFramework GlobalUsings contains Entity.{ModuleName}
            try
            {
                var efGlobalUsingsPath = Path.Combine(
                    _projectContext.SolutionPath!,
                    PathConst.EntityFrameworkPath,
                    ConstVal.GlobalUsingsFile
                );

                var usingLine = $"global using {ConstVal.EntityName}.{metadata.ModuleName};";
                List<string> efLines = [];
                if (File.Exists(efGlobalUsingsPath))
                {
                    efLines = File.ReadAllLines(efGlobalUsingsPath).ToList();
                }
                if (!efLines.Any(l => l.Trim().Equals(usingLine, StringComparison.OrdinalIgnoreCase)))
                {
                    efLines.Add(usingLine);
                    await File.WriteAllLinesAsync(efGlobalUsingsPath, efLines);
                }
            }
            catch (Exception ex)
            {
                OutputHelper.Error($"Error updating EntityFramework GlobalUsings: {ex.Message}");
                _logger.LogWarning(ex, "Failed to update EntityFramework GlobalUsings for module {Module}", metadata.ModuleName);
            }
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"Error ensuring DbSet properties: {ex.Message}");
            _logger.LogWarning(ex, "Error while adding DbSet properties for module {Module}", metadata.ModuleName);
        }
    }

    /// <summary>
    /// Extract package and install files
    /// </summary>
    private async Task<PackageMetadata?> ExtractPackageAsync(
        string packagePath,
        string serviceName
    )
    {
        // Create temp directory for extraction
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Extract package with zip slip protection
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Validate entry path to prevent zip slip attacks
                    var entryPath = Path.GetFullPath(Path.Combine(tempDir, entry.FullName));
                    if (!entryPath.StartsWith(tempDir, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Zip entry '{entry.FullName}' attempts to extract outside temp directory"
                        );
                    }

                    // Extract entry
                    if (entry.FullName.EndsWith("/"))
                    {
                        // Directory entry
                        Directory.CreateDirectory(entryPath);
                    }
                    else
                    {
                        // File entry
                        var entryDir = Path.GetDirectoryName(entryPath);
                        if (entryDir != null)
                        {
                            Directory.CreateDirectory(entryDir);
                        }
                        entry.ExtractToFile(entryPath, overwrite: true);
                    }
                }
            }

            // Read metadata
            var metadataPath = Path.Combine(tempDir, "metadata.json");
            if (!File.Exists(metadataPath))
            {
                OutputHelper.Error(_localizer.Get(Localizer.InvalidPackageStructure));
                return null;
            }

            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<PackageMetadata>(metadataJson);
            if (metadata == null)
            {
                OutputHelper.Error(_localizer.Get(Localizer.InvalidPackageStructure));
                return null;
            }

            // Validate ModuleName to prevent path traversal
            if (string.IsNullOrWhiteSpace(metadata.ModuleName) ||
                metadata.ModuleName.Contains("..") ||
                metadata.ModuleName.Contains("/") ||
                metadata.ModuleName.Contains("\\") ||
                Path.GetInvalidFileNameChars().Any(c => metadata.ModuleName.Contains(c)))
            {
                OutputHelper.Error("Invalid module name in package metadata");
                return null;
            }

            // Install Entity files
            var entitySourceDir = Path.Combine(tempDir, ConstVal.EntityName, metadata.ModuleName);
            if (Directory.Exists(entitySourceDir))
            {
                var entityTargetDir = Path.Combine(
                    _projectContext.EntityPath!,
                    metadata.ModuleName
                );
                CopyDirectory(entitySourceDir, entityTargetDir);
            }

            // Install Module files
            var moduleSourceDir = Path.Combine(tempDir, ConstVal.ModulesDir, metadata.ModuleName);
            if (Directory.Exists(moduleSourceDir))
            {
                var moduleTargetDir = Path.Combine(
                    _projectContext.ModulesPath!,
                    metadata.ModuleName
                );
                CopyDirectory(moduleSourceDir, moduleTargetDir);
            }

            // Install Controller files
            var controllerSourceDir = Path.Combine(tempDir, ConstVal.ControllersDir, metadata.ModuleName);
            if (Directory.Exists(controllerSourceDir))
            {
                var controllerTargetDir = Path.Combine(
                    _projectContext.ServicesPath!,
                    serviceName,
                    ConstVal.ControllersDir,
                    metadata.ModuleName
                );
                CopyDirectory(controllerSourceDir, controllerTargetDir);
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
    private void CopyDirectory(string sourceDir, string targetDir)
    {
        // Create target directory
        Directory.CreateDirectory(targetDir);

        // Validate target directory is within expected bounds
        var fullTargetDir = Path.GetFullPath(targetDir);
        var solutionPath = Path.GetFullPath(_projectContext.SolutionPath!);
        if (!fullTargetDir.StartsWith(solutionPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Target directory is outside solution path");
        }

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetPath = Path.Combine(targetDir, relativePath);

            // Validate target path to prevent directory traversal
            var fullTargetPath = Path.GetFullPath(targetPath);
            if (!fullTargetPath.StartsWith(fullTargetDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Target path is outside target directory");
            }

            // Create subdirectories if needed
            var targetFileDir = Path.GetDirectoryName(targetPath);
            if (targetFileDir != null && !Directory.Exists(targetFileDir))
            {
                Directory.CreateDirectory(targetFileDir);
            }

            // Copy file
            File.Copy(file, targetPath, overwrite: true);
        }
    }
}
