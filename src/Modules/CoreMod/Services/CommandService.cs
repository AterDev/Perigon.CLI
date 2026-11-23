using Share.Models.CommandDtos;
using System.Text.Json.Nodes;

namespace CoreMod.Services;

/// <summary>
///  command service
/// </summary>
/// <param name="projectContext"></param>
/// <param name="solutionService"></param>
/// <param name="codeGenService"></param>
public class CommandService(
    SolutionContext projectContext,
    SolutionService solutionService,
    CodeGenService codeGenService,
    ModuleInstallService moduleInstallService
)
{
    public string? ErrorMsg { get; set; }

    public async Task<bool> CreateSolutionAsync(CreateSolutionDto dto)
    {
        dto.ApplyTemplateDefaults();

        // 生成项目
        string solutionPath = Path.Combine(dto.Path, dto.Name);
        string templateType = dto.IsLight ? ConstVal.Mini : ConstVal.WebApi;

        if (!ProcessHelper.RunCommand("dotnet", $"new list {ConstVal.WebApi}", out string _))
        {
            ProcessHelper.RunCommand(
              "dotnet",
              $"new install {ConstVal.TemplatePackageId}",
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
                $"new {templateType} -o {solutionPath} --force {templateOptions}",
                out string error
            )
        )
        {
            OutputHelper.Error(error);
            ErrorMsg = "Create failed, check the error output.";
            return false;
        }
        OutputHelper.Success($"Created new solution {solutionPath}");

        projectContext.SetSolution(solutionPath);

        OutputHelper.Important($"Apply settings...");

        // 更新服务的配置文件
        var services = solutionService.GetServices();
        if (services != null)
        {
            foreach (var service in services)
            {
                var servicePath = Path.Combine(solutionPath, PathConst.ServicesPath, service.Name);
                UpdateAppSettings(dto, servicePath);
            }
        }
        // 更新Aspire Host配置文件
        var aspirePath = Path.Combine(solutionPath, PathConst.AppHostPath);
        UpdateAppSettings(dto, aspirePath);

        // 前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            string appPath = Path.Combine(
                solutionPath,
                "src",
                "ClientApp",
                "WebApp"
            );
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }
        if (projectContext.EntityPath.NotEmpty())
        {
            SolutionService.BuildProject(projectContext.EntityPath);
        }
        if (projectContext.EntityFrameworkPath.NotEmpty())
        {
            SolutionService.BuildProject(projectContext.EntityFrameworkPath);
        }
        // restore dotnet tools
        if (!ProcessHelper.RunCommand("dotnet", "tool restore", out string restoreMsg))
        {
            OutputHelper.Error(restoreMsg);
        }

        if (!dto.IsLight && dto.OfficialModules.Count > 0)
        {
            var targetService = solutionService.GetServices().FirstOrDefault();
            if (targetService == null)
            {
                ErrorMsg = "Install official modules failed: service project not found.";
                OutputHelper.Error(ErrorMsg);
                return false;
            }

            foreach (var packageName in dto.OfficialModules.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                OutputHelper.Important($"Install official module:{packageName}");
                var installed = await moduleInstallService.InstallModuleAsync(
                    packageName,
                    targetService.Name
                );
                if (!installed)
                {
                    ErrorMsg = $"Install official module {packageName} failed.";
                    return false;
                }
            }
        }

        OutputHelper.Success($"Create solution {dto.Name} completed!");
        return true;
    }


    public async Task GenerateRequestClientAsync(
        string url,
        string outputPath,
        RequestClientType type,
        bool onlyModels = false,
        bool coverBaseService = false
    )
    {
        try
        {
            List<GenFileInfo> genFiles = [];
            if (type == RequestClientType.CSharp)
            {
                genFiles = await codeGenService.GenerateCsharpApiClientAsync(url, outputPath, onlyModels);
            }
            else
            {
                genFiles = await codeGenService.GenerateWebRequestAsync(
                   url,
                   outputPath,
                   type,
                   onlyModels,
                   coverBaseService
               ) ?? [];
            }
            codeGenService.GenerateFiles(genFiles);
        }
        catch (Exception ex)
        {
            OutputHelper.Error(ex.Message + ex.StackTrace);
        }
    }

    /// <summary>
    /// 更新配置文件
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="path"></param>
    private static void UpdateAppSettings(CreateSolutionDto dto, string path)
    {
        // 修改配置文件
        string configFile = Path.Combine(path, ConstVal.AppSettingDevelopmentJson);
        if (!File.Exists(configFile))
            return;
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
