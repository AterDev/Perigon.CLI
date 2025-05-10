using System.Text.Json.Nodes;
using StudioMod.Models;
using StudioMod.Services;

namespace StudioMod.Managers;
/// <summary>
/// 功能集成
/// </summary>
public class SolutionManager(
    IProjectContext projectContext,
    ProjectManager projectManager,
    ILogger<SolutionManager> logger,
    SolutionService solution
    )
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly ProjectManager _projectManager = projectManager;
    private readonly ILogger<SolutionManager> _logger = logger;
    private readonly SolutionService _solution = solution;

    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    /// 获取默认模块
    /// </summary>
    /// <returns></returns>
    public List<ModuleInfo> GetDefaultModules()
    {
        return ModuleInfo.GetModules();
    }

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CreateNewSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        string solutionPath = Path.Combine(dto.Path, dto.Name);
        string apiName = "Http.API";
        string templateType = dto.IsLight ? "atlight" : "atapi";

        string version = AssemblyHelper.GetCurrentToolVersion();

        if (ProcessHelper.RunCommand("dotnet", $"new list atapi", out _))
        {
            ProcessHelper.RunCommand("dotnet", $"new update", out _);
        }
        else
        {
            ProcessHelper.RunCommand("dotnet", $"new install ater.web.templates::{version}", out string msg);
            Console.WriteLine(msg);
        }

        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(solutionPath);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new {templateType} -o {solutionPath} --force", out _))
        {
            ErrorMsg = "创建项目失败，请尝试使用空目录创建";
            return false;
        }
        await Console.Out.WriteLineAsync($"✅ create new solution {solutionPath}");

        // 更新配置文件
        UpdateAppSettings(dto, solutionPath, apiName);
        // 数据库选择
        ChooseDatabase(dto.DBType, solutionPath, dto.IsLight);

        // 前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            string appPath = Path.Combine(solutionPath, "src", "ClientApp");
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }

        if (!dto.IsLight)
        {
            List<string> allModules = ModuleInfo.GetModules().Select(m => m.Value).ToList();

            List<string> notChoseModules = allModules.Except(dto.Modules).ToList();
            foreach (string? item in notChoseModules)
            {
                _solution.CleanModule(item);
            }
            foreach (string item in dto.Modules)
            {
                SolutionService.AddDefaultModule(item, solutionPath);
            }
        }

        // 保存项目信息
        var projectFilePath = Directory.GetFiles(solutionPath, $"*{ConstVal.SolutionExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(solutionPath, $"*{ConstVal.SolutionXMLExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(solutionPath, $"*{ConstVal.CSharpProjectExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(solutionPath, ConstVal.NodeProjectFile, SearchOption.TopDirectoryOnly).FirstOrDefault();
        var id = await _projectManager.AddAsync(dto.Name, projectFilePath);
        // restore & build solution
        Console.WriteLine("⛏️ restore & build project!");
        if (!ProcessHelper.RunCommand("dotnet", $"build {solutionPath}", out _))
        {
            ErrorMsg = "项目创建成功，但构建失败，请查看错误信息!";
            return false;
        }
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
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Key.DefaultPassword", dto.DefaultPassword ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CommandDb", dto.CommandDbConnStrings ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.QueryDb", dto.QueryDbConnStrings ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.Cache", dto.CacheConnStrings ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CacheInstanceName", dto.CacheInstanceName ?? "");

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }
    }

    /// <summary>
    /// 不同数据库处理
    /// </summary>
    /// <param name="dBType"></param>
    /// <param name="path"></param>
    /// <param name="isLight"></param>
    private void ChooseDatabase(DBType dBType, string path, bool isLight)
    {
        List<string> packageNames =
        [
            "Microsoft.EntityFrameworkCore.SqlServer",
            "Npgsql.EntityFrameworkCore.PostgreSQL"
        ];

        string useMethod = "UseNpgsql";
        string packageName = "Npgsql.EntityFrameworkCore.PostgreSQL";

        if (dBType == DBType.SQLServer)
        {
            useMethod = "UseSqlServer";
            packageName = "Microsoft.EntityFrameworkCore.SqlServer";
        }

        string appServiceFile = Path.Combine(path, "src", "Application", "AppServiceExtensions.cs");
        string content = File.ReadAllText(appServiceFile);
        content = content.Replace("option.UseNpgsql", "option." + useMethod);
        File.WriteAllText(appServiceFile, content);

        string queryFactoryFile = Path.Combine(path, "src", "Definition", "EntityFramework", "DBProvider", "QueryDbContextFactory.cs");
        content = File.ReadAllText(queryFactoryFile);
        content = content.Replace("builder.UseNpgsql", "builder." + useMethod);
        File.WriteAllText(queryFactoryFile, content);

        string commandFactoryFile = Path.Combine(path, "src", "Definition", "EntityFramework", "DBProvider", "CommandDbContextFactory.cs");
        content = File.ReadAllText(commandFactoryFile);
        content = content.Replace("builder.UseNpgsql", "builder." + useMethod);
        File.WriteAllText(commandFactoryFile, content);

        // 项目引用处理
        string entityProjectFile = Path.Combine(path, "src", "Definition", "Definition.csproj");
        if (!isLight)
        {
            entityProjectFile = Path.Combine(path, "src", "Definition", "EntityFramework", "EntityFramework.csproj");
        }

        packageNames.Remove(packageName);

        // 移除无用包
        List<string> lines = File.ReadAllLines(entityProjectFile).ToList();
        foreach (string item in packageNames)
        {
            int index = lines.FindIndex(x => x.Contains(item));
            if (index > 0)
            {
                lines.RemoveAt(index);
            }
        }
        File.WriteAllLines(entityProjectFile, lines);
    }

    /// <summary>
    /// 获取模块信息
    /// </summary>
    /// <returns></returns>
    public List<SubProjectInfo> GetModulesInfo()
    {
        List<SubProjectInfo> res = [];
        if (!Directory.Exists(_projectContext.ModulesPath!))
        {
            return [];
        }
        var projectFiles = Directory.GetFiles(_projectContext.ModulesPath!, $"*{ConstVal.CSharpProjectExtension}", SearchOption.AllDirectories).ToList() ?? [];

        projectFiles.ForEach(path =>
        {
            SubProjectInfo moduleInfo = new()
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Path = path,
                ProjectType = ProjectType.Module
            };
            res.Add(moduleInfo);
        });
        return res;
    }

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="name"></param>
    public async Task<bool> CreateModuleAsync(string moduleName)
    {
        moduleName = moduleName.EndsWith("Mod") ? moduleName : moduleName + "Mod";
        try
        {
            await _solution.CreateModuleAsync(moduleName);
            return true;
        }
        catch (Exception ex)
        {
            ErrorMsg = ex.Message;
            return false;
        }
    }
}
