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
            $"global using {ConstVal.ConventionLibName};",
            $"global using {ConstVal.ConventionLibName}.Abstraction;",
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
            EntityName = EntityInfo.Name,
            Comment = EntityInfo.Comment,
            ShareNamespace = ShareNamespace,
            AddCodes = GenAddCodes(isSystem),
            UpdateCodes = GenUpdateCodes(isSystem),
            DetailCodes = GenDetailCodes(isSystem),
            DeleteCodes = GenDeleteCodes(isSystem),
            FilterCodes = GenFilterCodes(isSystem),
        };
        return genContext.GenCode(tplContent, model);
    }

    private string GenFilterCodes(bool isSystem = false)
    {
        var result = string.Empty;
        if (!isSystem)
        {
            var userEntities = SolutionConfig.UserEntities;
            if (DtoDict.TryGetValue(EntityInfo.Name + DtoType.Filter.ToString(), out var dto))
            {
                var userProp = dto
                    .Properties.Where(d =>
                        d.IsNavigation && userEntities.Contains(d.NavigationName ?? d.Type)
                    )
                    .FirstOrDefault();

                if (userProp != null)
                {
                    result = $@"filter.{userProp.Name} = _user.UserId;";
                }
            }
        }
        return result;
    }

    private string GenAddCodes(bool isSystem = false)
    {
        var result = string.Empty;
        if (!isSystem)
        {
            var userEntities = SolutionConfig.UserEntities;
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
                    .EntityInfo?.Navigations.Where(n => userEntities.Contains(n.Type))
                    .FirstOrDefault();

                if (userNav != null)
                {
                    string navigationId = $"{userNav.Name}.Id";
                    // Using explicitly defined foreign keys
                    if (
                        userNav.ForeignKeyProperties.Any(p =>
                            !p.IsShadow && p.Name == userNav.ForeignKey
                        )
                    )
                    {
                        navigationId = userNav.ForeignKey;
                    }
                    result += $$"""
                        if (!await _manager.IsValidate{{navigation.Type}}Async(dto.{{navigationId}}, _user.UserId))
                                {
                                    return NotFound(Localizer.NotFoundResource);
                                }
                        """;
                }
            }

            if (DtoDict.TryGetValue(EntityInfo.Name + DtoType.Add.ToString(), out var dto))
            {
                var userProp = dto
                    .Properties.Where(d =>
                        d.IsNavigation && userEntities.Contains(d.NavigationName ?? d.Type)
                    )
                    .FirstOrDefault();
                if (userProp != null)
                {
                    result += $@"dto.{userProp.Name} = _user.UserId;";
                }
            }
        }
        return result;
    }

    private string GenUpdateCodes(bool isSystem = false)
    {
        var result = string.Empty;
        string method = isSystem ? "GetCurrentAsync(id)" : "GetOwnedAsync(id, _user.UserId)";

        result = $$"""
            var entity = await _manager.{{method}};
                    if (entity == null)
                    {
                        return NotFound(Localizer.NotFoundResource);
                    }
            """;
        return result;
    }

    private string GenDetailCodes(bool isSystem = false)
    {
        var result = string.Empty;
        if (!isSystem)
        {
            var navigations = EntityInfo.Navigations.Where(n =>
                SolutionConfig.UserEntities.Contains(n.Type)
            );

            if (navigations.Any())
            {
                result = """
                        if (!await _manager.IsOwnedAsync(id, _user.UserId))
                            {
                                return NotFound(Localizer.NotFoundResource);
                            }
                    """;
            }
        }
        return result;
    }

    private string GenDeleteCodes(bool isSystem = false)
    {
        var result = string.Empty;
        if (!isSystem)
        {
            var navigations = EntityInfo.Navigations.Where(n =>
                SolutionConfig.UserEntities.Contains(n.Type)
            );
            if (navigations.Any())
            {
                result = """
                    if (!await _manager.IsOwnedAsync(id, _user.UserId))
                            {
                                return NotFound(Localizer.NotFoundResource);
                            }
                    """;
            }
        }
        return result;
    }
}
