using CodeGenerator;
using DataContext.DBProvider;
using Mapster;
using Microsoft.OpenApi;
using StudioMod.Models.GenActionDtos;
using StudioMod.Models.GenStepDtos;

namespace StudioMod.Managers;

/// <summary>
/// The project's generate action
/// </summary>
public class GenActionManager(
    DefaultDbContext dbContext,
    CodeGenService codeGenService,
    ILogger<GenActionManager> logger,
    IProjectContext projectContext,
    CodeAnalysisService codeAnalysis
) : ManagerBase<DefaultDbContext, GenAction>(dbContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CodeGenService _codeGen = codeGenService;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;

    protected override ICollection<GenAction> GetCollection() => _dbContext.GenActions;

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<int?> CreateNewEntityAsync(GenActionAddDto dto)
    {
        var entity = dto.MapTo<GenAction>();
        entity.ProjectId = (int)_projectContext.SolutionId!.Value.GetHashCode();
        return await AddAsync(entity) ? entity.Id : null;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(GenAction entity, GenActionUpdateDto dto)
    {
        entity.Merge(dto);
        // TODO:完善更新逻辑
        return await UpdateAsync(entity);
    }

    public async Task<PageList<GenActionItemDto>> ToPageAsync(GenActionFilterDto filter)
    {
        var query = Queryable;
        
        if (!string.IsNullOrEmpty(filter.Name))
        {
            query = query.Where(q => q.Name.ToLower().Contains(filter.Name.Trim().ToLower()));
        }
        
        if (filter.SourceType.HasValue)
        {
            query = query.Where(q => q.SourceType == filter.SourceType);
        }
        
        if (filter.ProjectId.HasValue)
        {
            query = query.Where(q => q.ProjectId == (int)filter.ProjectId.Value.GetHashCode());
        }

        Queryable = query;
        return await ToPageAsync<GenActionFilterDto, GenActionItemDto>(filter);
    }

    /// <summary>
    /// 获取步骤 - 需要手动查询，因为不支持Include
    /// </summary>
    /// <param name="actionId"></param>
    /// <returns></returns>
    public async Task<List<GenStepItemDto>> GetStepsAsync(int actionId)
    {
        var actionSteps = _dbContext.GenActionGenSteps
            .Where(gs => gs.GenActionId == actionId)
            .ToList();
        
        var stepIds = actionSteps.Select(gs => gs.GenStepId).ToList();
        var steps = _dbContext.GenSteps
            .Where(s => stepIds.Contains(s.Id))
            .ToList();
        
        return steps.Adapt<List<GenStepItemDto>>();
    }

    /// <summary>
    /// 获取实体详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenActionDetailDto?> GetDetailAsync(int id)
    {
        return await FindAsync<GenActionDetailDto>(e => e.Id == id);
    }

    /// <summary>
    /// 唯一性判断
    /// </summary>
    /// <param name="name">唯一标识</param>
    /// <param name="id">排除当前</param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(string name, int? id = null)
    {
        var exists = GetCollection().Any(q => q.Name == name && (id == null || q.Id != id));
        return !exists;
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public new async Task<bool> DeleteAsync(List<int> ids, bool softDelete = true)
    {
        return await base.DeleteAsync(ids, softDelete);
    }

    /// <summary>
    /// 数据权限验证
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenAction?> GetOwnedAsync(int id)
    {
        var query = GetCollection().Where(q => q.Id == id);
        // TODO:自定义数据权限验证
        // query = query.Where(q => q.User.Id == _userContext.UserIdKeys);
        return query.FirstOrDefault();
    }

    /// <summary>
    /// 添加步骤
    /// </summary>
    /// <param name="id"></param>
    /// <param name="stepIds"></param>
    /// <returns></returns>
    public async Task<bool> AddStepsAsync(int id, List<int> stepIds)
    {
        try
        {
            var existingSteps = _dbContext.GenActionGenSteps
                .Where(q => q.GenActionId == id)
                .ToList();
            
            foreach (var step in existingSteps)
            {
                _dbContext.GenActionGenSteps.Remove(step);
            }

            var actionSteps = stepIds.Select(q => new GenActionGenStep
            {
                GenActionId = id,
                GenStepId = q,
            });
            
            foreach (var step in actionSteps)
            {
                _dbContext.GenActionGenSteps.Add(step);
            }
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Add steps failed");
            return false;
        }
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<GenActionResultDto> ExecuteActionAsync(GenActionRunDto dto)
    {
        var res = new GenActionResultDto();
        var action = await GetCurrentAsync(dto.Id);
        if (action == null)
        {
            res.Success = false;
            res.Message = $"Action {dto.Id} not found";
            return res;
        }

        action.ActionStatus = ActionStatus.InProgress;
        await UpdateAsync(action);

        // 构建任务执行需要的内容
        var variables = action.Variables;
        if (dto.Variables != null)
        {
            variables = variables
                .Concat(dto.Variables)
                .DistinctBy(v => v.Key)
                .ToList();
        }
        var actionRunModel = new ActionRunModel { Variables = [.. variables] };

        // 解析模型
        if (action.SourceType is GenSourceType.EntityClass or GenSourceType.DtoModel)
        {
            // 兼容dto名称
            if (action.SourceType is GenSourceType.DtoModel && dto.ModelInfo != null)
            {
                actionRunModel.ModelName = dto.ModelInfo.Name;
                actionRunModel.PropertyInfos = dto.ModelInfo.PropertyInfos;
                actionRunModel.Description = dto.ModelInfo.CommentSummary ?? dto.ModelInfo.Comment;

                // 添加变量
                actionRunModel.Variables.Add(
                    new Variable { Key = "ModelName", Value = dto.ModelInfo.Name }
                );
                actionRunModel.Variables.Add(
                    new Variable { Key = "ModelNameHyphen", Value = dto.ModelInfo.Name.ToHyphen() }
                );
            }
            else if (dto.SourceFilePath.NotEmpty())
            {
                var entityInfo = (
                    await CodeAnalysisService.GetEntityInfosAsync([dto.SourceFilePath])
                ).FirstOrDefault();

                if (entityInfo != null)
                {
                    actionRunModel.ModelName = entityInfo.Name;
                    actionRunModel.Namespace = entityInfo.NamespaceName;
                    actionRunModel.PropertyInfos = entityInfo.PropertyInfos;
                    actionRunModel.Description = entityInfo.Summary;

                    // 添加变量
                    actionRunModel.Variables.Add(
                        new Variable { Key = "ModelName", Value = entityInfo.Name }
                    );
                    actionRunModel.Variables.Add(
                        new Variable { Key = "ModelNameHyphen", Value = entityInfo.Name.ToHyphen() }
                    );
                    // 解析dto
                    var dtoPath = _projectContext.GetDtoPath(
                        entityInfo.Name,
                        entityInfo.ModuleName
                    );
                    if (Directory.Exists(dtoPath))
                    {
                        var matchFiles = new string[]
                        {
                            "AddDto.cs",
                            "UpdateDto.cs",
                            "DetailDto.cs",
                            "ItemDto.cs",
                            "FilterDto.cs",
                        };

                        var dtoFiles = Directory
                            .GetFiles(dtoPath, "*Dto.cs", SearchOption.AllDirectories)
                            .Where(q => matchFiles.Any(m => Path.GetFileName(q).EndsWith(m)))
                            .ToList();

                        var dtoInfos = await CodeAnalysisService.GetEntityInfosAsync(dtoFiles);

                        actionRunModel.AddPropertyInfos =
                            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("AddDto"))?.PropertyInfos
                            ?? [];

                        actionRunModel.UpdatePropertyInfos =
                            dtoInfos
                                .FirstOrDefault(q => q.Name.EndsWith("UpdateDto"))
                                ?.PropertyInfos ?? [];

                        actionRunModel.DetailPropertyInfos =
                            dtoInfos
                                .FirstOrDefault(q => q.Name.EndsWith("DetailDto"))
                                ?.PropertyInfos ?? [];

                        actionRunModel.ItemPropertyInfos =
                            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("ItemDto"))?.PropertyInfos
                            ?? [];

                        actionRunModel.FilterPropertyInfos =
                            dtoInfos
                                .FirstOrDefault(q => q.Name.EndsWith("FilterDto"))
                                ?.PropertyInfos ?? [];
                    }
                }
            }
        }


        if (action.SourceType is GenSourceType.OpenAPI && dto.SourceFilePath != null)
        {
            var (apiDocument, _) = await OpenApiDocument.LoadAsync(dto.SourceFilePath);
            actionRunModel.OpenApiPaths = apiDocument?.Paths ?? [];

        }
        if (action.GenSteps.Count > 0)
        {
            try
            {
                foreach (var step in action.GenSteps)
                {
                    var content = string.Empty;
                    var filePath = Path.Combine(
                        _projectContext.SolutionPath!,
                        step.TemplatePath ?? ""
                    );
                    if (File.Exists(filePath))
                    {
                        content = File.ReadAllText(filePath);
                    }

                    var outputContent = _codeGen.GenTemplateFile(content, actionRunModel);
                    if (step.OutputPath.NotEmpty())
                    {
                        // 处理outputPath中的变量
                        var outputPath = step.OutputPathFormat(actionRunModel.Variables);
                        outputPath = Path.Combine(_projectContext.SolutionPath!, outputPath);

                        res.OutputFiles.Add(
                             new ModelFileItemDto
                             {
                                 Name = Path.GetFileName(outputPath),
                                 FullName = outputPath,
                                 Content = outputContent,
                             }
                         );
                        if (!dto.OnlyOutput)
                        {

                            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                            }
                            File.WriteAllText(outputPath, outputContent, new UTF8Encoding(false));
                        }
                        res.IsSuccess = true;
                    }
                }
                action.ActionStatus = ActionStatus.Success;
            }
            catch (Exception ex)
            {
                action.ActionStatus = ActionStatus.Failed;
                // TODO: 记录执行情况
                logger.LogError(ex, "Execute action failed");
                res.ErrorMsg = ex.Message;
                await context.SaveChangesAsync();
                res.IsSuccess = false;
            }
        }
        else
        {
            action.ActionStatus = ActionStatus.Failed;
            await context.SaveChangesAsync();
            res.IsSuccess = false;
            res.Equals("未找到任务步骤");
        }
        await context.SaveChangesAsync();
        return res;
    }

    public List<ModelFileItemDto> GetModelFile(GenSourceType sourceType)
    {
        var entityPath = _projectContext.EntityPath;
        var filePaths = CodeAnalysisService.GetEntityFilePaths(entityPath!);
        var entityFiles = new List<EntityFile>();
        if (filePaths.Count != 0)
        {
            entityFiles = CodeAnalysisService.GetEntityFiles(entityPath!, filePaths);
        }

        if (sourceType == GenSourceType.EntityClass)
        {
            return entityFiles
                .Select(q => new ModelFileItemDto { Name = q.Name, FullName = q.FullName })
                .ToList();
        }
        else if (sourceType == GenSourceType.DtoModel)
        {
            var res = new List<ModelFileItemDto>();
            foreach (var item in entityFiles)
            {
                var dtoPath = item.GetDtoPath(_projectContext);
                if (!Directory.Exists(dtoPath))
                {
                    continue;
                }
                var dtoFiles = Directory.GetFiles(dtoPath, "*Dto.cs", SearchOption.AllDirectories);

                res.AddRange(
                    dtoFiles.Select(q => new ModelFileItemDto
                    {
                        Name = Path.GetFileName(q),
                        FullName = q,
                    })
                );
            }
            return res;
        }
        return [];
    }
}
