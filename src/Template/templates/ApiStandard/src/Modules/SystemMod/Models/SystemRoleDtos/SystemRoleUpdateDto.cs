namespace SystemMod.Models.SystemRoleDtos;

/// <summary>
/// 角色更新时请求结构
/// </summary>
/// <inheritdoc cref="SystemRole"/>
public class SystemRoleUpdateDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [MaxLength(30)]
    public string? Name { get; set; }

    /// <summary>
    /// 角色标识
    /// </summary>
    [MaxLength(60)]
    public string? NameValue { get; set; }

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool? IsSystem { get; set; }
}
