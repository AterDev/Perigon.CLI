using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using Share.Helper;
using Share.Services;

namespace AterStudio.McpTools;

/// <summary>
/// 代码生成MCP工具
/// </summary>
[McpServerToolType]
public class CodeTools(
    ILogger<CodeTools> logger,
    EntityInfoManager manager,
    SolutionService solutionService,
    CodeGenService codeGenService,
    IProjectContext projectContext
)
{
    [McpServerTool, Description("create entity model class")]
    public string? NewEntity([Description("the prompt from user input")] string prompt)
    {
        var message = Prompts.CreateEntity();

        var res = $"""
            <prompt>
            {prompt}
            </prompt>

            {message}
            """;

        //logger.LogInformation(res);
        return res;
    }

    [McpServerTool, Description("generate DTO model class from entity")]
    public async Task<string?> GenerateDtoAsync(
        McpServer server,
        [Description("the entity model file absolute path")] string entityPath
    )
    {
        var prompt = Prompts.GenerateDto();
        var example = await GenerateAsync(server, entityPath, CommandType.Dto);

        return $"""
            {example}
            {prompt}
            """;
    }

    [McpServerTool, Description("generate Manager class from entity")]
    public async Task<string?> GenerateManagerAsync(
        McpServer server,
        [Description("the entity model file absolute path")] string entityPath,
        [Description("the prompt from user input")] string? prompt = ""
    )
    {
        prompt ??= "";
        var rules = Prompts.GenerateManager();
        prompt += Environment.NewLine + rules;

        var example = await GenerateAsync(server, entityPath, CommandType.Manager);
        return $"""
            {example}
            {prompt}
            """;
    }

    [McpServerTool, Description("generate Controller API from entity")]
    public async Task<string?> GenerateControllerAsync(
        McpServer server,
        [Description("the entity model file absolute path, required")] string entityPath,
        [Description("the target service absolute path, required")] string servicePath,
        [Description("the prompt from user input")] string? prompt = ""
    )
    {
        prompt ??= "";
        var rules = Prompts.GenerateController();
        prompt += Environment.NewLine + rules;

        if (servicePath.NotEmpty())
        {
            if (servicePath.EndsWith(".csproj"))
            {
                servicePath = Path.GetDirectoryName(servicePath) ?? servicePath;
            }
        }
        var example = await GenerateAsync(server, entityPath, CommandType.API, [servicePath]);
        return $"""
            {example}
            {prompt}
            """;
    }

    [McpServerTool, Description("add or create new module")]
    public async Task<string> CreateModuleAsync(
        [Description("module name,required")] string moduleName,
        McpServer server
    )
    {
        try
        {
            await SetProjectContextAsync(server);

            moduleName = moduleName.EndsWith("Mod") ? moduleName : moduleName + "Mod";
            await solutionService.CreateModuleAsync(moduleName);
            return "created success";
        }
        catch (Exception ex)
        {
            logger.LogError("Create Module: {ex}", ex);
            return ex.Message;
        }
    }


    [McpServerTool, Description("create razor tempalte from entity or openapi")]
    public async Task<string> CreateRazorTemplateAsync(McpServer server)
    {
        var rules = Prompts.GenerateRazorTemplate();

        return $"""
            {rules}
            """;

    }


    //[McpServerTool, Description("生成前端请求服务")]
    public async Task<string?> GenerateServiceAsync(
        [Description("openapi的url地址或本地路径")] string openApiPath,
        [Description("代码生成的输出路径")] string outputPath,
        [Description("前端请求类型,NgHttp或Axios")] RequestClientType clientType
    )
    {
        try
        {
            var genFiles = await codeGenService.GenerateWebRequestAsync(
                openApiPath,
                outputPath,
                clientType
            );
            if (genFiles != null)
            {
                codeGenService.GenerateFiles(genFiles);
                var resDes = new StringBuilder();
                resDes.AppendLine("生成的文件如下:");
                foreach (var file in genFiles)
                {
                    resDes.AppendLine(file.FullName);
                }
                resDes.AppendLine("");
                return resDes.ToString();
            }
            return "No validate files generated!";
        }
        catch (Exception ex)
        {
            return ex.Message + ex.StackTrace;
        }
    }


    //[McpServerTool, Description("根据指定DbContext生成数据库迁移")]

    public async Task<string?> GenerateDBMigrationAsync(
        McpServer server,
        [Description("用户指定的DbContext文件路径")] string? dbContextFilePath = null,
        [Description("迁移名称标识, 留空将自动生成")] string? migrationName = null
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dbContextFilePath))
            {
                return NotSupportClient(server, "can't get dbContextFilePath param");
            }

            migrationName ??= "AutoMigrate" + DateTime.Now.ToString("yyyyMMddHHmmss");
            await SetProjectContextAsync(server);
            string dbContextName = Path.GetFileNameWithoutExtension(dbContextFilePath);
            var result = solutionService.GenerateMigrations(dbContextName, migrationName);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GenerateDBMigrationAsync error");
            return "生成迁移失败: " + ex.Message;
        }
    }

    /// <summary>
    /// 生成服务
    /// </summary>
    /// <returns></returns>
    private async Task<string> GenerateAsync(
        McpServer server,
        string entityPath,
        CommandType type,
        string[]? servicePath = null
    )
    {
        await SetProjectContextAsync(server);
        try
        {
            var dto = new GenerateDto
            {
                EntityPath = entityPath,
                CommandType = type,
                Force = true,
                OnlyContent = true,
                ServicePath = servicePath ?? []
            };

            var res = await manager.GenerateAsync(dto);
            var resDes = new StringBuilder("<example>");
            foreach (var file in res)
            {
                resDes.AppendLine(file.ToMarkdown());
            }

            resDes.AppendLine("</example>");
            return resDes.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError("{ex}", ex);
            return "generate error:：" + ex.Message;
        }
        finally
        {
            // 生成完成后简单的垃圾回收
            OutputHelper.Info("🧹 Cleaning up after code generation...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private string NotSupportClient(McpServer server, string msg)
    {
        var result = string.Empty;
        var client = server.ClientInfo;
        if (client != null)
        {
            result = $"The {client.Name} {client.Version} may don't support this tool:{msg}";
        }
        else
        {
            result = $"The client can't support this tool:{msg}";
        }
        return result;
    }

    private async Task SetProjectContextAsync(McpServer server)
    {
        var roots = await server.RequestRootsAsync(
            new ModelContextProtocol.Protocol.ListRootsRequestParams { }
        );

        var uri = roots.Roots.FirstOrDefault()?.Uri;
        if (string.IsNullOrEmpty(uri))
        {
            logger.LogError("未找到有效的根目录路径。");
            return;
        }

        var solutionPath = new Uri(uri).LocalPath;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (solutionPath.StartsWith('/'))
            {
                solutionPath = solutionPath[1..];
            }
        }

        logger.LogInformation("SetProjectContextAsync: {solutionPath}", solutionPath);
        await projectContext.SetProjectAsync(solutionPath);
    }
}
