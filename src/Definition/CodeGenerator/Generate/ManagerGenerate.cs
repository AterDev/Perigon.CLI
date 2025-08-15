using Entity;

namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate(EntityInfo entityInfo, ICollection<string> userEntities)
{
    public string CommonNamespace { get; init; } = entityInfo.GetCommonNamespace();
    public string ShareNamespace { get; init; } = entityInfo.GetShareNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;
    public ICollection<string> UserEntities = userEntities;

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
        var model = new ManagerViewModel
        {
            Namespace = nsp,
            EntityName = EntityInfo.Name,
            ShareNamespace = ShareNamespace,
            DbContextName = EntityInfo.DbContextName,
            Comment = EntityInfo.Comment,
            FilterCode = GetFilterMethodContent(),
            AdditionMethods = GenAdditionMethods(),
        };

        return genContext.GenManager(tplContent, model);
    }

    /// <summary>
    /// 额外方法
    /// </summary>
    /// <returns></returns>
    private string GenAdditionMethods()
    {
        var methods = new List<string>();
        methods.Add(GenGetOwnedMethod());
        methods.Add(GenIsOwnedMethods());
        methods.Add(GenValidateMethods());

        return string.Join(Environment.NewLine, methods);
    }

    private string GenValidateMethods()
    {
        // 仅对非用户实体的导航生成校验方法，如 Blog 的 Catalog，校验 Catalog 是否属于指定用户
        var navigations = EntityInfo.Navigations.Where(n => !UserEntities.Contains(n.Type));
        if (!navigations.Any())
        {
            return string.Empty;
        }

        var methods = new List<string>();
        foreach (var navigation in navigations)
        {
            // 推断外键参数名称（当前实体上的外键，如 CatalogId）
            var foreignKey = navigation.ForeignKey;
            if (navigation.ForeignKeyProperties.Count > 1)
            {
                foreignKey = navigation
                    .ForeignKey.Split(",")
                    .Where(f => f.Contains(navigation.Name))
                    .FirstOrDefault();

                foreignKey ??= navigation.ForeignKeyProperties.First().Name;
            }
            var fkParam = foreignKey.ToCamelCase();
            // 方法名基于导航类型
            var methodName = $"IsValidate{navigation.Type}Async";
            var entityType = navigation.Type;

            var method = $$"""
                /// <summary>
                /// validate {{entityType}} owned by user
                /// </summary>
                public async Task<bool> {{methodName}}(Guid {{fkParam}}, Guid userId)
                {
                        return await _dbContext
                            .Set<{{entityType}}>()
                            .Where(q => q.Id == {{fkParam}} && q.UserId == userId)
                            .AnyAsync();
                }
                """;

            methods.Add(method);
        }

        return string.Join(Environment.NewLine, methods);
    }

    /// <summary>
    /// generate is owned methods
    /// </summary>
    /// <returns></returns>
    private string GenIsOwnedMethods()
    {
        var navigations = EntityInfo.Navigations.Where(n => UserEntities.Contains(n.Type));
        if (!navigations.Any())
        {
            return string.Empty;
        }
        var methodParams = string.Empty;
        var conditions = string.Empty;
        foreach (var navigation in navigations)
        {
            var foreignKey = navigation.ForeignKey;
            if (navigation.ForeignKeyProperties.Count > 1)
            {
                foreignKey = navigation
                    .ForeignKey.Split(",")
                    .Where(f => f.Contains(navigation.Name))
                    .FirstOrDefault();

                foreignKey ??= navigation.ForeignKeyProperties.First().Name;
            }
            methodParams += $", Guid {foreignKey.ToCamelCase()}";
            string navigationId = $"{navigation.Name}.Id";
            // Using explicitly defined foreign keys
            if (navigation.ForeignKeyProperties.Any(p => !p.IsShadow && p.Name == foreignKey))
            {
                navigationId = foreignKey;
            }
            conditions += $" && {navigationId} == {foreignKey.ToCamelCase()})";
        }
        return $$"""
            /// <summary>
            /// has @(Model.EntityName)
            /// </summary>
            public async Task<bool> IsOwnedAsync(Guid id{{methodParams}})
            {
                    return await Queryable.AnyAsync(q => q.Id == id {{conditions}});
                        
            }
            """;
    }

    /// <summary>
    /// generate get owned method
    /// </summary>
    /// <returns></returns>
    private string GenGetOwnedMethod()
    {
        var navigations = EntityInfo.Navigations.Where(n => UserEntities.Contains(n.Type));

        if (!navigations.Any())
        {
            return string.Empty;
        }
        var methodParams = string.Empty;
        var queryLines = string.Empty;
        foreach (var navigation in navigations)
        {
            var foreignKey = navigation.ForeignKey;
            if (navigation.ForeignKeyProperties.Count > 1)
            {
                foreignKey = navigation
                    .ForeignKey.Split(",")
                    .Where(f => f.Contains(navigation.Name))
                    .FirstOrDefault();

                foreignKey ??= navigation.ForeignKeyProperties.First().Name;
            }
            methodParams += $", Guid {foreignKey.ToCamelCase()}";
            string navigationId = $"{navigation.Name}.Id";
            // Using explicitly defined foreign keys
            if (navigation.ForeignKeyProperties.Any(p => !p.IsShadow && p.Name == foreignKey))
            {
                navigationId = foreignKey;
            }
            queryLines +=
                $".Where(q => q.{navigationId} == {foreignKey.ToCamelCase()}){Environment.NewLine}";
        }
        if (queryLines.NotEmpty())
        {
            queryLines = $"query = query.{queryLines};";
        }

        return $$"""
            /// <summary>
            /// get owned @(Model.EntityName)
            /// </summary>
            public async Task<@(Model.EntityName)?> GetOwnedAsync(Guid id{{methodParams}})
            {
                    var query = _dbSet.Where(q => q.Id == id);
                    {{queryLines}}
                    return await query.FirstOrDefaultAsync();
            }
            """;
    }

    /// <summary>
    /// filter method queryable
    /// </summary>
    /// <returns></returns>
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
            content += $$"""
                        .WhereNotNull(filter.{{name}}, q => q.{{name}} == filter.{{name}}){{(
                isLast ? ";" : ""
            )}}

            """;
        });
        content += $$"""
                    
                    return await ToPageAsync<{{entityName + ConstVal.FilterDto}}, {{entityName
                + ConstVal.ItemDto}}>(filter);
            """;
        return content;
    }
}
