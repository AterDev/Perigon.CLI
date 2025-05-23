namespace StudioMod.Models;

public class ConvertDto
{
    public required string Content { get; set; }

    /// <summary>
    /// 类名称
    /// </summary>
    public string? ClassName { get; set; }
}
