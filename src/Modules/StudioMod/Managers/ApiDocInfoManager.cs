using CodeGenerator.Models;
using Microsoft.OpenApi;
using StudioMod.Models.ApiDocInfoDtos;

namespace StudioMod.Managers;

/// <summary>
/// 接口文档
/// </summary>
/// <remarks>
/// Constructor for DbContext pattern
/// </remarks>
public class ApiDocInfoManager(
    DefaultDbContext dbContext,
    IProjectContext project,
    ILogger<ApiDocInfoManager> logger,
    CodeGenService codeGenService,
    Localizer localizer
) : ManagerBase<DefaultDbContext, ApiDocInfo>(dbContext, logger)
{

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo> CreateNewEntityAsync(ApiDocInfoAddDto dto)
    {
        ApiDocInfo entity = dto.MapTo<ApiDocInfo>();
        entity.ProjectId = (int)project.SolutionId!.Value.GetHashCode();
        return await Task.FromResult(entity);
    }

    public async Task<bool> UpdateAsync(ApiDocInfo entity, ApiDocInfoUpdateDto dto)
    {
        entity = entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<ApiDocInfoItemDto>> FilterAsync(ApiDocInfoFilterDto filter)
    {
        var query = Queryable;

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(q => q.ProjectId == (int)filter.ProjectId.Value.GetHashCode());
        }

        if (!string.IsNullOrEmpty(filter.Name))
        {
            query = query.Where(q => q.Name == filter.Name);
        }

        Queryable = query;
        return await ToPageAsync<ApiDocInfoFilterDto, ApiDocInfoItemDto>(filter);
    }

    /// <summary>
    /// 解析并获取文档内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocContent?> GetContentAsync(int id, bool isFresh)
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
    public async Task<string?> ExportDocAsync(int id)
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
        return _dbSet.Any(q => q.Id == int.Parse(unique));
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo?> GetOwnedAsync(int id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        return query.FirstOrDefault();
    }

    /// <summary>
    /// 生成请求客户端
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>?> GenerateRequestClientAsync(
        int openApiDocId,
        RequestClientDto dto
    )
    {
        var doc = _dbSet.FirstOrDefault(d => d.Id == openApiDocId);
        if (doc == null)
        {
            ErrorMsg = localizer.Get(Localizer.NotFoundWithName, openApiDocId.ToString());
            return null;
        }
        doc.LocalPath = dto.OutputPath;
        await UpdateAsync(doc);

        var files = new List<GenFileInfo>();
        switch (dto.ClientType)
        {
            case RequestClientType.NgHttp:
            case RequestClientType.Axios:
                files = await codeGenService.GenerateWebRequestAsync(
                    dto.OpenApiEndpoint!,
                    dto.OutputPath!,
                    dto.ClientType,
                    dto.OnlyModels
                );

                break;
            case RequestClientType.CSharp:
                files = await codeGenService.GenerateCsharpApiClientAsync(
                    dto.OpenApiEndpoint!,
                    dto.OutputPath!
                );
                break;
            default:
                break;
        }
        codeGenService.GenerateFiles(files, false);
        return files;
    }
}
