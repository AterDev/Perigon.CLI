using Entity;
using Microsoft.AspNetCore.Http;

namespace Share;

/// <summary>
/// 项目上下文
/// </summary>
public class ProjectContext : IProjectContext
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? ApplicationPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }
    public string? EntityFrameworkPath { get; set; }
    public string? ModulesPath { get; set; }

    public ProjectContext(IHttpContextAccessor httpContextAccessor, CommandDbContext context)
    {
        string? id = httpContextAccessor.HttpContext?.Request.Headers["projectId"].ToString();
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (Guid.TryParse(id, out Guid projectId))
            {
                ProjectId = projectId;
                Project = context.Projects.Find(projectId);
                if (Project != null)
                {
                    ConstVal.PROJECT_ID = projectId;
                    SolutionPath = Project.Path;
                    var config = Project.Config;
                    SharePath = Path.Combine(SolutionPath, config.SharePath);
                    ApplicationPath = Path.Combine(SolutionPath, config.ApplicationPath);
                    EntityPath = Path.Combine(SolutionPath, config.EntityPath);
                    ApiPath = Path.Combine(SolutionPath, config.ApiPath);
                    EntityFrameworkPath = Path.Combine(SolutionPath, config.EntityFrameworkPath);
                    ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
                }
            }
            else
            {
                throw new NullReferenceException("未获取到有效的ProjectId");
            }
        }
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
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName, ConstVal.ModelsDir, $"{name}Dtos");
    }


    public string GetSharePath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(SharePath ?? PathConst.SharePath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }


    /// <summary>
    /// 获取manager路径
    /// </summary>
    /// <returns></returns>
    public string GetManagerPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(ApplicationPath ?? PathConst.CommonModPath, ConstVal.ManagersDir)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName, ConstVal.ManagersDir);
    }

    public string GetApplicationPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(ApplicationPath ?? PathConst.CommonModPath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }

    /// <summary>
    /// controller Path
    /// </summary>
    /// <returns></returns>
    public string GetControllerPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(ApiPath ?? PathConst.APIPath, ConstVal.ControllersDir)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName, ConstVal.ControllersDir);
    }

    public string GetApiPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(ApiPath ?? PathConst.APIPath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }
}
