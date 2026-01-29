using CodeGenerator;
using ModelContextProtocol.Server;
using Share.Helper;
using Share.Services;
using StudioMod.Models.GenActionDtos;
using System.ComponentModel;
using System.Text;

namespace AterStudio.McpTools;

/// <summary>
/// 代码生成MCP工具
/// </summary>
[McpServerToolType]
public class CodeTools(
    ILogger<CodeTools> logger,
    EntityInfoManager manager,
    SolutionService solutionService,
    GenActionManager genAction,
    DefaultDbContext dbContext,
    IProjectContext projectContext,
    CommandService commandService
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


    [McpServerTool, Description("verify razor tempalte using entity")]
    public async Task<string> VerifyRazorTemplateAsync(McpServer server,
        [Description("the entity model file absolute path")] string entityPath,
        [Description("the razor template content")] string razorTemplate)
    {

        await SetProjectContextAsync(server);
        var genContext = new RazorGenContext();
        try
        {
            var entityInfo = (
                   await CodeAnalysisService.GetEntityInfosAsync([entityPath])
               ).FirstOrDefault();
            if (entityInfo == null)
            {
                return "can't find entity info from path:" + entityPath;
            }
            var actionRunModel = new ActionRunModel
            {
                ModelName = entityInfo.Name,
                Namespace = entityInfo.NamespaceName,
                PropertyInfos = entityInfo.PropertyInfos,
                Description = entityInfo.Summary
            };
            // 添加变量
            actionRunModel.Variables.Add(
                new Variable { Key = "ModelName", Value = entityInfo.Name }
            );
            actionRunModel.Variables.Add(
                new Variable { Key = "ModelNameHyphen", Value = entityInfo.Name.ToHyphen() }
            );
            // 解析dto
            var dtoPath = projectContext.GetDtoPath(
                entityInfo.Name,
                entityInfo.ModuleName
            );
            if (Directory.Exists(dtoPath))
            {
                var matchFiles = new string[]
                {
                            "AddDto.cs",
                            "UpdateDto.cs",
                            "DetailDto.cs",
                            "ItemDto.cs",
                            "FilterDto.cs",
                };

                var dtoFiles = Directory
                    .GetFiles(dtoPath, "*Dto.cs", SearchOption.AllDirectories)
                    .Where(q => matchFiles.Any(m => Path.GetFileName(q).EndsWith(m)))
                    .ToList();

                var dtoInfos = await CodeAnalysisService.GetEntityInfosAsync(dtoFiles);

                actionRunModel.AddPropertyInfos =
                    dtoInfos.FirstOrDefault(q => q.Name.EndsWith("AddDto"))?.PropertyInfos
                    ?? [];

                actionRunModel.UpdatePropertyInfos =
                    dtoInfos
                        .FirstOrDefault(q => q.Name.EndsWith("UpdateDto"))
                        ?.PropertyInfos ?? [];

                actionRunModel.DetailPropertyInfos =
                    dtoInfos
                        .FirstOrDefault(q => q.Name.EndsWith("DetailDto"))
                        ?.PropertyInfos ?? [];

                actionRunModel.ItemPropertyInfos =
                    dtoInfos.FirstOrDefault(q => q.Name.EndsWith("ItemDto"))?.PropertyInfos
                    ?? [];

                actionRunModel.FilterPropertyInfos =
                    dtoInfos
                        .FirstOrDefault(q => q.Name.EndsWith("FilterDto"))
                        ?.PropertyInfos ?? [];
            }
            string resContent = genContext.GenTemplate(razorTemplate, actionRunModel);
            return resContent;
        }
        catch (Exception ex)
        {
            return ex.Message + ex.StackTrace;
        }
    }


    [McpServerTool, Description("execute generate task")]
    public async Task<string> ExecuteGenerateTaskAsync(McpServer server,
        [Description("the generate task id")] int? taskId,
        [Description("the entity model file absolute path")] string entityPath
        )
    {
        if (taskId == null)
        {
            var actions = dbContext.GenActions.Select(s => new
            {
                s.Id,
                s.Name,
                s.Description
            }).ToList();
            var actionJson = JsonSerializer.Serialize(actions);
            return "需要提供任务id，请根据用户描述选择对应的任务，选择对应的任务id后重试:" + actionJson;
        }
        await SetProjectContextAsync(server);
        try
        {
            var dto = new GenActionRunDto
            {
                Id = taskId.Value,
                SourceFilePath = entityPath,
                OnlyOutput = false
            };
            var res = await genAction.ExecuteActionAsync(dto);
            return res.ErrorMsg ?? "generate success";
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

    [McpServerTool, Description("generate request client from api document")]
    public async Task<string> GenerateRequestClientAsync(
        McpServer server,
        [Description("request client type: NgHttp, Axios, or CSharp")] string clientType,
        [Description("the output directory path")] string outputPath,
        [Description("only generate models, not client code")] bool onlyModels = false,
        [Description("the api document id")] int? apiDocId = null
    )
    {
        try
        {
            await SetProjectContextAsync(server);

            if (projectContext.SolutionId == null || projectContext.SolutionId <= 0)
            {
                return "❌ Can't find solution, please add solution first!";
            }
            var currentProjectId = projectContext.SolutionId;

            if (apiDocId == null)
            {
                var apiDocs = dbContext.ApiDocInfos
                    .Where(x => x.ProjectId == currentProjectId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Description,
                        x.Path
                    })
                    .ToList();

                if (apiDocs.Count == 0)
                {
                    return "No API documents found in current project. Please create or import an API document first.";
                }

                var docJson = JsonSerializer.Serialize(apiDocs);
                return $"Please let User select an API document by providing its ID:\n{docJson}\n\nCall this method again with the apiDocId parameter.";
            }

            var apiDoc = dbContext.ApiDocInfos
                .FirstOrDefault(x => x.Id == apiDocId && x.ProjectId == currentProjectId);

            if (apiDoc == null)
            {
                return $"API document with ID {apiDocId} not found in current project.";
            }

            if (!Enum.TryParse<RequestClientType>(clientType, true, out var requestClientType))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(RequestClientType)));
                return $"Invalid client type. Valid types are: {validTypes}";
            }

            OutputHelper.Info($"Generating {clientType} client from API: {apiDoc.Name}");

            await commandService.GenerateRequestClientAsync(
                apiDoc.Path,
                outputPath,
                requestClientType,
                onlyModels
            );

            return $"Successfully generated {clientType} request client from '{apiDoc.Name}' to '{outputPath}'";
        }
        catch (Exception ex)
        {
            logger.LogError("{ex}", ex);
            return $"Generate client error: {ex.Message}";
        }
        finally
        {
            OutputHelper.Info("🧹 Cleaning up after code generation...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
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
