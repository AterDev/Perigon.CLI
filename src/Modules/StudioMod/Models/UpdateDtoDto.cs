namespace StudioMod.Models;

public class UpdateDtoDto
{
    /// <summary>
    /// 文件完整路径
    /// </summary>
    [MaxLength(200)]
    public required string FileName { get; set; }

    public required string Content { get; set; }
}
