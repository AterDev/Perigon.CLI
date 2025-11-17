namespace Entity.CMSMod;

/// <summary>
/// 目录
/// </summary>
[Index(nameof(UserId), nameof(Name), IsUnique = true)]
public class Catalog : EntityBase, ITreeNode<Catalog>
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    public short Level { get; set; }

    /// <summary>
    /// 子目录
    /// </summary>
    public List<Catalog> Children { get; set; } = [];

    /// <summary>
    /// 父目录
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public Catalog? Parent { get; set; }
    public Guid? ParentId { get; set; }
    public ICollection<Article> Blogs { get; set; } = [];

    public Guid UserId { get; set; }
}
