namespace SystemMod.Models.SystemRoleDtos;

/// <summary>
/// 角色列表元素
/// </summary>
/// <inheritdoc cref="SystemRole"/>
public class SystemRoleItemDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 角色名称
    /// </summary>
    [MaxLength(30)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 角色标识
    /// </summary>
    public string NameValue { get; set; } = default!;

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool IsSystem { get; set; }

    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
}
