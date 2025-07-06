namespace StudioMod.Managers;

public class ProjectManager(
    DataAccessContext<Project> dataContext,
    ILogger<ProjectManager> logger,
    CommandService commandService
) : ManagerBase<Project>(dataContext, logger)
{
    public string GetToolVersion()
    {
        return AssemblyHelper.GetCurrentToolVersion();
    }

    /// <summary>
    /// 获取项目列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Project>> ListAsync()
    {
        var projects = await Command.ToListAsync();
        for (int i = 0; i < projects.Count; i++)
        {
            var p = projects[i];
            // 移除不存在的项目
            if (!Directory.Exists(p.Path))
            {
                Command.Remove(p);
                projects.Remove(p);
            }
            await SaveChangesAsync();
        }
        return projects;
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="name"></param>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(string name, string projectPath)
    {
        return await commandService.AddProjectAsync(name, projectPath);
    }

    public async Task<Project?> GetDetailAsync(Guid id)
    {
        return await FindAsync(id);
    }

    /// <summary>
    /// 打开项目
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string OpenSolution(string path)
    {
        string res = ProcessHelper.ExecuteCommands($"start {path}");
        return res;
    }

    /// <summary>
    /// 更新配置内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateConfigAsync(Project project, ProjectConfig dto)
    {
        project!.Config = dto;
        return await UpdateAsync(project);
    }
}
