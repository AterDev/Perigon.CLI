namespace StudioMod.Models.ApiDocInfoDtos;
/// <summary>
/// 接口文档添加时请求结构
/// </summary>
/// <see cref="ApiDocInfo"/>
public class ApiDocInfoAddDto
{
    /// <summary>
    /// 文档名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    /// <summary>
    /// 文档描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    /// <summary>
    /// 文档地址
    /// </summary>
    [MaxLength(300)]
    public required string Path { get; set; }
    /// <summary>
    /// 生成路径
    /// </summary>
    [MaxLength(200)]
    public string? LocalPath { get; set; }

}
