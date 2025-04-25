using System.Text.Json.Serialization;

namespace Entity.UserMod;
/// <summary>
/// 用户日志
/// </summary>
[Index(nameof(ActionType))]
[Index(nameof(ActionUserName))]
[Index(nameof(CreatedTime))]
public class UserLog : EntityBase
{
    /// <summary>
    /// 操作人名称
    /// </summary>
    [MaxLength(100)]
    public required string ActionUserName { get; set; }

    /// <summary>
    /// 操作对象名称
    /// </summary>
    [MaxLength(100)]
    public string? TargetName { get; set; }

    [NotMapped]
    [JsonIgnore]
    public object? Data { get; set; }
    /// <summary>
    /// 操作路由
    /// </summary>
    [MaxLength(200)]
    public required string Route { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型
    /// </summary>
    public required UserActionType ActionType { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public Guid UserId { get; set; } = default!;


    public static UserLog NewLog(string userName, Guid userId, UserActionType actionType, object? entity, string? route = null, string? description = null)
    {
        return new UserLog
        {
            UserId = userId,
            ActionUserName = userName,
            Data = entity,
            Route = route ?? string.Empty,
            ActionType = actionType,
            Description = description,
        };
    }
}
