using CodeGenerator.Models;
using Microsoft.OpenApi;
using StudioMod.Models.ApiDocInfoDtos;

namespace StudioMod.Managers;

/// <summary>
/// 接口文档
/// </summary>
public class ApiDocInfoManager(
    DefaultDbContext dbContext,
    IProjectContext project,
    ILogger<ApiDocInfoManager> logger,
    CodeGenService codeGenService,
    Localizer localizer
) : ManagerBase<DefaultDbContext, ApiDocInfo>(dbContext, logger)
{
    private readonly IProjectContext _project = project;
    private readonly CodeGenService _codeGenService = codeGenService;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo> CreateNewEntityAsync(ApiDocInfoAddDto dto)
    {
        ApiDocInfo entity = dto.MapTo<ApiDocInfo>();
        entity.ProjectId = _project.SolutionId!.Value;
        return await Task.FromResult(entity);
    }

    public async Task<bool> UpdateAsync(ApiDocInfo entity, ApiDocInfoUpdateDto dto)
    {
        entity = entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<ApiDocInfoItemDto>> FilterAsync(ApiDocInfoFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.ProjectId, q => q.ProjectId == filter.ProjectId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name);

        return await ToPageAsync<ApiDocInfoFilterDto, ApiDocInfoItemDto>(filter);
    }

    /// <summary>
    /// 解析并获取文档内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocContent?> GetContentAsync(Guid id, bool isFresh)
    {
        ApiDocInfo? apiDocInfo = await GetCurrentAsync(id);
        string path = apiDocInfo!.Path;
        try
        {
            var (apiDocument, _) = await OpenApiDocument.LoadAsync(path);

            if (apiDocument == null)
            {
                ErrorMsg = $"parse {path} faield!";
                return null;
            }
            var helper = new OpenApiService(apiDocument);
            return new ApiDocContent
            {
                TypeMeta = helper.ModelInfos,
                OpenApiTags = helper.OpenApiTags,
                RestApiGroups = helper.RestApiGroups,
            };
        }
        catch (Exception ex)
        {
            ErrorMsg = ex.Message;
            return null;
        }
    }

    /// <summary>
    /// 导出文档
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<string?> ExportDocAsync(Guid id)
    {
        ApiDocInfo? apiDocInfo = await FindAsync(id);
        if (apiDocInfo == null)
            return string.Empty;
        string path = apiDocInfo.Path;
        var (apiDocument, _) = await OpenApiDocument.LoadAsync(path);

        if (apiDocument == null)
        {
            ErrorMsg = $"parse {path} faield!";
            return null;
        }

        var helper = new OpenApiService(apiDocument);
        var groups = helper.RestApiGroups;

        StringBuilder sb = new();
        foreach (var group in groups)
        {
            sb.AppendLine("## " + group.Description);
            sb.AppendLine();
            foreach (var api in group.ApiInfos)
            {
                sb.AppendLine($"### {api.Summary}");
                sb.AppendLine();
                sb.AppendLine("|||");
                sb.AppendLine("|---------|---------|");
                sb.AppendLine($"|接口说明|{api.Summary}|");
                sb.AppendLine($"|接口地址|{api.Router}|");
                sb.AppendLine($"|接口方法|{api.HttpMethod.ToString()}|");
                sb.AppendLine();

                sb.AppendLine("#### 请求内容");
                sb.AppendLine();
                var requestInfo = api.RequestInfo;
                if (requestInfo != null)
                {
                    sb.AppendLine("|名称|类型|是否必须|说明|");
                    sb.AppendLine("|---------|---------|---------|---------|");
                    foreach (var property in requestInfo.PropertyInfos)
                    {
                        sb.AppendLine(
                            $"|{property.Name}|{property.Type}|{(property.IsRequired ? "是" : "否")}|{property.CommentSummary?.Trim()}|"
                        );
                    }
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("无");
                }
                sb.AppendLine();
                sb.AppendLine("#### 返回内容");
                var responseInfo = api.ResponseInfo;
                if (responseInfo != null)
                {
                    sb.AppendLine("|名称|类型|说明|");
                    sb.AppendLine("|---------|---------|---------|");
                    foreach (var property in responseInfo.PropertyInfos)
                    {
                        sb.AppendLine(
                            $"|{property.Name}|{property.Type}|{property.CommentSummary?.Trim()}|"
                        );
                    }
                }
                else
                {
                    sb.AppendLine("无");
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 是否唯一
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsConflictAsync(string unique)
    {
        return await _dbSet.AnyAsync(q => q.Id == new Guid(unique));
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo?> GetOwnedAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 生成请求客户端
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>?> GenerateRequestClientAsync(
        Guid openApiDocId,
        RequestClientDto dto
    )
    {
        var doc = await GetCurrentAsync(openApiDocId);
        if (doc == null)
        {
            ErrorMsg = localizer.Get(Localizer.NotFoundWithName, openApiDocId);
            return null;
        }
        doc.LocalPath = dto.OutputPath;
        await SaveChangesAsync();

        var files = new List<GenFileInfo>();
        switch (dto.ClientType)
        {
            case RequestClientType.NgHttp:
            case RequestClientType.Axios:
                files = await _codeGenService.GenerateWebRequestAsync(
                    dto.OpenApiEndpoint!,
                    dto.OutputPath!,
                    dto.ClientType
                );

                break;
            case RequestClientType.Csharp:
                files = await CodeGenService.GenerateCsharpApiClientAsync(
                    dto.OpenApiEndpoint!,
                    dto.OutputPath!
                );
                break;
            default:
                break;
        }
        _codeGenService.GenerateFiles(files);
        return files;
    }
}
