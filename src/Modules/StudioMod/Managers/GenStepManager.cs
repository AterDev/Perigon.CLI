using StudioMod.Models.GenStepDtos;

namespace StudioMod.Managers;

/// <summary>
/// task step
/// </summary>
public class GenStepManager(
    DefaultDbContext dbContext,
    ILogger<GenStepManager> logger,
    IProjectContext projectContext
) : ManagerBase<DefaultDbContext, GenStep>(dbContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;

    public sealed record UpsertFromTemplatesResult(int Added, int Updated, int Skipped);

    public async Task<UpsertFromTemplatesResult> UpsertStepsFromTemplatesAsync(
        IEnumerable<DataFile> templates,
        string? templateDirectoryName,
        int solutionId,
        string? relativeOutputDir
    )
    {
        ArgumentNullException.ThrowIfNull(templates);


        var outputDir = (relativeOutputDir ?? string.Empty).Trim();
        outputDir = outputDir.Replace('\\', '/').TrimStart('/');

        var templatePaths = templates
            .Where(t => !string.IsNullOrWhiteSpace(t.FullPath))
            .Select(t => t.FullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (templatePaths.Count == 0)
            return new UpsertFromTemplatesResult(0, 0, 0);

        var existing = Queryable
            .Where(s => s.ProjectId == solutionId && s.TemplatePath != null)
            .Where(s => templatePaths.Contains(s.TemplatePath!))
            .ToList();

        var byTemplatePath = existing.ToDictionary(s => s.TemplatePath!, StringComparer.OrdinalIgnoreCase);

        int added = 0;
        int updated = 0;
        int skipped = 0;

        foreach (var tpl in templates)
        {
            if (string.IsNullOrWhiteSpace(tpl.FullPath))
            {
                skipped++;
                continue;
            }

            if (byTemplatePath.ContainsKey(tpl.FullPath))
            {
                skipped++;
                continue;
            }

            var templateBaseName = Path.GetFileNameWithoutExtension(tpl.Name);
            var stepName = BuildStepName(templateDirectoryName, templateBaseName);

            var outputPath = Path.Combine(outputDir, templateBaseName);

            var newStep = new GenStep
            {
                Name = stepName,
                TemplatePath = tpl.FullPath,
                OutputPath = outputPath,
                ProjectId = solutionId,
            };
            var addRes = await AddAsync(newStep);
            if (!addRes)
                return new UpsertFromTemplatesResult(added, updated, skipped);

            added++;
        }

        return new UpsertFromTemplatesResult(added, updated, skipped);
    }

    private static string BuildStepName(string? directoryName, string templateBaseName)
    {
        var raw = string.IsNullOrWhiteSpace(directoryName)
            ? templateBaseName
            : $"{directoryName}-{templateBaseName}";

        raw = raw.Replace(".razor", string.Empty, StringComparison.OrdinalIgnoreCase);
        raw = raw.Replace('.', '-');
        return raw;
    }


    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(GenStep entity, GenStepUpdateDto dto)
    {
        entity.Merge(dto);
        var fileExt = Path.GetExtension(dto.OutputPath ?? "");
        entity.FileType = fileExt;
        return await base.UpdateAsync(entity);
    }

    public async Task<PageList<GenStepItemDto>> ToPageAsync(GenStepFilterDto filter)
    {
        Queryable = Queryable.Where(q => q.ProjectId == _projectContext.SolutionId);

        if (!string.IsNullOrEmpty(filter.Name))
        {
            Queryable = Queryable.Where(q => q.Name.Contains(filter.Name));
        }

        if (!string.IsNullOrEmpty(filter.FileType))
        {
            Queryable = Queryable.Where(q => q.FileType == filter.FileType);
        }
        return await ToPageAsync<GenStepFilterDto, GenStepItemDto>(filter);
    }

    /// <summary>
    /// 获取实体详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenStepDetailDto?> GetDetailAsync(int id)
    {
        return await FindAsync<GenStepDetailDto>(e => e.Id == id);
    }

    /// <summary>
    /// TODO:唯一性判断
    /// </summary>
    /// <param name="unique">唯一标识</param>
    /// <param name="id">排除当前</param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(string unique, int? id = null)
    {
        // 自定义唯一性验证参数和逻辑
        var query = _dbSet.Where(q => q.Id.ToString() == unique);

        if (id.HasValue)
        {
            query = query.Where(q => q.Id != id.Value);
        }

        return !query.Any();
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public new async Task<bool> DeleteAsync(List<int> ids, bool softDelete = true)
    {
        // remove relation
        var relations = _dbContext.GenActionGenSteps
            .Where(q => ids.Contains(q.GenStepId))
            .ToList();

        foreach (var relation in relations)
        {
            _dbContext.GenActionGenSteps.Remove(relation);
        }
        return await base.DeleteAsync(ids, softDelete);
    }
}
