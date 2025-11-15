namespace SystemMod.Models.SystemUserDtos;

/// <summary>
/// 系统用户概要
/// </summary>
/// <inheritdoc cref="SystemUser"/>
public class SystemUserDetailDto
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
    public bool EmailConfirmed { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTimeOffset? LastLoginTime { get; set; }

    /// <summary>
    /// 密码重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [MaxLength(200)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderType Sex { get; set; } = GenderType.Male;
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
}
