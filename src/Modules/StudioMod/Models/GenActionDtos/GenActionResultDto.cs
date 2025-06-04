namespace StudioMod.Models.GenActionDtos;
/// <summary>
/// 执行结果
/// </summary>
public class GenActionResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMsg { get; set; }

    public List<ModelFileItemDto> OutputFiles { get; set; } = [];

}
