using System.Text.Json.Nodes;
using Share.Models.CommandDtos;

namespace StudioMod.Managers;

/// <summary>
/// 功能集成
/// </summary>
public class SolutionManager(
    IProjectContext projectContext,
    ProjectManager projectManager,
    ILogger<SolutionManager> logger,
    CommandService commandService,
    SolutionService solution
) : ManagerBase(logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly ProjectManager _projectManager = projectManager;
    private readonly SolutionService _solution = solution;
    private readonly CommandService _commandService = commandService;

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
        return await _commandService.CreateSolutionAsync(dto);
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
        JsonNode? jsonNode = JsonNode.Parse(
            jsonString,
            documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }
        );
        if (jsonNode != null)
        {
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString());
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "Key.DefaultPassword",
                dto.DefaultPassword ?? ""
            );
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "ConnectionStrings.CommandDb",
                dto.CommandDbConnStrings ?? ""
            );
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "ConnectionStrings.QueryDb",
                dto.QueryDbConnStrings ?? ""
            );
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "ConnectionStrings.Cache",
                dto.CacheConnStrings ?? ""
            );
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "ConnectionStrings.CacheInstanceName",
                dto.CacheInstanceName ?? ""
            );

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }
    }

    /// <summary>
    /// 获取模块信息
    /// </summary>
    /// <returns></returns>
    public List<SubProjectInfo> GetModules()
    {
        List<SubProjectInfo> res = [];
        if (!Directory.Exists(_projectContext.ModulesPath!))
        {
            return [];
        }
        var projectFiles =
            Directory
                .GetFiles(
                    _projectContext.ModulesPath!,
                    $"*{ConstVal.CSharpProjectExtension}",
                    SearchOption.AllDirectories
                )
                .ToList() ?? [];

        projectFiles.ForEach(path =>
        {
            SubProjectInfo moduleInfo = new()
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Path = path,
                ProjectType = ProjectType.Module,
            };
            res.Add(moduleInfo);
        });
        return res;
    }

    /// <summary>
    /// 获取服务列表
    /// </summary>
    /// <returns></returns>
    public List<SubProjectInfo> GetServices(bool onlyWeb = true)
    {
        return _solution.GetServices(onlyWeb);
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

    /// <summary>
    /// 移除模块
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public bool DeleteModule(string moduleName)
    {
        try
        {
            _solution.DeleteModule(moduleName);
            return true;
        }
        catch (Exception ex)
        {
            ErrorMsg = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 清理解决方案
    /// </summary>
    /// <returns></returns>
    public bool CleanSolution()
    {
        try
        {
            if (_solution.CleanSolution(out string error))
            {
                return true;
            }
            else
            {
                ErrorMsg = error;
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMsg = ex.Message;
            return false;
        }
    }

    public async Task<bool> CreateServiceAsync(string serviceName)
    {
        var (res, error) = await _solution.CreateServiceAsync(serviceName);
        if (res)
        {
            return true;
        }
        else
        {
            ErrorMsg = error ?? string.Empty;
            return false;
        }
    }
}
