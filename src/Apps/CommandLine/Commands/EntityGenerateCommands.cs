using CoreMod.Managers;
using Share;
using Share.Helper;
using Share.Models;

namespace CommandLine.Commands;

public class EntityGenerateSettings
{
    public required string EntityPath { get; set; }

    public bool Force { get; set; }
}

public sealed class GenerateDtoCommand(
    EntityInfoManager entityInfoManager,
    SolutionContext projectContext,
    Localizer localizer
) : EntityGenerateCommandBase<EntityGenerateSettings>(entityInfoManager, projectContext, localizer)
{
    protected override CommandType CommandType => CommandType.Dto;
}

public sealed class GenerateManagerCommand(
    EntityInfoManager entityInfoManager,
    SolutionContext projectContext,
    Localizer localizer
) : EntityGenerateCommandBase<EntityGenerateSettings>(entityInfoManager, projectContext, localizer)
{
    protected override CommandType CommandType => CommandType.Manager;
}

public sealed class GenerateControllerSettings : EntityGenerateSettings
{
    public required string ServicePath { get; set; }
}

public sealed class GenerateControllerCommand(
    EntityInfoManager entityInfoManager,
    SolutionContext projectContext,
    Localizer localizer
) : EntityGenerateCommandBase<GenerateControllerSettings>(entityInfoManager, projectContext, localizer)
{
    protected override CommandType CommandType => CommandType.API;

    protected override string[] ResolveServicePaths(GenerateControllerSettings settings)
    {
        var servicePath = ResolveServicePath(settings.ServicePath);
        if (servicePath == null)
        {
            throw new DirectoryNotFoundException(
                _localizer.Get(Localizer.ServiceNotFound, settings.ServicePath)
            );
        }

        return [servicePath];
    }

    private string? ResolveServicePath(string value)
    {
        var candidates = new List<string>();

        void AddCandidate(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!candidates.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(path);
            }
        }

        var looksLikePath = Path.IsPathRooted(value) || value.Contains('\\') || value.Contains('/');

        if (value.EndsWith(ConstVal.CSharpProjectExtension, StringComparison.OrdinalIgnoreCase))
        {
            var projectPath = looksLikePath
                ? Path.GetFullPath(value)
                : Path.Combine(_projectContext.ServicesPath ?? string.Empty, value);
            AddCandidate(Path.GetDirectoryName(projectPath));
        }

        if (looksLikePath)
        {
            AddCandidate(Path.GetFullPath(value));
        }

        if (!string.IsNullOrWhiteSpace(_projectContext.ServicesPath))
        {
            AddCandidate(Path.Combine(_projectContext.ServicesPath, value));
            AddCandidate(
                Path.Combine(
                    _projectContext.ServicesPath,
                    Path.GetFileNameWithoutExtension(value)
                )
            );
        }

        return candidates.FirstOrDefault(Directory.Exists);
    }
}

public abstract class EntityGenerateCommandBase<TSettings>(
    EntityInfoManager entityInfoManager,
    SolutionContext projectContext,
    Localizer localizer
)
    where TSettings : EntityGenerateSettings
{
    protected readonly Localizer _localizer = localizer;
    protected readonly SolutionContext _projectContext = projectContext;

    private readonly EntityInfoManager _entityInfoManager = entityInfoManager;

    protected abstract CommandType CommandType { get; }

    public async Task<int> ExecuteAsync(TSettings settings, CancellationToken cancellationToken)
    {
        if (!CommandSolutionHelper.TrySetSolution(_projectContext, _localizer))
        {
            return 1;
        }

        var entityPath = Path.GetFullPath(settings.EntityPath);
        if (!File.Exists(entityPath))
        {
            OutputHelper.Error(_localizer.Get(Localizer.NotFoundWithName, settings.EntityPath));
            return 1;
        }

        try
        {
            await _entityInfoManager.GenerateAsync(
                new GenerateDto
                {
                    EntityPath = entityPath,
                    CommandType = CommandType,
                    Force = settings.Force,
                    ServicePath = ResolveServicePaths(settings)
                }
            );

            return 0;
        }
        catch (Exception ex)
        {
            OutputHelper.Error(ex.Message);
            return 1;
        }
    }

    protected virtual string[] ResolveServicePaths(TSettings settings)
    {
        return [];
    }
}
