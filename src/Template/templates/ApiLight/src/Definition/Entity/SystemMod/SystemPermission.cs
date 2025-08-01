namespace Definition.Entity.SystemMod;

/// <summary>
/// 权限
/// </summary>
[Index(nameof(Name))]
[Index(nameof(PermissionType))]
[Module(Modules.System)]
public class SystemPermission : EntityBase
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(60)]
    public required string Name { get; set; }

    /// <summary>
    /// 权限说明
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// 权限类型
    /// </summary>
    public PermissionType PermissionType { get; set; }

    /// <summary>
    /// 权限组
    /// </summary>
    [ForeignKey(nameof(GroupId))]
    public SystemPermissionGroup Group { get; set; } = null!;

    public Guid GroupId { get; set; } = default!;
}

/// <summary>
/// 权限类型
/// </summary>
[Flags]
public enum PermissionType
{
    [Description("无权限")]
    None = 0,

    [Description("可读")]
    Read = 1,

    /// <summary>
    /// 审核
    /// </summary>
    [Description("可审核")]
    Audit = 2,

    /// <summary>
    /// 仅添加
    /// </summary>
    [Description("仅添加")]
    Add = 4,

    /// <summary>
    /// 仅编辑
    /// </summary>
    [Description("仅编辑")]
    Edit = 16,

    /// <summary>
    /// 可读写
    /// </summary>
    [Description("可读写")]
    Write = Read | Add | Edit,

    /// <summary>
    /// 读写且可审核
    /// </summary>
    [Description("读写且可审核")]
    AuditWrite = Write | Audit,
}
