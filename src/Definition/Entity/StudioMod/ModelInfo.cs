namespace Entity.StudioMod;

/// <summary>
/// 模型信息
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// file content md5 hash
    /// </summary>
    [MaxLength(32)]
    public required string Md5Hash { get; set; }

    /// <summary>
    /// module name
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// file path
    /// </summary>
    [MaxLength(200)]
    public required string FilePath { get; set; }

    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    [MaxLength(100)]
    public required string NamespaceName { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    [MaxLength(100)]
    public string? AssemblyName { get; set; }

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

    public Solution Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;

    /// <summary>
    /// 属性
    /// </summary>
    public List<ModelProperty> PropertyInfos { get; set; } = [];
}

public enum EntityKeyType
{
    Guid,
    Int,
    String,
}
