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
    public required string Key { get; set; }
    /// <summary>
    /// 值 
    /// </summary>
    [MaxLength(2000)]
    public required string Value { get; set; }

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
