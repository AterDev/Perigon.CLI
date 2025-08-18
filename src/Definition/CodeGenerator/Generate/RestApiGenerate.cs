using Entity;

namespace CodeGenerator.Generate;

/// <summary>
/// 生成Rest API控制器
/// </summary>
public class RestApiGenerate(EntityInfo entityInfo, string serviceName)
{
    public string? EntityNamespace { get; set; } = entityInfo.NamespaceName;

    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; } = entityInfo.GetShareNamespace();
    public string? ModuleNamespace { get; set; } = entityInfo.GetCommonNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;
    public string ServiceName { get; init; } = serviceName;

    public List<string> GetGlobalUsings()
    {
        return
        [
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.AspNetCore.Mvc;",
            "global using Microsoft.AspNetCore.Authorization;",
            "global using System.Text.Json.Serialization;",
            "global using Microsoft.EntityFrameworkCore;",
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
    public string GetRestApiContent(string tplContent, bool isSystem = false)
    {
        var genContext = new RazorGenContext();
        var model = new ControllerViewModel
        {
            Namespace = ServiceName,
            EntityName = EntityInfo.Name,
            Comment = EntityInfo.Comment,
            ShareNamespace = ShareNamespace,
        };
        return genContext.GenCode(tplContent, model);
    }

    private string GenFilterCodes(bool isSystem = false)
    {
        var result = string.Empty;
        if (!isSystem) { }
        return result;
    }
}
