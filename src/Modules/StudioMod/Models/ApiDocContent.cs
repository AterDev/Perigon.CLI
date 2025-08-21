namespace StudioMod.Models;

/// <summary>
/// 接口返回模型
/// </summary>
public class ApiDocContent
{
    /// <summary>
    /// 接口信息
    /// </summary>
    public List<RestApiGroup> RestApiGroups { get; set; } = [];

    /// <summary>
    /// 所有请求及返回类型信息
    /// </summary>
    public List<TypeMeta> TypeMeta { get; set; } = [];

    /// <summary>
    /// tag信息
    /// </summary>
    public List<ApiDocTag> OpenApiTags { get; set; } = [];
}
