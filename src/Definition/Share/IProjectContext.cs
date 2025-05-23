namespace Share;
/// <summary>
/// 项目上下文
/// </summary>
public interface IProjectContext
{
    Guid ProjectId { get; set; }
    Project? Project { get; set; }
    string? SolutionPath { get; set; }
    string? SharePath { get; set; }
    string? CommonModPath { get; set; }
    string? EntityPath { get; set; }
    string? ApiPath { get; set; }
    string? EntityFrameworkPath { get; set; }
    string? ModulesPath { get; set; }

    /// <summary>
    /// 获取Api目录路径
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns>模块存在时，返回模块项目路径</returns>
    string GetApiPath(string? moduleName = null);
    string GetControllerPath(string? moduleName = null);
    string GetDtoPath(string entityName, string? moduleName = null);
    string GetManagerPath(string? moduleName = null);

    /// <summary>
    /// 获取Share目录路径
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns>模块存在时，返回模块项目路径</returns>
    string GetModulePath(string? moduleName = null);

    Task SetProjectAsync(string solutionPath);
}
