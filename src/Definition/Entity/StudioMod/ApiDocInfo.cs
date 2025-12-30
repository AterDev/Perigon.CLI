namespace Entity.StudioMod;

/// <summary>
/// 接口文档
/// </summary>
public class ApiDocInfo : EntityBase
{
    /// <summary>
    /// 文档名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10000)]
    public string? Content { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 文档地址
    /// </summary>
    [MaxLength(300)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 生成路径
    /// </summary>
    [MaxLength(200)]
    public string? LocalPath { get; set; }

    /// <summary>
    /// project id
    /// </summary>
    public int ProjectId { get; set; }
}
