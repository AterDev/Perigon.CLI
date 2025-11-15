namespace SystemMod.Models.SystemUserDtos;

/// <summary>
/// 系统用户列表元素
/// </summary>
/// <inheritdoc cref="SystemUser"/>
public class SystemUserItemDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(30)]
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(30)]
    public string? RealName { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTimeOffset? LastLoginTime { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderType Sex { get; set; } = GenderType.Male;
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
}
