namespace CodeGenerator.Generate.LanguageFormatter;

/// <summary>
/// 语言类型名称格式化接口: 输入标准(C#)类型字符串与属性标记, 输出目标语言类型表达。
/// 标准类型指 OpenApiHelper / 解析阶段确定下来的 C# 基础 / 泛型 / 集合 / 字典 结构。
/// </summary>
public interface ILanguageFormatter
{
    /// <summary>
    /// 类型名称格式化 (原 Format)。
    /// </summary>
    string FormatType(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false);

    /// <summary>
    /// 根据类型元数据生成目标语言模型(包含 import / 结构体 / 接口 等)。
    /// </summary>
    /// <param name="meta">类型元数据</param>
    /// <returns>生成后的代码文本</returns>
    string GenerateModel(TypeMeta meta, string nsp = "");
}
