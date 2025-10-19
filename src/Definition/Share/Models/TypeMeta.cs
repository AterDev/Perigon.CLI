using CodeGenerator.Helper;

namespace Share.Models;
/// <summary>
/// 模型信息
/// </summary>
public class TypeMeta
{
    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 类注释
    /// </summary>
    [MaxLength(300)]
    public string? Comment { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    [MaxLength(100)]
    public string? Summary { get; set; }
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;

    /// <summary>
    /// 是否为枚举类
    /// </summary>
    public bool? IsEnum { get; set; } = false;
    public bool IsList { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    public List<PropertyInfo> PropertyInfos { get; set; } = [];

    /// <summary>
    /// 标准化名称
    /// </summary>
    /// <returns></returns>
    public string FormatTypeName => OpenApiHelper.FormatSchemaKey(Name);
}
