namespace CodeGenerator.Models;
/// <summary>
/// 服务文件
/// </summary>
public class RequestServiceFile
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<RequestServiceFunction>? Functions { get; set; }
}

/// <summary>
/// 请求服务的函数
/// </summary>
public class RequestServiceFunction
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Method { get; set; } = default!;
    public string? ResponseType { get; set; }
    /// <summary>
    /// 返回中的引用类型
    /// </summary>
    public string? ResponseRefType { get; set; }
    public string RequestType { get; set; } = string.Empty;
    /// <summary>
    /// 请求中的引用类型
    /// </summary>
    public string? RequestRefType { get; set; }
    /// <summary>
    /// 参数及类型
    /// </summary>
    public List<FunctionParams>? Params { get; set; }
    /// <summary>
    /// 相对请求路径
    /// </summary>
    public string Path { get; set; } = default!;
    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }

}

/// <summary>
/// 函数参数
/// </summary>
public class FunctionParams
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = true;
    /// <summary>
    /// 是否路由参数
    /// </summary>
    public bool InPath { get; set; }
}
