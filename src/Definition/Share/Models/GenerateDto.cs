using System.ComponentModel;

namespace Share.Models;

public class GenerateDto
{
    public required string EntityPath { get; set; }
    public CommandType CommandType { get; set; }
    public string? ServicePath { get; set; }

    /// <summary>
    /// 是否覆盖
    /// </summary>
    public bool Force { get; set; }
}

/// <summary>
/// 命令类型
/// </summary>
public enum CommandType
{
    /// <summary>
    /// dto
    /// </summary>
    [Description("dto")]
    Dto,

    /// <summary>
    /// manager
    /// </summary>
    [Description("manager")]
    Manager,

    /// <summary>
    /// api
    /// </summary>
    [Description("api")]
    API,
}
