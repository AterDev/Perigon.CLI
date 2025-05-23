using System.Text.Json.Nodes;
using CodeGenerator.Helper;
using Entity;
using Share.Models.CommandDtos;

namespace Share.Services;
/// <summary>
///  command service
/// </summary>
/// <param name="context"></param>
public class CommandService(CommandDbContext context)
{
    public string? ErrorMsg { get; set; }

    public async Task<Guid?> AddProjectAsync(string name, string path)
    {
        var projectFilePath = Directory.GetFiles(path, $"*{ConstVal.SolutionExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();

        projectFilePath ??= Directory.GetFiles(path, $"*{ConstVal.SolutionXMLExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();

        projectFilePath ??= Directory.GetFiles(path, $"*{ConstVal.CSharpProjectExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(path, ConstVal.NodeProjectFile, SearchOption.TopDirectoryOnly).FirstOrDefault();


        var solutionType = AssemblyHelper.GetSolutionType(projectFilePath);
        var solutionName = Path.GetFileName(projectFilePath) ?? name;
        var solutionPath = Path.GetDirectoryName(projectFilePath) ?? "";
        var entity = new Project()
        {
            DisplayName = name,
            Path = solutionPath,
            Name = solutionName,
            SolutionType = solutionType
        };
        entity.Config.SolutionPath = solutionPath;

        context.Projects.Add(entity);
        return await context.SaveChangesAsync() > 0 ? entity.Id : null;
    }

    public async Task<bool> CreateSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        string solutionPath = Path.Combine(dto.Path, dto.Name);
        string apiName = ConstVal.APIName;
        string templateType = dto.IsLight ? ConstVal.Mini : ConstVal.Standard;

        string version = AssemblyHelper.GetCurrentToolVersion();

        if (ProcessHelper.RunCommand("dotnet", $"new list ater-{templateType}", out _))
        {
            ProcessHelper.RunCommand("dotnet", $"new update", out _);
        }
        else
        {
            ProcessHelper.RunCommand("dotnet", $"new install ater.web.templates::{version}", out string msg);
            OutputHelper.Info(msg);
        }

        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(solutionPath);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new ater-{templateType} -o {solutionPath} --force", out string error))
        {
            OutputHelper.Error(error);
            ErrorMsg = "Create failed, check the error output.";
            return false;
        }

        OutputHelper.Success($"create new solution {solutionPath}");

        // 更新配置文件
        UpdateAppSettings(dto, solutionPath, apiName);

        // 前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            string appPath = Path.Combine(solutionPath, "src", "ClientApp", "WebApp");
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }

        if (!dto.IsLight && dto.Modules.Count > 0)
        {
            foreach (string item in dto.Modules)
            {
                // 添加模块到解决方案中
                SolutionService.AddDefaultModule(item, solutionPath);
            }
        }

        // 保存项目信息
        var projectFilePath = Directory.GetFiles(solutionPath, $"*{ConstVal.SolutionExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(solutionPath, $"*{ConstVal.SolutionXMLExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(solutionPath, $"*{ConstVal.CSharpProjectExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(solutionPath, ConstVal.NodeProjectFile, SearchOption.TopDirectoryOnly).FirstOrDefault();


        var solutionType = AssemblyHelper.GetSolutionType(projectFilePath);
        var solutionName = Path.GetFileName(projectFilePath) ?? dto.Name;
        var entity = new Project()
        {
            DisplayName = dto.Name,
            Path = solutionPath,
            Name = solutionName,
            SolutionType = solutionType
        };
        entity.Config.SolutionPath = solutionPath;
        await context.Projects.AddAsync(entity);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新配置文件
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="path"></param>
    /// <param name="apiName"></param>
    private static void UpdateAppSettings(CreateSolutionDto dto, string path, string apiName)
    {
        // 修改配置文件
        string configFile = Path.Combine(path, "src", apiName, "appsettings.json");
        string jsonString = File.ReadAllText(configFile);
        JsonNode? jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });
        if (jsonNode != null)
        {
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString());

            if (dto.CommandDbConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CommandDb", dto.CommandDbConnStrings);
            }

            if (dto.QueryDbConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.QueryDb", dto.QueryDbConnStrings);
            }
            if (dto.CacheConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.Cache", dto.CacheConnStrings);
            }
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CacheInstanceName", dto.CacheInstanceName ?? "Dev");

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }
    }
}