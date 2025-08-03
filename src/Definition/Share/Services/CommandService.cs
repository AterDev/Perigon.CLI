using System.Text.Json.Nodes;
using CodeGenerator.Helper;
using Entity;
using Share.Models.CommandDtos;

namespace Share.Services;

/// <summary>
///  command service
/// </summary>
/// <param name="context"></param>
/// <param name="projectContext"></param>
/// <param name="solutionService"></param>
public class CommandService(
    CommandDbContext context,
    IProjectContext projectContext,
    SolutionService solutionService
)
{
    public string? ErrorMsg { get; set; }

    public async Task<Guid?> AddProjectAsync(string name, string path)
    {
        var projectFilePath = Directory
            .GetFiles(path, $"*{ConstVal.SolutionExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        projectFilePath ??= Directory
            .GetFiles(path, $"*{ConstVal.SolutionXMLExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        projectFilePath ??= Directory
            .GetFiles(path, $"*{ConstVal.CSharpProjectExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
        projectFilePath ??= Directory
            .GetFiles(path, ConstVal.NodeProjectFile, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        var solutionType = AssemblyHelper.GetSolutionType(projectFilePath);
        var solutionName = Path.GetFileName(projectFilePath) ?? name;
        var solutionPath = Path.GetDirectoryName(projectFilePath) ?? "";
        var entity = new Solution()
        {
            DisplayName = name,
            Path = solutionPath,
            Name = solutionName,
            SolutionType = solutionType,
        };
        entity.Config.SolutionPath = solutionPath;

        context.Solutions.Add(entity);
        return await context.SaveChangesAsync() > 0 ? entity.Id : null;
    }

    /// <summary>
    /// 创建解决方案
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> CreateSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        string solutionPath = Path.Combine(dto.Path, dto.Name);
        string templateType = dto.IsLight ? ConstVal.Mini : ConstVal.Standard;

        string version = AssemblyHelper.GetCurrentToolVersion();

        if (ProcessHelper.RunCommand("dotnet", $"new list ater-{templateType}", out _))
        {
            ProcessHelper.RunCommand("dotnet", $"new update", out _);
        }
        else
        {
            ProcessHelper.RunCommand(
                "dotnet",
                $"new install ater.web.templates::{version}",
                out string msg
            );
            OutputHelper.Info(msg);
        }

        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(solutionPath);
        }

        var templateOptions = string.Empty;
        if (dto.FrontType != FrontType.None)
        {
            templateOptions = $" --frontType {dto.FrontType}";
        }
        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"new ater-{templateType} -o {solutionPath} --force {templateOptions}",
                out string error
            )
        )
        {
            OutputHelper.Error(error);
            ErrorMsg = "Create failed, check the error output.";
            return false;
        }
        OutputHelper.Success($"Created new solution {solutionPath}");

        var id = await solutionService.SaveSolutionAsync(solutionPath, dto.Name);
        await projectContext.SetProjectByIdAsync(id);

        OutputHelper.Important($"Apply settings...");

        // 更新配置文件
        var services = solutionService.GetServices();
        if (services != null)
        {
            foreach (var service in services)
            {
                UpdateAppSettings(dto, solutionPath, service.Name);
            }
        }

        // 前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            string appPath = Path.Combine(solutionPath, "src", "ClientApp", "WebApp");
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }
        // 添加模块到解决方案中
        if (!dto.IsLight && dto.Modules?.Count > 0)
        {
            SolutionService.AddDefaultModule(ModuleInfo.User, solutionPath);
            foreach (string item in dto.Modules)
            {
                OutputHelper.Important($"Add module:{item}");
                SolutionService.AddDefaultModule(item, solutionPath);
            }
        }

        SolutionService.BuildSourceGeneration(solutionPath);
        OutputHelper.Success($"Create solution {dto.Name} completed!");
        return true;
    }

    /// <summary>
    /// 更新配置文件
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="path"></param>
    /// <param name="serviceName"></param>
    private static void UpdateAppSettings(CreateSolutionDto dto, string path, string serviceName)
    {
        // 修改配置文件
        string configFile = Path.Combine(
            path,
            PathConst.ServicesPath,
            serviceName,
            ConstVal.AppSettingDevelopmentJson
        );
        string jsonString = File.ReadAllText(configFile);
        JsonNode? jsonNode = JsonNode.Parse(
            jsonString,
            documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }
        );
        if (jsonNode != null)
        {
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString());

            if (dto.CommandDbConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(
                    jsonNode,
                    "ConnectionStrings.CommandDb",
                    dto.CommandDbConnStrings
                );
            }

            if (dto.QueryDbConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(
                    jsonNode,
                    "ConnectionStrings.QueryDb",
                    dto.QueryDbConnStrings
                );
            }
            if (dto.CacheConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(
                    jsonNode,
                    "ConnectionStrings.Cache",
                    dto.CacheConnStrings
                );
            }
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "ConnectionStrings.CacheInstanceName",
                dto.CacheInstanceName ?? "Dev"
            );

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }
    }
}
