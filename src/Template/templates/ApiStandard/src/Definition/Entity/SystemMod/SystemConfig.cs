namespace Entity.SystemMod;

/// <summary>
/// 系统配置
/// </summary>
[Index(nameof(Key))]
[Index(nameof(IsSystem))]
[Index(nameof(Valid))]
[Index(nameof(GroupName))]
public class SystemConfig : EntityBase
{
    [MaxLength(100)]
    public required string Key { get; set; }

    /// <summary>
    /// 以json字符串形式存储
    /// </summary>
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
    public bool Valid { get; set; } = true;

    /// <summary>
    /// 是否属于系统配置
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 组
    /// </summary>
    [MaxLength(60)]
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 创建系统配置
    /// </summary>
    /// <param name="groupName">分组名称</param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static SystemConfig NewSystemConfig(string groupName, string key, string value)
    {
        return new SystemConfig
        {
            Key = key,
            Value = value,
            GroupName = groupName,
            IsSystem = true,
        };
    }
}
