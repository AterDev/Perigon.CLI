using Microsoft.OpenApi.Models;
namespace Share.Models;

/// <summary>
/// 接口信息
/// </summary>
public class RestApiInfo
{
    /// <summary>
    /// 请求方式
    /// </summary>
    public OperationType OperationType { get; set; }
    /// <summary>
    /// 路由
    /// </summary>
    public required string Router { get; set; }
    /// <summary>
    /// 说明
    /// </summary>
    public string? Summary { get; set; }
    public string? Tag { get; set; }
    public required string OperationId { get; set; }
    /// <summary>
    /// 请求查询参数
    /// </summary>
    public List<PropertyInfo>? QueryParameters { get; set; }

    /// <summary>
    /// 请求类型参数
    /// </summary>
    public TypeMeta? RequestInfo { get; set; }
    /// <summary>
    /// 返回类型内容
    /// </summary>
    public TypeMeta? ResponseInfo { get; set; }
}

/// <summary>
/// 接口分组信息
/// </summary>
public class RestApiGroup
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<RestApiInfo> ApiInfos { get; set; } = [];
}