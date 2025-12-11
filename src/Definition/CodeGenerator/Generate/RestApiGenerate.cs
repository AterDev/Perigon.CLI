using System.Collections.ObjectModel;
using Entity;
using Entity.StudioMod;

namespace CodeGenerator.Generate;

/// <summary>
/// 生成Rest API控制器
/// </summary>
public class RestApiGenerate(
    EntityInfo entityInfo,
    SolutionConfig solutionConfig,
    ReadOnlyDictionary<string, DtoInfo> dtoDict
)
{
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    private readonly string? ShareNamespace = entityInfo.GetShareNamespace();
    private readonly string? ModuleNamespace = entityInfo.GetCommonNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;
    public ReadOnlyDictionary<string, DtoInfo> DtoDict { get; init; } = dtoDict;
    public SolutionConfig SolutionConfig { get; init; } = solutionConfig;

    public List<string> GetGlobalUsings()
    {
        return
        [
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.AspNetCore.Mvc;",
            "global using Microsoft.AspNetCore.Authorization;",
            "global using System.Text.Json.Serialization;",
            "global using Microsoft.EntityFrameworkCore;",
            "global using Share;",
            $"global using {ConstVal.CoreLibName}.Models;",
            $"global using {ConstVal.CoreLibName}.Utils;",
            $"global using {ConstVal.CoreLibName};",
            $"global using {ConstVal.CoreLibName}.Abstraction;",
            $"global using {ConstVal.ExtensionLibName}.Services;",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {ModuleNamespace}.{ConstVal.ManagersDir};",
        ];
    }

    /// <summary>
    /// 生成控制器
    /// </summary>
    public string GetRestApiContent(string tplContent, string serviceName, bool isSystem = false)
    {
        var genContext = new RazorGenContext();
        var model = new ControllerViewModel
        {
            Namespace = serviceName,
            ModuleName = EntityInfo.ModuleName,
            EntityName = EntityInfo.Name,
            Comment = EntityInfo.Comment,
            Summary = EntityInfo.Summary,
            ShareNamespace = ShareNamespace,
            AddCodes = GenAddCodes(isSystem),
        };
        return genContext.GenCode(tplContent, model);
    }


    private string GenAddCodes(bool isSystem = false)
    {
        var result = string.Empty;
        if (!isSystem)
        {
            var userEntities = SolutionConfig.UserIdKeys;
            var navigations = EntityInfo.Navigations.Where(n =>
                !userEntities.Contains(n.Type) && n.Type != EntityInfo.Name && !n.IsCollection
            );
            if (!navigations.Any())
            {
                return string.Empty;
            }

            foreach (var navigation in navigations)
            {
                var userNav = navigation
                    .EntityInfo?.Navigations.FirstOrDefault(n => userEntities.Contains(n.Type));
                if (userNav != null)
                {
                    string navigationId = $"{userNav.Name}Id";
                    // Using explicitly defined foreign keys
                    if (userNav.ForeignKeyProperties.Any(p => !p.IsShadow && p.Name == userNav.ForeignKey))
                    {
                        navigationId = userNav.ForeignKey;
                    }
                    result += $"// should validate {navigationId}";
                }
            }
        }
        return result;
    }
}
