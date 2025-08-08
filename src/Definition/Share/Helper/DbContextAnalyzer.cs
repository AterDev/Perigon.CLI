using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Share.Helper;

public class ExternalDbContextAnalyzer
{
    private readonly Assembly _assembly;

    /// <summary>
    /// 初始化 <see cref="ExternalDbContextAnalyzer"/> 的新实例。
    /// </summary>
    /// <param name="assemblyPath">目标程序集的路径。</param>
    public ExternalDbContextAnalyzer(string assemblyPath)
    {
        _assembly = Assembly.LoadFrom(assemblyPath);
    }

    /// <summary>
    /// 获取所有继承自指定基类的 DbContext 的模型信息。
    /// </summary>
    /// <param name="baseContextTypeName">DbContext 基类的完全限定名，例如 "Context.ContextBase"</param>
    /// <returns>DbContext 名称和其模型信息的字典。</returns>
    public Dictionary<string, IModel> GetDbContextModels(string baseContextTypeName = "ContextBase")
    {
        var result = new Dictionary<string, IModel>();
        var contextTypes = GetDbContextTypes(baseContextTypeName);

        foreach (var contextType in contextTypes)
        {
            try
            {
                var model = GetModel(contextType);
                if (model != null)
                {
                    result.Add(contextType.Name, model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        return result;
    }

    private IModel? GetModel(Type contextType)
    {
        // Try to create DbContext instance using parameterless constructor
        var dbContextInstance = Activator.CreateInstance(contextType) as DbContext;
        if (dbContextInstance == null)
        {
            return null;
        }
        return dbContextInstance.Model;
    }

    private static DbContext? GetDbContext(IServiceProvider serviceProvider, Type contextType)
    {
        // 尝试从服务中获取 DbContext 实例
        var context = (DbContext?)serviceProvider.GetService(contextType);
        if (context != null)
        {
            return context;
        }

        // 如果无法直接解析，则尝试使用 DI 的 ActivatorUtilities 创建
        try
        {
            var created =
                ActivatorUtilities.CreateInstance(serviceProvider, contextType) as DbContext;
            if (created != null)
            {
                return created;
            }
        }
        catch
        {
            // 忽略异常并尝试回退
        }

        // 最后回退到无参构造函数创建
        return Activator.CreateInstance(contextType) as DbContext;
    }

    /// <summary>
    /// 从已加载的程序集中查找所有继承自指定基类的 DbContext 类型。
    /// </summary>
    /// <param name="baseContextTypeName">DbContext 基类的完全限定名。</param>
    /// <returns>符合条件的 DbContext 类型列表。</returns>
    private List<Type> GetDbContextTypes(string baseContextTypeName)
    {
        var contextTypes = new List<Type>();
        var baseType = _assembly.GetType(baseContextTypeName, throwOnError: true)!;

        foreach (var type in _assembly.GetTypes())
        {
            if (
                type.IsClass
                && !type.IsAbstract
                && type.IsPublic
                && baseType.IsAssignableFrom(type)
            )
            {
                contextTypes.Add(type);
            }
        }
        return contextTypes;
    }
}
