using CodeGenerator;
using StudioMod.Models.GenActionDtos;
using StudioMod.Models.GenStepDtos;

namespace StudioMod.Managers;
/// <summary>
/// The project's generate action
/// </summary>
public class GenActionManager(
    DataAccessContext<GenAction> dataContext,
    CodeGenService codeGenService,
    ILogger<GenActionManager> logger,
    IProjectContext projectContext,
    CodeAnalysisService codeAnalysis,
    UserContext userContext) : ManagerBase<GenAction>(dataContext, logger)
{
    private readonly UserContext _userContext = userContext;
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CodeGenService _codeGen = codeGenService;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> CreateNewEntityAsync(GenActionAddDto dto)
    {
        var entity = dto.MapTo<GenActionAddDto, GenAction>();
        entity.ProjectId = _projectContext.ProjectId;
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
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name.ToLower().Contains(filter.Name!.Trim().ToLower()))
            .WhereNotNull(filter.SourceType, q => q.SourceType == filter.SourceType)
            .WhereNotNull(filter.ProjectId, q => q.ProjectId == filter.ProjectId);

        return await ToPageAsync<GenActionFilterDto, GenActionItemDto>(filter);
    }

    /// <summary>
    /// 获取步骤
    /// </summary>
    /// <param name="actionId"></param>
    /// <returns></returns>
    public async Task<List<GenStepItemDto>> GetStepsAsync(Guid actionId)
    {
        var data = await Query.Where(q => q.Id == actionId)
             .SelectMany(q => q.GenSteps)
             .ProjectTo<GenStepItemDto>()
             .ToListAsync();
        return data;
    }

    /// <summary>
    /// 获取实体详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenActionDetailDto?> GetDetailAsync(Guid id)
    {
        return await FindAsync<GenActionDetailDto>(e => e.Id == id);
    }

    /// <summary>
    /// 唯一性判断
    /// </summary>
    /// <param name="name">唯一标识</param>
    /// <param name="id">排除当前</param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(string name, Guid? id = null)
    {
        // 自定义唯一性验证参数和逻辑
        return await Command.Where(q => q.Name == name)
            .WhereNotNull(id, q => q.Id != id)
            .AnyAsync();
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public new async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        return await base.DeleteAsync(ids, softDelete);
    }

    /// <summary>
    /// 数据权限验证
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenAction?> GetOwnedAsync(Guid id)
    {
        var query = Command.Where(q => q.Id == id);
        // TODO:自定义数据权限验证
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 添加步骤
    /// </summary>
    /// <param name="id"></param>
    /// <param name="stepIds"></param>
    /// <returns></returns>
    public async Task<bool> AddStepsAsync(Guid id, List<Guid> stepIds)
    {
        await Database.BeginTransactionAsync();
        try
        {
            await CommandContext.GenActionGenSteps.Where(q => q.GenActionsId == id)
                .ExecuteDeleteAsync();

            var actionSteps = stepIds.Select(q => new GenActionGenStep
            {
                GenActionsId = id,
                GenStepsId = q
            });
            await CommandContext.GenActionGenSteps.AddRangeAsync(actionSteps);
            await SaveChangesAsync();
            await Database.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await Database.RollbackTransactionAsync();
            _logger.LogError(ex, "Add steps failed");
            return false;
        }
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenActionResultDto> ExecuteActionAsync(GenActionRunDto dto)
    {
        var res = new GenActionResultDto();
        var action = await Command.Where(a => a.Id == dto.Id)
            .Include(a => a.GenSteps)
            .SingleAsync();
        action.ActionStatus = ActionStatus.InProgress;
        await SaveChangesAsync();

        // 构建任务执行需要的内容
        var variables = action.Variables;
        if (dto.Variables != null)
        {
            variables = variables.Concat(dto.Variables)
                .DistinctBy(variables => variables.Key)
                .ToList();
        }
        var actionRunModel = new ActionRunModel
        {
            Variables = [.. variables]
        };

        // 解析模型
        if (action.SourceType is GenSourceType.EntityCLass or GenSourceType.DtoModel)
        {
            // 兼容dto名称
            if (action.SourceType is GenSourceType.DtoModel && dto.ModelInfo != null)
            {
                actionRunModel.ModelName = dto.ModelInfo.Name;
                actionRunModel.PropertyInfos = dto.ModelInfo.PropertyInfos;
                actionRunModel.Description = dto.ModelInfo.Summary ?? dto.ModelInfo.Comment;

                // 添加变量
                actionRunModel.Variables.Add(new Variable
                {
                    Key = "ModelName",
                    Value = dto.ModelInfo.Name
                });
                actionRunModel.Variables.Add(new Variable
                {
                    Key = "ModelNameHyphen",
                    Value = dto.ModelInfo.Name.ToHyphen()
                });
            }
            else if (dto.SourceFilePath.NotEmpty())
            {
                var entityInfo = _codeAnalysis.GetEntityInfos([dto.SourceFilePath]).FirstOrDefault();
                if (entityInfo != null)
                {
                    actionRunModel.ModelName = entityInfo.Name;
                    actionRunModel.Namespace = entityInfo.NamespaceName;
                    actionRunModel.PropertyInfos = entityInfo.PropertyInfos;
                    actionRunModel.Description = entityInfo.Summary;

                    // 添加变量
                    actionRunModel.Variables.Add(new Variable
                    {
                        Key = "ModelName",
                        Value = entityInfo.Name
                    });
                    actionRunModel.Variables.Add(new Variable
                    {
                        Key = "ModelNameHyphen",
                        Value = entityInfo.Name.ToHyphen()
                    });
                    // 解析dto
                    var dtoPath = _projectContext.GetDtoPath(entityInfo.Name, entityInfo.ModuleName);
                    if (Directory.Exists(dtoPath))
                    {
                        var matchFiles = new string[] { "AddDto.cs", "UpdateDto.cs", "DetailDto.cs", "ItemDto.cs", "FilterDto.cs" };

                        var dtoFiles = Directory.GetFiles(dtoPath, "*Dto.cs", SearchOption.AllDirectories)
                            .Where(q => matchFiles.Any(m => Path.GetFileName(q).EndsWith(m)))
                            .ToList();

                        var dtoInfos = _codeAnalysis.GetEntityInfos(dtoFiles);

                        actionRunModel.AddPropertyInfos = dtoInfos.FirstOrDefault(q => q.Name.EndsWith("AddDto"))?.PropertyInfos ?? [];

                        actionRunModel.UpdatePropertyInfos = dtoInfos.FirstOrDefault(q => q.Name.EndsWith("UpdateDto"))?.PropertyInfos ?? [];

                        actionRunModel.DetailPropertyInfos = dtoInfos.FirstOrDefault(q => q.Name.EndsWith("DetailDto"))?.PropertyInfos ?? [];

                        actionRunModel.ItemPropertyInfos = dtoInfos.FirstOrDefault(q => q.Name.EndsWith("ItemDto"))?.PropertyInfos ?? [];

                        actionRunModel.FilterPropertyInfos = dtoInfos.FirstOrDefault(q => q.Name.EndsWith("FilterDto"))?.PropertyInfos ?? [];
                    }
                }
            }
        }

        if (action.GenSteps.Count > 0)
        {
            try
            {
                foreach (var step in action.GenSteps)
                {
                    var content = step.Content ?? string.Empty;
                    if (step.Path.NotEmpty())
                    {
                        var filePath = Path.Combine(_projectContext.SolutionPath!, step.Path);
                        if (File.Exists(filePath))
                        {
                            content = File.ReadAllText(filePath);
                        }
                    }
                    switch (step.GenStepType)
                    {
                        case GenStepType.File:
                            var outputContent = _codeGen.GenTemplateFile(content, actionRunModel);
                            if (step.OutputPath.NotEmpty())
                            {
                                // 处理outputPath中的变量
                                var outputPath = step.OutputPathFormat(actionRunModel.Variables);
                                outputPath = Path.Combine(_projectContext.SolutionPath!, outputPath);

                                if (dto.OnlyOutput)
                                {
                                    res.OutputFiles.Add(new ModelFileItemDto
                                    {
                                        Name = Path.GetFileName(outputPath),
                                        FullName = outputPath,
                                        Content = outputContent
                                    });
                                }
                                else
                                {
                                    if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                                    }
                                    File.WriteAllText(outputPath, outputContent, new UTF8Encoding(false));
                                }
                                res.IsSuccess = true;
                            }
                            break;
                        case GenStepType.Command:

                            break;
                        case GenStepType.Script:

                            break;
                        default:
                            break;
                    }
                }
                action.ActionStatus = ActionStatus.Success;
            }
            catch (Exception ex)
            {
                action.ActionStatus = ActionStatus.Failed;
                // TODO: 记录执行情况
                _logger.LogError(ex, "Execute action failed");
                res.ErrorMsg = ex.Message;
                await SaveChangesAsync();
                res.IsSuccess = false;
            }
        }
        else
        {
            action.ActionStatus = ActionStatus.Failed;
            await SaveChangesAsync();
            res.IsSuccess = false;
            res.Equals("未找到任务步骤");
        }
        await SaveChangesAsync();
        return res;
    }

    public List<ModelFileItemDto> GetModelFile(GenSourceType sourceType)
    {
        var entityPath = _projectContext.EntityPath;
        var filePaths = CodeAnalysisService.GetEntityFilePaths(entityPath!);
        var entityFiles = new List<EntityFile>();
        if (filePaths.Count != 0)
        {
            entityFiles = _codeAnalysis.GetEntityFiles(entityPath!, filePaths);
        }

        if (sourceType == GenSourceType.EntityCLass)
        {
            return entityFiles.Select(q => new ModelFileItemDto
            {
                Name = q.Name,
                FullName = q.FullName,
            }).ToList();

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

                res.AddRange(dtoFiles.Select(q => new ModelFileItemDto
                {
                    Name = Path.GetFileName(q),
                    FullName = q,
                }));
            }
            return res;
        }
        return [];
    }

}