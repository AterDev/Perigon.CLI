using Microsoft.OpenApi.Readers;
using StudioMod.Models.ApiDocInfoDtos;

namespace StudioMod.Managers;
/// <summary>
/// 接口文档
/// </summary>
public class ApiDocInfoManager(
    DataAccessContext<ApiDocInfo> dataContext,
    IProjectContext project,
    ILogger<ApiDocInfoManager> logger,
    CodeGenService codeGenService
    ) : ManagerBase<ApiDocInfo>(dataContext, logger)
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
        ApiDocInfo entity = dto.MapTo<ApiDocInfoAddDto, ApiDocInfo>();
        entity.ProjectId = _project.ProjectId;
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
        string openApiContent = "";

        if (!isFresh && apiDocInfo.Content.NotEmpty())
        {
            openApiContent = apiDocInfo.Content;
        }
        else
        {
            try
            {
                if (path.StartsWith("http://") || path.StartsWith("https://"))
                {
                    HttpClientHandler handler = new()
                    {
                        ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                    };
                    using HttpClient http = new(handler);
                    http.Timeout = TimeSpan.FromSeconds(60);
                    openApiContent = await http.GetStringAsync(path);
                }
                else
                {
                    openApiContent = File.ReadAllText(path);
                }
                apiDocInfo.Content = openApiContent;
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMsg = $"{path} 请求失败！" + ex.Message;
                return default;
            }
        }

        openApiContent = openApiContent
               .Replace("«", "")
               .Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
        var helper = new OpenApiService(apiDocument);
        return new ApiDocContent
        {
            TypeMeta = helper.ModelInfos,
            OpenApiTags = helper.OpenApiTags,
            RestApiGroups = helper.RestApiGroups
        };
    }

    /// <summary>
    /// 导出文档
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<string> ExportDocAsync(Guid id)
    {
        ApiDocInfo? apiDocInfo = await FindAsync(id);
        if (apiDocInfo == null) return string.Empty;
        string path = apiDocInfo.Path;
        string openApiContent = "";
        if (path.StartsWith("http://") || path.StartsWith("https://"))
        {
            using HttpClient http = new();
            http.Timeout = TimeSpan.FromSeconds(60);
            openApiContent = await http.GetStringAsync(path);
        }
        else
        {
            openApiContent = File.ReadAllText(path);
        }
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);

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
                sb.AppendLine($"|接口方法|{api.OperationType.ToString()}|");
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
                        sb.AppendLine($"|{property.Name}|{property.Type}|{(property.IsRequired ? "是" : "否")}|{property.CommentSummary?.Trim()}|");
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
                        sb.AppendLine($"|{property.Name}|{property.Type}|{property.CommentSummary?.Trim()}|");
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
        // TODO:自定义唯一性验证参数和逻辑
        return await Command.AnyAsync(q => q.Id == new Guid(unique));
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo?> GetOwnedAsync(Guid id)
    {
        var query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 生成前端请求服务
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="webPath"></param>
    /// <param name="type"></param>
    /// <param name="swaggerPath"></param>
    /// <returns></returns>
    public async Task GenerateRequestAsync(ApiDocInfo doc, string webPath, RequestLibType type, string? swaggerPath = null)
    {
        // 保存路径
        doc.LocalPath = webPath;
        await SaveChangesAsync();

        swaggerPath ??= Path.Combine(_project.ApiPath!, "swagger.json");
        var files = await _codeGenService.GenerateWebRequestAsync(swaggerPath, webPath, type);
        _codeGenService.GenerateFiles(files);
    }


    /// <summary>
    /// 生成csharp请求服务
    /// </summary>
    /// <param name="webPath"></param>
    /// <param name="swaggerPath"></param>
    /// <returns></returns>
    public async Task GenerateCsharpRequestAsync(string webPath, string? swaggerPath = null)
    {
        swaggerPath ??= Path.Combine(_project.ApiPath!, "swagger.json");
        var files = await _codeGenService.GenerateCsharpApiClientAsync(swaggerPath, webPath);
        _codeGenService.GenerateFiles(files);
    }
}
