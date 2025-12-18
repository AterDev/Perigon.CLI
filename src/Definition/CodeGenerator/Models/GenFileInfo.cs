namespace CodeGenerator.Models;

/// <summary>
/// 生成文件的信息，主要用来存储多个文件生成的内容
/// </summary>
public class GenFileInfo(string name, string content)
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; } = name;

    public string DirName { get; init; } = string.Empty;

    public string Content { get; set; } = content;

    /// <summary>
    /// 模块名称
    /// </summary>
    public string? ModuleName { get; set; }
    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 是否覆盖
    /// </summary>
    public bool IsCover { get; set; }

    public GenFileType FileType { get; set; } = GenFileType.Default;

    /// <summary>
    /// file path
    /// </summary>
    public string FullName { get; set; } = string.Empty;


    public string ToMarkdown()
    {
        return $"""

            ## {Name}

            - Path: {FullName}
            - Content: 
            ```csharp
            {Content}
            ```            
            ---
            """;
    }
}

public enum GenFileType
{
    Default,
    Global,
}