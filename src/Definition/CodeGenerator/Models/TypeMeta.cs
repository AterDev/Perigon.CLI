using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Models;

public class TypeMeta
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Namespace { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Comment { get; set; }

    [MaxLength(200)]
    public string? CommentSummary { get; set; }

    public bool IsNullable { get; set; }

    public ICollection<TypeMeta> GenericParams { get; set; } = [];
    public bool IsGeneric => GenericParams.Count > 0;

    public bool IsReference { get; set; }
    [MaxLength(200)]
    public string? ReferenceName { get; set; }

    public bool? IsEnum { get; set; } = false;

    public bool IsList { get; set; }

    // 属性集合 (保留原始 PropertyInfo 结构)
    public List<PropertyInfo> PropertyInfos { get; set; } = [];

    public string FormatTypeName => OpenApiHelper.FormatSchemaKey(Name);

}
