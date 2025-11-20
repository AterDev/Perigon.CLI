using System.Text.Json.Serialization;

namespace Entity.SystemMod;

/// <summary>
/// 系统用户
/// </summary>
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(PhoneNumber), IsUnique = true)]
[Index(nameof(CreatedTime))]
[Index(nameof(IsDeleted))]
public class SystemUser : EntityBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(30)]
    [MinLength(4)]
    public string? UserName { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(30)]
    [Length(2, 30)]
    public string? RealName { get; set; }

    [MaxLength(100)]
    [MinLength(6)]
    [EmailAddress]
    public required string Email { get; set; }
    public bool EmailConfirmed { get; set; }

    [JsonIgnore]
    [MaxLength(100)]
    public string PasswordHash { get; set; } = default!;

    [JsonIgnore]
    [MaxLength(60)]
    public string PasswordSalt { get; set; } = default!;

    [Phone]
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
    /// 最后密码修改时间
    /// </summary>
    public DateTimeOffset LastPwdEditTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 密码重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [MaxLength(200)]
    public string? Avatar { get; set; }
    public ICollection<SystemRole> SystemRoles { get; set; } = [];
    public ICollection<SystemLogs> SystemLogs { get; set; } = [];
    public ICollection<SystemOrganization> SystemOrganizations { get; set; } = [];

    /// <summary>
    /// 性别
    /// </summary>
    public Sex Sex { get; set; } = Sex.Male;

    public string GetUniqueKey(string prefix, string client)
    {
        return prefix + client + Id.ToString();
    }
}

/// <summary>
/// 性别
/// </summary>
public enum Sex
{
    /// <summary>
    /// 男性
    /// </summary>
    [Description("男性")]
    Male,

    /// <summary>
    /// 女性
    /// </summary>
    [Description("女性")]
    Female,

    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Else,
}
