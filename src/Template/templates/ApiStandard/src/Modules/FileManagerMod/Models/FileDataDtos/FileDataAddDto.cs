namespace FileManagerMod.Models.FileDataDtos;
/// <summary>
/// 文件数据添加时请求结构
/// </summary>
/// <see cref="Entity.FileManagerMod.FileData"/>
public class FileDataAddDto
{
    /// <summary>
    /// 文件名
    /// </summary>
    [MaxLength(200)]
    public required string FileName { get; set; }
    /// <summary>
    /// 扩展名
    /// </summary>
    [MaxLength(20)]
    public string? Extension { get; set; }
    /// <summary>
    /// md5值
    /// </summary>
    [MaxLength(100)]
    public required string Md5 { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(1024 * 1024 * 2)]
    public required byte[] Content { get; set; }

}
