using Entity;

namespace CodeGenerator.Helper;

public class DbContextAnalysisHelper
{
    public CompilationHelper CompilationHelper { get; init; }
    public string ProjectName { get; init; }
    public string DllPath { get; init; }
    public string? BaseDbContextName { get; init; }
    public List<INamedTypeSymbol> DbContextNamedTypeSymbols { get; init; }

    /// <summary>
    /// 分析 DbContext
    /// </summary>
    /// <param name="path">the project dir path contain dbcontexts, like entityframework dir path </param>
    /// <exception cref="FileNotFoundException"></exception>
    public DbContextAnalysisHelper(string path)
    {
        var csproj = Directory
            .GetFiles(path, $"*{ConstVal.CSharpProjectExtension}")
            .FirstOrDefault();
        if (csproj == null)
        {
            throw new FileNotFoundException("No .csproj file found in the specified path.", path);
        }
        ProjectName = Path.GetFileNameWithoutExtension(csproj);
        DllPath = GetProjectDllPath(path, ProjectName);
        if (string.IsNullOrEmpty(DllPath))
        {
            throw new FileNotFoundException(
                "Could not find project output DLL. Make sure the project is built in Debug configuration.",
                $"{ProjectName}.dll"
            );
        }

        CompilationHelper = new CompilationHelper(Path.GetDirectoryName(DllPath)!);
        BaseDbContextName = GetBaseDbContextName();
        DbContextNamedTypeSymbols = GetDbContextTypes();
    }

    /// <summary>
    /// 获取项目dll路径
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public static string GetProjectDllPath(string projectPath, string projectName)
    {
        // find <projectName>.dll in bin/Debug/net* folder
        string binPath = Path.Combine(projectPath, "bin", "Debug");
        if (Directory.Exists(binPath))
        {
            string[] dlls = Directory.GetFiles(
                binPath,
                $"{projectName}.dll",
                SearchOption.AllDirectories
            );
            if (dlls.Length > 0)
            {
                return dlls[0];
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取包含某个实体类型的DbContext
    /// </summary>
    /// <param name="entityName"></param>
    /// <returns></returns>
    public INamedTypeSymbol? GetDbContextType(string entityName)
    {
        foreach (var dbContextType in DbContextNamedTypeSymbols)
        {
            var properties = dbContextType.GetMembers().OfType<IPropertySymbol>();
            foreach (var prop in properties)
            {
                if (prop.Type is INamedTypeSymbol propertyType && propertyType.IsGenericType)
                {
                    if (
                        propertyType.Name.Equals("DbSet")
                        && propertyType.TypeArguments.Any(t => t.Name.Equals(entityName))
                    )
                    {
                        return dbContextType;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 获取基础DbContext名称
    /// </summary>
    /// <returns></returns>
    private string? GetBaseDbContextName()
    {
        // find abstract class inherit from DbContext
        var dbContextClass = CompilationHelper.AllClass.FirstOrDefault(c =>
            c.BaseType != null && c.BaseType.Name.Equals("DbContext") && c.IsAbstract
        );
        return dbContextClass?.ToDisplayString();
    }

    /// <summary>
    /// 获取所有DbContext类型
    /// </summary>
    /// <returns></returns>
    private List<INamedTypeSymbol> GetDbContextTypes()
    {
        if (BaseDbContextName == null)
        {
            return [];
        }
        // find all public non-abstract class inherit from baseDbContextName
        var dbContextSymbols = CompilationHelper
            .AllClass.Where(c =>
                c.BaseType != null
                && c.BaseType.ToDisplayString().Equals(BaseDbContextName)
                && !c.IsAbstract
                && c.DeclaredAccessibility == Accessibility.Public
            )
            .ToList();

        return dbContextSymbols;
    }
}
