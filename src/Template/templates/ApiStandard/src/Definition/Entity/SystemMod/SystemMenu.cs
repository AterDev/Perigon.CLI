namespace Entity.SystemMod;

/// <summary>
/// 系统菜单
/// </summary>
[Index(nameof(AccessCode), IsUnique = true)]
public class SystemMenu : EntityBase, ITreeNode<SystemMenu>
{
    /// <summary>
    /// 菜单名称
    /// </summary>
    [MaxLength(60)]
    public required string Name { get; set; }

    /// <summary>
    /// 菜单路径
    /// </summary>
    [MaxLength(100)]
    public string? Path { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [MaxLength(30)]
    public string? Icon { get; set; }

    /// <summary>
    /// 父菜单
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public SystemMenu? Parent { get; set; }
    public Guid? ParentId { get; set; }

    /// <summary>
    ///  是否有效
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// 子菜单
    /// </summary>
    public List<SystemMenu> Children { get; set; } = [];

    /// <summary>
    /// 所属角色
    /// </summary>
    public ICollection<SystemRole> Roles { get; set; } = [];

    /// <summary>
    /// 权限编码
    /// </summary>
    [MaxLength(50)]
    public required string AccessCode { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public MenuType MenuType { get; set; } = MenuType.Page;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool Hidden { get; set; } = true;
}

public enum MenuType
{
    /// <summary>
    /// 页面
    /// </summary>
    [Description("页面")]
    Page,

    /// <summary>
    /// 按钮
    /// </summary>
    [Description("按钮")]
    Button,
}
