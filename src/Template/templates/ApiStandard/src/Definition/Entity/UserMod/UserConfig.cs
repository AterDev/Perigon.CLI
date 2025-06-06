namespace Entity.UserMod;
/// <summary>
/// 用户配置
/// </summary>
[Index(nameof(Key))]
[Index(nameof(Valid))]
[Index(nameof(GroupName))]
[Module(Modules.User)]
public class UserConfig : EntityBase
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
    /// 组
    /// </summary>
    [MaxLength(60)]
    public string GroupName { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    public Guid UserId { get; set; } = default!;

}
