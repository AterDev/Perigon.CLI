using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

[McpServerPromptType]
public static class Prompts
{
    [McpServerPrompt, Description("The prompt for create entity class")]
    public static ChatMessage CreateEntity()
    {
        return new ChatMessage(
            ChatRole.User,
            """
            <rules>
            实体模型需要创建在`src/Definition`目录下的`Entity`项目中；

            如果指定了模块名称，则需要在`Entity`程序集下创建一个新的文件夹，命名为模块名称+Mod，如`UserMod`，如果没有模块名称，则不创建文件夹；

            如果模块不存在(`src/Modules`下没有对应模块)，则调用创建模块的工具先创建模块，再创建实体模型；

            创建一个C#实体模型类，要遵循以下规范:
            1 命名空间必须使用文件范围命名空间;
            2 继承`EntityBase`类，它包含了Id/IsDelete/CreatedTime/UpdatedTime/TenantId等基础属性;
            3 所有属性要添加 xml 注释，注释语言根据用户输入决定；
            4 枚举类型属性要使用英文命名，且必须添加[Description]标签，内容为枚举字段名;
            5 使用[Index]特性来定义数据库索引，根据实体属性含义来定义索引；
            6 属性严格遵循可空类型的定义，必需的属性使用required关键词；
            7 所有string类型，都要添加[MaxLength]标签，请根据字段含义自行设定；
            8 如果是List类型，默认使用默认值`= []`;
            9 将枚举类型定义跟实体类型定义放在同一个文件中，枚举类型放在实体类型的后面；
            
            </rules>
            """
        );
    }

    public static ChatMessage GenerateDto()
    {
        return new ChatMessage(
            ChatRole.User,
            """
            <rules>
             - 参考以上文件路径和代码内容生成Dto模型类
             - Dto的路径一般是在对应模块中的Models/XXXDtos目录下
             - 请结合实体的实际CURD操作以及关联模型来设计Dto模型
            </rules>
            """
        );
    }
}
