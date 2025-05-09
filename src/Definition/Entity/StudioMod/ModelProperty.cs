using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.StudioMod;

/// <summary>
/// 属性
/// </summary>
[Index(nameof(Name))]
[Index(nameof(Type))]
[Index(nameof(IsEnum))]
public class ModelProperty : EntityBase
{
    /// <summary>
    /// 类型
    /// </summary>
    [MaxLength(100)]
    public required string Type { get; set; }
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    [MaxLength(100)]
    public string? DisplayName { get; set; }
    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsList { get; set; }
    public bool IsPublic { get; set; } = true;
    /// <summary>
    /// 是否为导航属性
    /// </summary>
    public bool IsNavigation { get; set; }
    public bool IsJsonIgnore { get; set; }
    /// <summary>
    /// 导航属性类名称
    /// </summary>
    [MaxLength(100)]
    public string? NavigationName { get; set; }
    public bool IsComplexType { get; set; }
    /// <summary>
    /// 导航属性的对应关系
    /// </summary>
    public bool? HasMany { get; set; }
    public bool IsEnum { get; set; }
    /// <summary>
    /// 是否包括set方法
    /// </summary>
    public bool HasSet { get; set; } = true;
    [MaxLength(100)]
    public string? AttributeText { get; set; }
    /// <summary>
    /// xml comment
    /// </summary>
    [MaxLength(500)]
    public string? CommentXml { get; set; }
    /// <summary>
    /// comment summary
    /// </summary>
    [MaxLength(200)]
    public string? CommentSummary { get; set; }
    /// <summary>
    /// 是否必须
    /// </summary>
    public bool IsRequired { get; set; }
    /// <summary>
    /// 可空?
    /// </summary>
    public bool IsNullable { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsDecimal { get; set; }
    /// <summary>
    /// 尾缀，如#endregion
    /// </summary>
    [MaxLength(100)]
    public string? SuffixContent { get; set; }
    /// <summary>
    /// 默认值
    /// </summary>
    [MaxLength(100)]
    public string DefaultValue { get; set; } = string.Empty;

    [ForeignKey(nameof(ModelInfoId))]
    public ModelInfo EntityInfo { get; set; } = null!;
    public Guid ModelInfoId { get; set; } = default!;

    /// <summary>
    /// 转换成C#属性
    /// </summary>
    /// <param name="isInput">是否作为输入属性</param>
    /// <returns></returns>
    public string ToCsharpLine(bool isInput = false)
    {
        string? attributeText = AttributeText;
        // 默认值
        string defaultValue = DefaultValue;

        if (!string.IsNullOrEmpty(attributeText))
        {
            attributeText = attributeText.Trim();
            attributeText = $@"    {attributeText.Replace(Environment.NewLine, Environment.NewLine + "    ")}"
                + Environment.NewLine;
        }
        string nullableMark = IsNullable ? "?" : "";
        string requiredKeyword = IsRequired ? "required " : "";

        // 非输入的ViewModel
        if (!isInput)
        {
            requiredKeyword = "";
            if (IsRequired) { defaultValue = "default!"; }
        }
        // 可空移除默认值
        if (IsNullable) { defaultValue = string.Empty; }

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            defaultValue = $" = {defaultValue};";
        }
        string content = @$"    public {requiredKeyword}{Type}{nullableMark} {Name} {{ get; set; }}{defaultValue}";
        if (!isInput && Name.ToLower().Contains("password"))
        {
            attributeText = attributeText?.Replace("    ", "    // ");
            content = @$"    // public {Type}{nullableMark} {Name} {{ get; set; }}{defaultValue}";
        }
        return $@"{CommentXml}{attributeText}{content}{SuffixContent}
";
    }

    public static (List<ModelProperty> add, List<ModelProperty> delete) GetDiffProperties(List<ModelProperty> origin, List<ModelProperty> compare)
    {
        List<ModelProperty> add = [];
        List<ModelProperty> delete = [];
        foreach (var item in compare)
        {
            if (!origin.Any(p => p.Name.Equals(item.Name)
                && p.IsList == item.IsList
                && p.IsEnum == item.IsEnum
                && p.Type == item.Type))
            {
                add.Add(item);
            }
        }
        foreach (var item in origin)
        {
            if (!compare.Any(p => p.Name.Equals(item.Name)
                && p.IsList == item.IsList
                && p.IsEnum == item.IsEnum
                && p.Type == item.Type))
            {
                delete.Add(item);
            }
        }
        return (add, delete);
    }
}