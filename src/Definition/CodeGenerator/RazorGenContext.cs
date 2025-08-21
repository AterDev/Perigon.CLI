using Entity;
using Entity.StudioMod;
using RazorEngineCore;

namespace CodeGenerator;

/// <summary>
/// 代码生成下下文
/// </summary>
public class RazorGenContext
{
    public IRazorEngine RazorEngine { get; set; } = new RazorEngine();

    public string GenManager(string templateContent, ManagerViewModel model)
    {
        return GenCode(templateContent, model);
    }

    /// <summary>
    /// 特定类型生成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="templateContent"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public string GenCode<T>(string templateContent, T model)
    {
        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> template = RazorEngine.Compile<
            RazorEngineTemplateBase<T>
        >(templateContent);
        string result = template.Run(instance =>
        {
            instance.Model = model;
        });
        return result;
    }

    /// <summary>
    /// 变量模板替换
    /// </summary>
    /// <param name="templateContent"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public string GenTemplate(string templateContent, List<Variable> model)
    {
        // model to dictionary
        var dictionary = model.ToDictionary(v => v.Key, v => v.Value);

        var template = RazorEngine.Compile<RazorEngineTemplateBase<Dictionary<string, string>>>(
            templateContent
        );
        string result = template.Run(instance =>
        {
            instance.Model = dictionary;
        });
        return result;
    }

    public string GenTemplate(string templateContent, ActionRunModel model)
    {
        templateContent =
            $"@using {ConstVal.CoreLibName}.Utils;" + Environment.NewLine + templateContent;
        var dictionary = model
            .Variables.DistinctBy(v => v.Key)
            .ToDictionary(v => v.Key, v => v.Value);

        var template = RazorEngine.Compile<CustomTemplate>(
            templateContent,
            builder =>
            {
                builder.AddAssemblyReferenceByName("System.Collections");
                builder.AddAssemblyReferenceByName("System");
                builder.AddAssemblyReferenceByName($"{ConstVal.CoreLibName}");
                builder.AddAssemblyReferenceByName("Entity");
            }
        );

        string result = template.Run(instance =>
        {
            instance.Variables = dictionary;
            instance.ModelName = model.ModelName;
            instance.Namespace = model.Namespace;
            instance.PropertyInfos = model.PropertyInfos;
            instance.NewLine = Environment.NewLine;
            instance.AddPropertyInfos = model.AddPropertyInfos;
            instance.UpdatePropertyInfos = model.UpdatePropertyInfos;
            instance.DetailPropertyInfos = model.DetailPropertyInfos;
            instance.ItemPropertyInfos = model.ItemPropertyInfos;
            instance.FilterPropertyInfos = model.FilterPropertyInfos;
        });
        return result;
    }
}

public class CustomTemplate : RazorEngineTemplateBase
{
    public Dictionary<string, string> Variables { get; set; } = [];

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// 类型描述
    /// </summary>
    public string? Description { get; set; }

    public string NewLine { get; set; } = Environment.NewLine;

    public List<PropertyInfo> PropertyInfos { get; set; } = [];
    public List<PropertyInfo> AddPropertyInfos { get; set; } = [];
    public List<PropertyInfo> UpdatePropertyInfos { get; set; } = [];
    public List<PropertyInfo> DetailPropertyInfos { get; set; } = [];
    public List<PropertyInfo> ItemPropertyInfos { get; set; } = [];
    public List<PropertyInfo> FilterPropertyInfos { get; set; } = [];
}

public class ActionRunModel
{
    public List<Variable> Variables { get; set; } = [];

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// 类型描述
    /// </summary>
    public string? Description { get; set; }
    public List<PropertyInfo> PropertyInfos { get; set; } = [];
    public List<PropertyInfo> AddPropertyInfos { get; set; } = [];
    public List<PropertyInfo> UpdatePropertyInfos { get; set; } = [];
    public List<PropertyInfo> DetailPropertyInfos { get; set; } = [];
    public List<PropertyInfo> ItemPropertyInfos { get; set; } = [];
    public List<PropertyInfo> FilterPropertyInfos { get; set; } = [];
}
