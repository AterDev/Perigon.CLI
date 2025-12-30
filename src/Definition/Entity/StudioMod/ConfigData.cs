namespace Entity.StudioMod;
/// <summary>
/// 配置
/// </summary>
public class ConfigData : EntityBase
{
    /// <summary>
    /// 键
    /// </summary>
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    /// <summary>
    /// 值 
    /// </summary>
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    public ValueType ValueType { get; set; } = ValueType.String;
}

public enum ValueType
{
    Integer,
    Double,
    String,
    Boolean,
    /// <summary>
    /// 加密字符串
    /// </summary>
    Encrypt
}
