using System.ComponentModel;
using System.Text;
using Ater.Common.Utils;
using ModelContextProtocol.Server;
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
    IProjectContext projectContext)
{
    [McpServerTool, Description("创建实体模型类")]
    public string? NewEntity([Description("用户输入的提示词内容")] string prompt)
    {
        var message = Prompts.CreateEntity();

        var res = $"""
            <prompt>
            {prompt}
            </prompt>

            {message.ToString()}
            """;

        //logger.LogInformation(res);
        return res;
    }

    [McpServerTool, Description("根据实体模型生成Dto")]
    public async Task<string?> GenerateDtoAsync(IMcpServer server, [Description("实体模型文件的绝对路径")] string entityPath)
    {
        return await GenerateAsync(server, entityPath, CommandType.Dto);
    }

    [McpServerTool, Description("根据实体模型生成Manager")]
    public async Task<string?> GenerateManagerAsync([Description("实体模型文件的绝对路径")] string entityPath, IMcpServer server)
    {
        return await GenerateAsync(server, entityPath, CommandType.Manager);
    }

    [McpServerTool, Description("根据实体模型生成Controller/API接口")]
    public async Task<string?> GenerateControllerAsync([Description("实体模型文件的绝对路径")] string entityPath, IMcpServer server)
    {
        return await GenerateAsync(server, entityPath, CommandType.API);
    }

    [McpServerTool, Description("创建或添加新的模块")]
    public async Task<string> CreateModuleAsync([Description("模块名称")] string moduleName, IMcpServer server)
    {
        try
        {
            await SetProjectContextAsync(server);

            moduleName = moduleName.EndsWith("Mod") ? moduleName : moduleName + "Mod";
            await solutionService.CreateModuleAsync(moduleName);
            return "创建成功";
        }
        catch (Exception ex)
        {
            logger.LogError("Create Module: {ex}", ex);
            return ex.Message;
        }
    }

    [McpServerTool, Description("生成前端请求服务")]
    public async Task<string?> GenerateServiceAsync(
        [Description("openapi的url地址或本地路径")] string openApiPath,
        [Description("代码生成的输出路径")] string outputPath,
        [Description("前端请求类型,NgHttp或Axios")] RequestLibType clientType)
    {

        try
        {
            var genFiles = await codeGenService.GenerateWebRequestAsync(openApiPath, outputPath, clientType);

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
        catch (Exception ex)
        {
            return ex.Message + ex.StackTrace;
        }
    }

    /// <summary>
    /// 生成服务
    /// </summary>
    /// <returns></returns>
    private async Task<string> GenerateAsync(IMcpServer server, string entityPath, CommandType type)
    {
        await SetProjectContextAsync(server);
        try
        {
            var dto = new GenerateDto
            {
                EntityPath = entityPath,
                CommandType = type,
                Force = true,
            };

            var res = await manager.GenerateAsync(dto);
            var resDes = new StringBuilder("<result>");
            resDes.AppendLine("受影响的文件路径:");
            foreach (var file in res)
            {
                resDes.AppendLine(file.FullName);
            }
            resDes.AppendLine("在对话中列出以上文件，标记出新增或修改的文件");
            resDes.AppendLine("</result>");

            if (manager.ModuleName.NotEmpty())
            {
                resDes.AppendLine("生成后的操作：");
                resDes.AppendLine("<prompt>");
                resDes.AppendLine($"检查src/Modules目录下是否包含{manager.ModuleName}/{manager.ModuleName}.csproj 程序集，如果不包含 则调用 添加新模块工具");
                resDes.AppendLine("</prompt>");
            }
            return resDes.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError("{ex}", ex);
            return "工具出错：" + ex.Message;
        }
    }

    private async Task SetProjectContextAsync(IMcpServer server)
    {
        var roots = await server.RequestRootsAsync(new ModelContextProtocol.Protocol.ListRootsRequestParams
        {
            Meta = new ModelContextProtocol.Protocol.RequestParamsMetadata
            {
                ProgressToken = new ModelContextProtocol.Protocol.ProgressToken("CodeTools")
            }
        });

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
