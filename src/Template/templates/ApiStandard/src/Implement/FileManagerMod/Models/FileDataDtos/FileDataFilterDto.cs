using System.ComponentModel;
using Ater.Common.Models;

namespace FileManagerMod.Models.FileDataDtos;
/// <summary>
/// 文件数据查询筛选
/// </summary>
/// <see cref="Entity.FileManagerMod.FileData"/>
public class FileDataFilterDto : FilterBase
{
    /// <summary>
    /// 文件名
    /// </summary>
    [MaxLength(200)]
    public string? FileName { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    [MaxLength(20)]
    public FileType? FileType { get; set; }
    /// <summary>
    /// md5值
    /// </summary>
    [MaxLength(100)]
    public string? Md5 { get; set; }
    public Guid? FolderId { get; set; }
}
/// <summary>
/// 文件类型
/// </summary>
public enum FileType
{
    /// <summary>
    /// 图片
    /// </summary>
    [Description("图片")]
    Image,
    /// <summary>
    /// 文本
    /// </summary>
    [Description("文本")]
    Text,
    /// <summary>
    /// 压缩 
    /// </summary>
    [Description("压缩")]
    Compressed,
    /// <summary>
    /// 文档
    /// </summary>
    [Description("文档")]
    Docs
}
