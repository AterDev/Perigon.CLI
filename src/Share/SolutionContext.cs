namespace Share;

/// <summary>
/// 项目上下文
/// </summary>
public class SolutionContext
{
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? CommonModPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }
    public string? EntityFrameworkPath { get; set; }
    public string? ModulesPath { get; set; }
    public string? ServicesPath { get; set; }

    public SolutionConfig? SolutionConfig { get; set; }

    public void SetSolution(string solutionPath)
    {
        SolutionPath = Path.GetFullPath(solutionPath);
        SolutionConfig = new SolutionConfig();
        SharePath = Path.Combine(SolutionPath, PathConst.SharePath);
        CommonModPath = Path.Combine(SolutionPath, PathConst.CommonModPath);
        EntityPath = Path.Combine(SolutionPath, PathConst.EntityPath);
        ApiPath = Path.Combine(SolutionPath, PathConst.APIPath);
        EntityFrameworkPath = Path.Combine(SolutionPath, PathConst.EntityFrameworkPath);
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
    /// controller TemplatePath
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

}
