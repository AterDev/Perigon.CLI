using Entity;

namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate(EntityInfo entityInfo, ICollection<string> userIdKeys)
{
    public string CommonNamespace { get; init; } = entityInfo.GetCommonNamespace();
    public string ShareNamespace { get; init; } = entityInfo.GetShareNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;
    public ICollection<string> UserIdKeys = userIdKeys;

    /// <summary>
    /// 全局依赖
    /// </summary>
    /// <returns></returns>
    public List<string> GetGlobalUsings()
    {
        var globalUsing = new List<string>
        {
            $"global using {EntityInfo.AssemblyName};",
            $"global using {EntityInfo.NamespaceName};",
            $"global using Share.Implement;",
            $"global using {CommonNamespace}.{ConstVal.ManagersDir};",
        };
        if (EntityInfo.DbContextSpaceName != null)
        {
            globalUsing.Add($"global using {EntityInfo.DbContextSpaceName};");
        }
        return globalUsing;
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent(string tplContent, string nsp)
    {
        var genContext = new RazorGenContext();
        var model      = new ManagerViewModel
        {
            Namespace = nsp,
            EntityName = EntityInfo.Name,
            EntitySummary = EntityInfo.Summary,
            ShareNamespace = ShareNamespace,
            DbContextName = EntityInfo.DbContextName,
            Comment = EntityInfo.Comment,
            AddMethod = GetAddMethodContent(),
            FilterMethod = GetFilterMethodContent(),
            AdditionMethods = GetUserRelateMethods(),
        };

        return genContext.GenManager(tplContent, model);
    }

    private string GetAddMethodContent()
    {
        var userId = GetUserIdKey();
        return $$"""
                /// <summary>
                /// Add {{EntityInfo.Summary}}
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task{{EntityInfo.Name}}> AddAsync(@(Model.EntityName)AddDto dto)
                {
                    var entity = dto.MapTo{{EntityInfo.Name}}>();
                    {{(userId == null ? "" : "entity.UserId = _userContext.UserId;")}}
                    await InsertAsync(entity);
                    return entity;
                }
            """;
    }

    private string? GetUserIdKey()
    {
        var prop = EntityInfo.PropertyInfos
            .Where(p => UserIdKeys.Contains(p.Name)
            && p.Type == "Guid")
            .FirstOrDefault();
        return prop?.Name;

    }

    /// <summary>
    /// 额外方法
    /// </summary>
    /// <returns></returns>
    private string GetUserRelateMethods()
    {
        var methods = new List<string>
        {
            GenPermissionMethods(),
        };

        return string.Join(Environment.NewLine, methods);
    }

    private string GenPermissionMethods()
    {
        var userId      = GetUserIdKey();
        var queryString = string.Empty;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            queryString = $".Where(q => q.{userId} == _userContext.UserId)";
        }
        return $$"""
                public override async Task<bool> HasPermissionAsync(Guid id)
                {
                    var query = _dbSet{{queryString}}
                        .Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
                    return await query.AnyAsync();
                }

                public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
                {
                    if (!ids.Any())
                    {
                        return [];
                    }
                    var query = _dbSet{{queryString}}
                        .Where(q => ids.Contains(q.Id) && q.TenantId == _userContext.TenantId)
                        .Select(q => q.Id);
                    return await query.ToListAsync();
                }
            """;
    }

    /// <summary>
    /// filter method queryable
    /// </summary>
    /// <returns></returns>
    private string GetFilterMethodContent()
    {
        string              content    = "";
        string              entityName = EntityInfo?.Name ?? "";
        List<PropertyInfo>? props      = EntityInfo?.GetFilterProperties();
        if (props != null && props.Count != 0)
        {
            content += """
                        Queryable = Queryable

                """;
        }

        var userId = GetUserIdKey();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            content = $".Where(q => q.{userId} == _userContext.UserId)";
        }

        var last = props?.LastOrDefault();
        props?.ForEach(p =>
        {
            bool isLast = p == last;
            string name = p.Name;
            content += $$"""
                        .WhereNotNull(filter.{{name}}, q => q.{{name}} == filter.{{name}}){{(
                isLast ? ";" : ""
            )}}

            """;
        });
        content += $$"""
                /// <summary>
                /// Filter {{EntityInfo?.Summary}} with paging
                /// </summary>
                public async Task<PageList{{EntityInfo?.Name}}ItemDto>> FilterAsync(@(Model.EntityName)FilterDto filter)
                {        
                    return await PageListAsync<{{entityName + ConstVal.FilterDto}}, {{entityName + ConstVal.ItemDto}}>(filter);
                }
            """;
        return content;
    }
}
