using Entity;

namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate(EntityInfo entityInfo)
{
    public string ApplicationNamespace { get; init; } = entityInfo.GetManagerNamespace();
    public string ShareNamespace { get; init; } = entityInfo.GetShareNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;

    /// <summary>
    /// 全局依赖
    /// </summary>
    /// <returns></returns>
    public List<string> GetGlobalUsings()
    {
        return
        [
            $"global using {EntityInfo.AssemblyName};",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {ApplicationNamespace}.{ConstVal.ManagersDir};",
            "",
        ];
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent(string tplContent, string nsp)
    {
        var genContext = new RazorGenContext();
        var model = new ManagerViewModel
        {
            Namespace = nsp,
            EntityName = EntityInfo.Name,
            ShareNamespace = ShareNamespace,
            DbContextName = EntityInfo.DbContextName,
            Comment = EntityInfo.Comment,
            FilterCode = GetFilterMethodContent(),
        };

        return genContext.GenManager(tplContent, model);
    }

    private string GetFilterMethodContent()
    {
        string content = "";
        string entityName = EntityInfo?.Name ?? "";
        List<PropertyInfo>? props = EntityInfo?.GetFilterProperties();
        if (props != null && props.Count != 0)
        {
            content += """
                        Queryable = Queryable

                """;
        }
        var last = props?.LastOrDefault();
        props?.ForEach(p =>
        {
            bool isLast = p == last;
            string name = p.Name;
            if (p.IsNavigation && !p.IsComplexType)
            {
                content += $$"""
                            .WhereNotNull(filter.{{name}}Id, q => q.{{name}}.Id == filter.{{name}}Id){{(
                    isLast ? ";" : ""
                )}}

                """;
            }
            else
            {
                content += $$"""
                            .WhereNotNull(filter.{{name}}, q => q.{{name}} == filter.{{name}}){{(
                    isLast ? ";" : ""
                )}}

                """;
            }
        });
        content += $$"""
                    
                    return await ToPageAsync<{{entityName + ConstVal.FilterDto}},{{entityName
                + ConstVal.ItemDto}}>(filter);
            """;
        return content;
    }
}
