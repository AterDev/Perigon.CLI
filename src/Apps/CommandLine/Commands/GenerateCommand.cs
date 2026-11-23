using CoreMod.Services;
using Share.Helper;
using Share.Services;
using Spectre.Console;

namespace CommandLine.Commands;

/// <summary>
/// Settings for the entity-generation guidance command.
/// </summary>
public sealed class GenerateEntitySettings;

public sealed class GenerateEntityCommand
{
    public int Execute(GenerateEntitySettings settings)
    {
        AnsiConsole.WriteLine(
            """
            <rules>
            实体模型需要创建在`src/Definition`目录下的`Entity`项目中；

            如果指定了模块名称，则需要在`Entity`程序集下创建模块目录(如果没有)，命名为模块名称+Mod，如`UserMod`；

            如果模块不存在(`src/Modules`下没有对应模块)，则先创建模块，再创建实体模型；

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
            11 DbContext中的DbSet属性使用标准{get;set;}定义;如无明确说明，不要使用ToTable等额外配置。
            </rules>
            """
        );
        return 0;
    }
}

/// <summary>
/// 生成请求
/// </summary>
public class RequestCommand(CommandService commandService)
{
    public async Task<int> ExecuteAsync(RequestSettings settings, CancellationToken cancellationToken)
    {
        if (Enum.TryParse<RequestType>(settings.Type, true, out RequestType type))
        {
            var clientType = type switch
            {
                RequestType.CSharp => RequestClientType.CSharp,
                RequestType.Angular => RequestClientType.NgHttp,
                RequestType.Axios => RequestClientType.Axios,
                _ => RequestClientType.NgHttp
            };

            OutputHelper.Important($"Generate {type} request client from {settings.Path} to {settings.OutputPath}");

            await commandService.GenerateRequestClientAsync(
                settings.Path,
                settings.OutputPath,
                clientType,
                settings.OnlyModel,
                settings.CoverBaseService
            );
            return 0;
        }
        else
        {
            OutputHelper.Error("Invalid type, only support: csharp, angular");
            return -1;
        }
    }
}

public sealed class RequestSettings
{
    public string Path { get; set; } = string.Empty;

    public required string OutputPath { get; set; }

    public string Type { get; set; } = "angular";

    public bool OnlyModel { get; set; }

    public bool CoverBaseService { get; set; }
}

public enum RequestType
{
    Angular,
    Axios,
    CSharp,
}
