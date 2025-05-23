using Entity;

namespace CodeGenerator.Generate;

/// <summary>
/// 生成Rest API控制器
/// </summary>
public class RestApiGenerate(EntityInfo entityInfo)
{
    public string? EntityNamespace { get; set; } = entityInfo.NamespaceName;
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; } = entityInfo.GetShareNamespace();
    public string? ApplicationNamespace { get; set; } = entityInfo.GetManagerNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;

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
            $"global using {ConstVal.ExtensionLibName}.Services;",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {ApplicationNamespace}.Const;",
            $"global using {ApplicationNamespace}.{ConstVal.ManagersDir};",
            $"global using {ApplicationNamespace}.Implement;",
        ];
    }

    /// <summary>
    /// 生成控制器
    /// </summary>
    public string GetRestApiContent(string tplContent)
    {
        var genContext = new RazorGenContext();
        var model = new ControllerViewModel
        {
            Namespace = EntityInfo.GetAPINamespace(),
            EntityName = EntityInfo.Name,
            Comment = EntityInfo.Comment,
            ShareNamespace = ShareNamespace
        };
        return genContext.GenCode(tplContent, model);
    }
}
