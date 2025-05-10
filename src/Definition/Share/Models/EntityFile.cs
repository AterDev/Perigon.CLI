
namespace Share.Models;

public class EntityFile
{
    public required string Name { get; set; }
    /// <summary>
    /// 注释说明
    /// </summary>
    public string? Comment { get; set; }
    public string BaseDirPath { get; set; } = string.Empty;
    public required string FullName { get; set; }
    public string? Content { get; set; }
    /// <summary>
    /// 所属模块
    /// </summary>
    public string? ModuleName { get; set; }

    public bool HasDto { get; set; }
    public bool HasManager { get; set; }
    public bool HasAPI { get; set; }

    /// <summary>
    /// Dto models path
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public string GetDtoPath(IProjectContext project)
    {
        var name = Path.GetFileNameWithoutExtension(Name);
        return ModuleName.IsEmpty()
            ? Path.Combine(project.SharePath ?? PathConst.SharePath, ConstVal.ModelsDir, $"{name}Dtos")
            : Path.Combine(project.ModulesPath ?? PathConst.ModulesPath, ModuleName, ConstVal.ModelsDir, $"{name}Dtos");
    }
    /// <summary>
    /// 获取manager路径
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public string GetManagerPath(IProjectContext project)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(project.ApplicationPath ?? PathConst.ApplicationPath, ConstVal.ManagersDir)
            : Path.Combine(project.ModulesPath ?? PathConst.ModulesPath, ModuleName, ConstVal.ManagersDir);
    }

    /// <summary>
    /// controller Path
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public string GetControllerPath(IProjectContext project)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(project.ApiPath ?? PathConst.APIPath, ConstVal.ControllersDir)
            : Path.Combine(project.ModulesPath ?? PathConst.ModulesPath, ModuleName, ConstVal.ControllersDir);
    }
}
