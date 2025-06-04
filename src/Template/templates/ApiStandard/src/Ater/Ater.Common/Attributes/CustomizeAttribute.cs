namespace Ater.Common.Attributes;
/// <summary>
/// 模块标记
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ModuleAttribute(string name) : Attribute
{
    /// <summary>
    /// 模块名称，区分大小写
    /// </summary>
    public string Name { get; init; } = name;
}