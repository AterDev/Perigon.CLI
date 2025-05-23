namespace StudioMod.Models.ApiDocInfoDtos;
/// <summary>
/// 接口文档列表元素
/// </summary>
/// <see cref="ApiDocInfo"/>
public class ApiDocInfoItemDto
{
    /// <summary>
    /// 文档名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// 文档地址
    /// </summary>
    [MaxLength(300)]
    public string Path { get; set; } = default!;

    /// <summary>
    /// 文档描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    /// <summary>
    /// 生成路径
    /// </summary>
    [MaxLength(200)]
    public string? LocalPath { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; }

}
