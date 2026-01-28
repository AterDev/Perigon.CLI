using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Entity;

public static class ConstVal
{
    public const string CommandName = "perigon";
    public const string Version = "10.1.0";
    public const string NetVersion = "net10.0";
    public const string PackageId = "perigon.cli";
    public const string TemplatePackageId = "Perigon.templates";
    public const string TemplateVersion = "1.0.0";

    public const string Mini = "mini";
    public const string WebApi = "perigon-webapi";

    public static JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public const string DbName = "perigon.mds";
    public const string StudioFileName = "AterStudio.dll";
    public const string ModuleExtensionFile = "ModuleExtensions.cs";

    // assembly name
    public const string CommonMod = "CommonMod";
    public const string ShareName = "Share";
    public const string EntityName = "Entity";
    public const string APIName = "ApiService";
    public const string EntityFrameworkName = "EntityFramework";
    public const string AppDbContextName = "AppDbContext";
    public const string ContextBase = "ContextBase";

    // dir names
    public const string DefinitionDir = "Definition";
    public const string ModulesDir = "Modules";
    public const string ModelsDir = "Models";
    public const string ManagersDir = "Managers";
    public const string ControllersDir = "Controllers";
    public const string SrcDir = "src";
    public const string TemplateDir = "templates";
    public const string ShareDlls = "ShareDlls.txt";
    public const string ServicesDir = "Services";
    public const string PerigonDir = "Perigon";
    public const string StudioDir = "PerigonStudio";
    public const string AppHostDir = "AppHost";

    // names
    public const string Manager = "Manager";
    public const string Controller = "Controller";
    public const string DetailDto = "DetailDto";
    public const string ItemDto = "ItemDto";
    public const string FilterDto = "FilterDto";
    public const string AddDto = "AddDto";
    public const string UpdateDto = "UpdateDto";

    // props & keys
    public const string FilterBase = "FilterBase";
    public const string EntityBase = "EntityBase";

    public const string Id = "Id";
    public const string Guid = "Guid";
    public const string CreatedTime = "CreatedTime";
    public const string UpdatedTime = "UpdatedTime";
    public const string IsDeleted = "IsDeleted";
    public const string TenantId = "TenantId";
    public const string PageSize = "PageSize";
    public const string PageIndex = "PageIndex";

    // files
    public const string TemplateZip = "template.zip";
    public const string ModulesZip = "modules.zip";
    public const string StudioZip = "studio.zip";
    public const string SyncJson = "sync.json";
    public const string AppSettingJson = "appsettings.json";
    public const string AppSettingDevelopmentJson = "appsettings.Development.json";

    public const string SolutionExtension = ".sln";
    public const string SolutionXMLExtension = ".slnx";
    public const string CSharpProjectExtension = ".csproj";
    public const string NodeProjectFile = "package.json";

    public const string CoreLibName = "Perigon.AspNetCore";
    public const string ExtensionLibName = "Perigon.AspNetCore.Toolkit";
    public const string SourceGenerationLibName = "Perigon.AspNetCore.SourceGeneration";
    public const string GlobalUsingsFile = "GlobalUsings.cs";
    public const string ModSuffix = "Mod";
}

/// <summary>
/// 默认路径
/// </summary>
public static class PathConst
{
    public static readonly string APIPath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.ServicesDir,
        ConstVal.APIName
    );
    public static readonly string CommonModPath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.ModulesDir,
        ConstVal.CommonMod
    );
    public static readonly string DefinitionPath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.DefinitionDir
    );
    public static readonly string SharePath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.DefinitionDir,
        ConstVal.ShareName
    );

    public static readonly string EntityPath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.DefinitionDir,
        ConstVal.EntityName
    );
    public static readonly string EntityFrameworkPath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.DefinitionDir,
        ConstVal.EntityFrameworkName
    );
    public static readonly string ModulesPath = Path.Combine(ConstVal.SrcDir, ConstVal.ModulesDir);
    public static readonly string ServicesPath = Path.Combine(
        ConstVal.SrcDir,
        ConstVal.ServicesDir
    );
    public static readonly string AppHostPath = Path.Combine(ConstVal.SrcDir, ConstVal.AppHostDir);
    public static readonly string AterPath = Path.Combine(ConstVal.SrcDir, ConstVal.PerigonDir);
    public static readonly string PromptPath = Path.Combine(".github", "prompts");
}
