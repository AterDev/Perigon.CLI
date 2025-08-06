namespace SystemMod.Models.SystemRoleDtos;

/// <summary>
/// 角色添加时请求结构
/// </summary>
/// <inheritdoc cref="SystemRole"/>
public class SystemRoleAddDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [MaxLength(30)]
    public required string Name { get; set; }

    /// <summary>
    /// 角色标识
    /// </summary>
    [MaxLength(60)]
    public required string NameValue { get; set; } = string.Empty;

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool IsSystem { get; set; }
}
