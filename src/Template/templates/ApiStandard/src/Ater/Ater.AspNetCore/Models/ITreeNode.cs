namespace Ater.AspNetCore.Models;

/// <summary>
/// 树型结构
/// </summary>
public interface ITreeNode<T>
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public List<T> Children { get; set; }
}
