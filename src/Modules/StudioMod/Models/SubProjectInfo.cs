
namespace StudioMod.Models;
/// <summary>
/// 项目信息
/// </summary>
public class SubProjectInfo
{
    public required string Name { get; set; }
    public required string Path { get; set; }

    public ProjectType ProjectType { get; set; } = ProjectType.Web;
}

public enum ProjectType
{
    /// <summary>
    /// web服务
    /// </summary>
    [Description("web服务")]
    Web,
    /// <summary>
    /// 控制台应用
    /// </summary>
    [Description("控制台应用")]
    Console,
    /// <summary>
    /// 类库
    /// </summary>
    [Description("类库")]
    Lib,
    /// <summary>
    /// 模块
    /// </summary>
    [Description("模块")]
    Module,
    /// <summary>
    /// 接口服务
    /// </summary>
    [Description("接口服务")]
    WebAPI,
    /// <summary>
    /// gPRC服务
    /// </summary>
    [Description("gPRC服务")]
    GRPC
}
