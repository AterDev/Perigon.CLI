using DataContext.DBProvider;
using Share.Models.CommandDtos;

namespace StudioMod.Managers;

/// <summary>
/// Solution manager
/// </summary>
public class SolutionManager(
    DefaultDbContext dbContext,
    IProjectContext projectContext,
    ILogger<SolutionManager> logger,
    CommandService commandService,
    SolutionService solution
) : ManagerBase<DefaultDbContext, Solution>(dbContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CommandService _commandService = commandService;
    private readonly SolutionService _solution = solution;

    protected override ICollection<Solution> GetCollection() => _dbContext.Solutions;

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CreateNewSolutionAsync(CreateSolutionDto dto)
    {
        return await _commandService.CreateSolutionAsync(dto);
    }

    /// <summary>
    /// 获取项目列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Solution>> ListAsync()
    {
        var collection = GetCollection();
        var projects = collection.ToList();
        for (int i = projects.Count - 1; i >= 0; i--)
        {
            var p = projects[i];
            // 移除不存在的项目
            if (!Directory.Exists(p.Path))
            {
                collection.Remove(p);
                projects.RemoveAt(i);
            }
        }
        await _dbContext.SaveChangesAsync();
        return projects;
    }

    public static string GetToolVersion()
    {
        return AssemblyHelper.GetCurrentToolVersion();
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="name"></param>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public async Task<int?> AddProjectAsync(string name, string projectPath)
    {
        return await _commandService.AddProjectAsync(name, projectPath);
    }

    /// <summary>
    /// 更新配置内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateConfigAsync(Solution project, SolutionConfig dto)
    {
        project!.Config = dto;
        return await UpdateAsync(project);
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

    /// <summary>
    /// open solution
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string OpenSolution(string path)
    {
        string res = ProcessHelper.ExecuteCommands($"start {path}");
        return res;
    }
}
