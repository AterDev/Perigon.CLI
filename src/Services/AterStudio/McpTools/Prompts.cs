using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

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
            0 要遵循EF Core实体定义规范，包括关联关系的定义；
            1 命名空间必须使用文件范围命名空间;
            2 继承`EntityBase`类，它包含了Id/IsDelete/CreatedTime/UpdatedTime/TenantId等基础属性;
            3 所有属性要添加 xml 注释，注释语言根据用户输入决定；
            4 枚举类型属性要使用英文命名，且必须添加[Description]标签，内容为枚举字段名;
            5 使用[Index]特性来定义数据库索引，根据实体属性含义来定义索引；
            6 属性严格遵循可空类型的定义，必需的属性使用required关键词；
            7 所有string类型，都要添加[MaxLength]标签，请根据字段含义自行设定；
            8 如果是List类型，默认使用默认值`= []`;
            9 将枚举类型定义跟实体类型定义放在同一个文件中，枚举类型放在实体类型的后面；
            10 多对多的关系要添加中间表实体类，并定义导航属性外键，EF Core会自动创建外键，无需手动添加Index特性；
            
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
             - Dto的路径在对应模块中的Models/XXXDtos目录下
             - 请结合实体的实际CURD操作以及关联模型来设计Dto模型
            </rules>
            """
        );
    }
    public static ChatMessage GenerateManager()
    {
        return new ChatMessage(ChatRole.User,
            """
            <rules>
             - 参考以上文件路径和代码内容生成Manager类
             - Manager类需要包含基本的CURD操作方法
             - 需要考虑实体的关联实体，如它存在必需的外键关联，则在创建和更新方法中需要处理关联实体的逻辑
             - 使用`BusinessException`来抛出业务异常
            </rules>
            """
        );
    }

    public static ChatMessage GenerateController()
    {
        return new ChatMessage(ChatRole.User,
            """
            <rules>
             - 参考以上文件路径和代码内容根据用户要求生成Controller类
             - Manager和Controller类需要包含基本的CURD操作方法及用户要求的其他方法
             - 需要结合Manager类来设计Controller的方法，尤其是权限判断相关逻辑
             - 遵循Restful API来设计接口
            </rules>
            """
        );
    }
    public static ChatMessage GenerateRazorTemplate()
    {
        return new ChatMessage(ChatRole.User,
            """
            <task>
            根据要求生成.razor模板内容，模板内容用于代码生成器(基于RazorEngineCore)在运行时生成代码文件。
            你可以从<structure>中查看 引擎支持的变量和扩展方法。CustomTemplate类是定义可在上下文中使用的属性。
            如果生成与实体数据相关，则可以参考PropertyInfo及相关Dto对应的PropertyInfo列表，以匹配不同的使用场景。
            如果生成内容与OpenApi相关，则使用OpenApiPaths对象来生成相应的内容。
            `OpenApiPaths`是`Microsoft.OpenApi 2.0+`中的对象。
            Variables属性是用户自定义的变量字典.
            </task>

            <structure>
            上下文定义:

            ```csharp
            class CustomTemplate : RazorEngineTemplateBase
            {
                public Dictionary<string, string> Variables { get; set; } = [];
                public string? ModelName { get; set; }
                public string? Namespace { get; set; }
                public string? Description { get; set; }
                public string NewLine { get; set; } = Environment.NewLine;
                public List<PropertyInfo> PropertyInfos { get; set; } = [];
                public List<PropertyInfo> AddPropertyInfos { get; set; } = [];
                public List<PropertyInfo> UpdatePropertyInfos { get; set; } = [];
                public List<PropertyInfo> DetailPropertyInfos { get; set; } = [];
                // 列表页属性
                public List<PropertyInfo> ItemPropertyInfos { get; set; } = [];
                // 筛选属性
                public List<PropertyInfo> FilterPropertyInfos { get; set; } = [];
                public OpenApiPaths OpenApiPaths { get; set; } = [];
            }
            public class PropertyInfo
            {
                public required string Type { get; set; }
                public required string Name { get; set; }
                public bool IsList { get; set; }
                public bool IsPublic { get; set; } = true;
                public bool IsForeignKey { get; set; }
                // 是否为导航属性
                public bool IsNavigation { get; set; }
                public bool IsJsonIgnore { get; set; }

                // 导航属性类名称
                public string? NavigationName { get; set; }
                public bool IsComplexType { get; set; }
                public bool? HasMany { get; set; }
                public bool IsEnum { get; set; }
                public bool HasSet { get; set; } = true;

                public string? AttributeText { get; set; }

                // xml comment
                public string? CommentXml { get; set; }

                // comment summary
                public string? CommentSummary { get; set; }
                public bool IsRequired { get; set; }
                public bool IsNullable { get; set; }
                public int? MinLength { get; set; }
                public int? MaxLength { get; set; }
                public bool IsDecimal { get; set; }
                public string? SuffixContent { get; set; }
                public string DefaultValue { get; set; } = string.Empty;
                public bool IsShadow { get; set; }
                public bool IsIndex { get; set; }
            }
            ```
            支持的字符串扩展方法:
            - ToCamelCase/ToPascalCase/ToSnakeLower/ToHyphen
            </structure>

            <rules>
             - 生成的内容要严格符合Razor模板语法
             - 不要添加@inherits和@using指令，引擎会自动添加必要的引用
             - 模板文件通常在`src/templates/`目录下
             - 逻辑实现可利用C# SDK提供的功能，包括:System.Collections/System/Microsoft.OpenApi/System.Net.Http等程序集
             - 充分利用razor语法带来的csharp逻辑功能，如：变量定义，定义方法然后调用。在复杂的生成逻辑中，可利用csharp方法调用，替代直接在模板中进行复杂的拼接，以及产生的冲突问题。
            </rules>
            """
        );
    }
}
