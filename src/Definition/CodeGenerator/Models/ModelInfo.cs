using System.ComponentModel.DataAnnotations;
using Entity;
using Entity.StudioMod;

namespace CodeGenerator.Models;

public class EntityInfo
{
    public static string[] IgnoreTypes { get; } = ["JsonDocument?", "byte[]"];
    public static string[] IgnoreProperties { get; } =
        [
            ConstVal.Id,
            ConstVal.CreatedTime,
            ConstVal.UpdatedTime,
            ConstVal.IsDeleted,
            "PageSize",
            "PageIndex",
        ];

    /// <summary>
    /// module name
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// the db context name
    /// </summary>
    public string? DbContextName { get; set; }

    /// <summary>
    /// file path
    /// </summary>
    [MaxLength(200)]
    public required string FilePath { get; set; }

    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    [MaxLength(100)]
    public required string NamespaceName { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    [MaxLength(100)]
    public string? AssemblyName { get; set; }

    /// <summary>
    /// 类注释
    /// </summary>
    [MaxLength(300)]
    public string? Comment { get; set; }

    /// <summary>
    /// 类注释
    /// </summary>
    [MaxLength(100)]
    public string? Summary { get; set; }
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;

    /// <summary>
    /// 是否为枚举类
    /// </summary>
    public bool? IsEnum { get; set; } = false;
    public bool IsList { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    public List<PropertyInfo> PropertyInfos { get; set; } = [];

    public string GetDtoNamespace()
    {
        return GetShareNamespace();
    }

    public string GetShareNamespace()
    {
        return ModuleName.IsEmpty() ? ConstVal.ShareName : ModuleName;
    }

    public string GetManagerNamespace()
    {
        return ModuleName.IsEmpty() ? ConstVal.CommonMod : ModuleName;
    }

    public string GetAPINamespace()
    {
        return ModuleName.IsEmpty() ? ConstVal.APIName : ModuleName;
    }

    /// <summary>
    /// 获取导航属性
    /// </summary>
    /// <returns></returns>
    public List<PropertyInfo>? GetRequiredNavigation()
    {
        return PropertyInfos
            ?.Where(p => p.IsNavigation && p.HasMany == false && p.IsRequired)
            .ToList();
    }

    /// <summary>
    /// 获取筛选属性
    /// </summary>
    /// <returns></returns>
    public List<PropertyInfo> GetFilterProperties()
    {
        return PropertyInfos
                .Where(p =>
                    p.IsRequired && !p.IsNavigation
                    || !p.IsList
                        && !p.IsNavigation
                        && !p.IsComplexType
                        && !IgnoreProperties.Contains(p.Name)
                        && !IgnoreTypes.Contains(p.Type)
                    || p.IsEnum
                )
                .Where(p => p.MaxLength is not (not null and >= 100))
                .ToList() ?? [];
    }
}
