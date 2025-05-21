using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

[McpServerPromptType]
public static class Prompts
{
    [McpServerPrompt, Description("create a prompt for create entity class")]
    public static ChatMessage CreateEntity()
    {
        return new ChatMessage(ChatRole.User,
            """
            <rules>
            实体模型需要创建在`src/Definition`目录下的`Entity`项目中；

            如果指定了模块名称，则需要在`Entity`程序集下创建一个新的文件夹，命名为模块名称+Mod，如`UserMod`，如果没有模块名称，则不创建文件夹；

            创建一个C#实体模型类，要遵循以下规范:
            1 命名空间必须使用文件范围命名空间;
            2 继承自`EntityBase`类;
            3 所属属性要添加 xml 注释，注释优先使用中文描述
            4 枚举类型属性要使用英文命名，且必须添加[Description]标签，也使用英文;
            5 如果有明确的模块名称，则为类添加如[Module(Modules.xxx)]标签，其中Modules.xxx是模块的静态定义，在Entity程序集的Modules.cs中定义，如果该文件中没有常量定义，则添加该常量；
            6 属性严格遵循可空类型的定义，必需的属性使用required关键词；
            7 所有string类型，都要添加[MaxLength]标签，如果没有指定长度，则默认为200；
            8 如果是List类型，默认使用默认值`= []`;

            </rules>
            """);
    }
}
