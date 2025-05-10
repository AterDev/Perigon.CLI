namespace StudioMod.Models;
/// <summary>
/// 创建UI组件结果
/// </summary>
public class CreateUIResult
{
    /// <summary>
    /// 页面内容
    /// </summary>
    public string Html { get; set; } = string.Empty;
    /// <summary>
    /// ts内容
    /// </summary>
    public string Typescript { get; set; } = string.Empty;
}
