namespace Ater.AspNetCore.Attributes;

/// <summary>
/// 为实体添加日志描述
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LogDescriptionAttribute(
    string description,
    string? filedName = null,
    string? moduleName = null
) : Attribute
{
    /// <summary>
    /// 实体描述
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// 要取的字段信息
    /// </summary>
    public string? FieldName { get; set; } = filedName;

    /// <summary>
    /// 所属模块名称
    /// </summary>
    public string? ModuleName { get; } = moduleName;
}
