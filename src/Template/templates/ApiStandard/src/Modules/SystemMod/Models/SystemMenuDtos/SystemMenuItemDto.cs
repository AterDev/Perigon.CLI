namespace SystemMod.Models.SystemMenuDtos;

/// <summary>
/// 系统菜单列表元素
/// </summary>
/// <inheritdoc cref="SystemMenu"/>
public class SystemMenuItemDto : ITreeNode<SystemMenuItemDto>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [MaxLength(60)]
    public string Name { get; set; } = default!;

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
    public Guid? ParentId { get; set; }

    /// <summary>
    ///  是否有效
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// 权限编码
    /// </summary>
    [MaxLength(50)]
    public string AccessCode { get; set; } = default!;

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
    public List<SystemMenuItemDto> Children { get; set; } = [];
}
