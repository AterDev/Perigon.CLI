namespace StudioMod.Models.ApiDocInfoDtos;
/// <summary>
/// 接口文档概要
/// </summary>
/// <see cref="ApiDocInfo"/>
public class ApiDocInfoShortDto
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
    /// 生成路径
    /// </summary>
    [MaxLength(200)]
    public string? LocalPath { get; set; }
    public Project Project { get; set; } = default!;

}
