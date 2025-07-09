using Entity;
using Microsoft.AspNetCore.Http;

namespace Share;

/// <summary>
/// 项目上下文
/// </summary>
public class ProjectContext : IProjectContext
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Project? Project { get; set; }
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? CommonModPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }
    public string? EntityFrameworkPath { get; set; }
    public string? ModulesPath { get; set; }
    public string? ServicesPath { get; set; }

    private readonly CommandDbContext _context;

    public ProjectContext(
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<CommandDbContext> contextFactory
    )
    {
        _context = contextFactory.CreateDbContext();
        string? id = httpContextAccessor.HttpContext?.Request.Headers["projectId"].ToString();

        if (!string.IsNullOrWhiteSpace(id))
        {
            if (Guid.TryParse(id, out Guid projectId))
            {
                ProjectId = projectId;
                Project = _context.Projects.Find(projectId);
                if (Project != null)
                {
                    SolutionPath = Project.Path;
                    var config = Project.Config;
                    SharePath = Path.Combine(SolutionPath, config.SharePath);
                    CommonModPath = Path.Combine(SolutionPath, config.CommonModPath);
                    EntityPath = Path.Combine(SolutionPath, config.EntityPath);
                    ApiPath = Path.Combine(SolutionPath, config.ApiPath);
                    EntityFrameworkPath = Path.Combine(SolutionPath, config.EntityFrameworkPath);
                    ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
                    ServicesPath = Path.Combine(SolutionPath, PathConst.ServicesPath);
                }
            }
            else
            {
                throw new NullReferenceException("未获取到有效的ProjectId");
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task SetProjectByIdAsync(Guid id)
    {
        ProjectId = id;
        Project = await _context.Projects.FindAsync(id);
        if (Project != null)
        {
            ProjectName = Project.Name;
            SolutionPath = Project.Path;
            var config = Project.Config;
            SharePath = Path.Combine(SolutionPath, config.SharePath);
            CommonModPath = Path.Combine(SolutionPath, config.CommonModPath);
            EntityPath = Path.Combine(SolutionPath, config.EntityPath);
            ApiPath = Path.Combine(SolutionPath, config.ApiPath);
            EntityFrameworkPath = Path.Combine(SolutionPath, config.EntityFrameworkPath);
            ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
            ServicesPath = Path.Combine(SolutionPath, PathConst.ServicesPath);
        }
    }

    public async Task SetProjectAsync(string solutionPath)
    {
        SolutionPath = solutionPath;
        var project = await _context
            .Projects.Where(p => p.Path.Equals(solutionPath))
            .FirstOrDefaultAsync();
        var config = project?.Config;
        SharePath = Path.Combine(SolutionPath, config?.SharePath ?? PathConst.SharePath);
        CommonModPath = Path.Combine(
            SolutionPath,
            config?.CommonModPath ?? PathConst.CommonModPath
        );
        EntityPath = Path.Combine(SolutionPath, config?.EntityPath ?? PathConst.EntityPath);
        ApiPath = Path.Combine(SolutionPath, config?.ApiPath ?? PathConst.APIPath);
        EntityFrameworkPath = Path.Combine(
            SolutionPath,
            config?.EntityFrameworkPath ?? PathConst.EntityFrameworkPath
        );
        ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
        ServicesPath = Path.Combine(SolutionPath, PathConst.ServicesPath);
    }

    /// <summary>
    /// get share(dto) path
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public string GetDtoPath(string entityName, string? moduleName = null)
    {
        var name = Path.GetFileNameWithoutExtension(entityName);
        return moduleName.IsEmpty()
            ? Path.Combine(SharePath ?? PathConst.SharePath, ConstVal.ModelsDir, $"{name}Dtos")
            : Path.Combine(
                ModulesPath ?? PathConst.ModulesPath,
                moduleName,
                ConstVal.ModelsDir,
                $"{name}Dtos"
            );
    }

    public string GetModulePath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(CommonModPath ?? PathConst.CommonModPath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }

    /// <summary>
    /// 获取manager路径
    /// </summary>
    /// <returns></returns>
    public string GetManagerPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(CommonModPath ?? PathConst.CommonModPath, ConstVal.ManagersDir)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName, ConstVal.ManagersDir);
    }

    /// <summary>
    /// controller Path
    /// </summary>
    /// <returns></returns>
    public string GetControllerPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(
                ModulesPath ?? PathConst.ModulesPath,
                ConstVal.CommonMod,
                ConstVal.ControllersDir
            )
            : Path.Combine(
                ModulesPath ?? PathConst.ModulesPath,
                moduleName,
                ConstVal.ControllersDir
            );
    }

    public string GetApiPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(ApiPath ?? PathConst.APIPath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }

    public string GetApplicationPath(string? moduleName = null)
    {
        throw new NotImplementedException();
    }

    public Task SetProjectByIdAsync(string id)
    {
        throw new NotImplementedException();
    }
}
