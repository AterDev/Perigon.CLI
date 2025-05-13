using CodeGenerator;
using CodeGenerator.Helper;
using CodeGenerator.Models;
using Command.Share.Runners;
using Microsoft.Extensions.Logging;
using Share.Services;

namespace Command.Share;
/// <summary>
/// 所有命令运行的类
/// </summary>
/// <param name="codeGen"></param>
/// <param name="codeAnalysis"></param>
/// <param name="logger"></param>
public class CommandRunner(CodeGenService codeGen, CodeAnalysisService codeAnalysis, ILogger<CommandRunner> logger)
{
    private readonly CodeGenService _codeGen = codeGen;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;
    private readonly ILogger<CommandRunner> _logger = logger;

    /// <summary>
    /// 运行studio
    /// </summary>
    /// <returns></returns>
    public static async Task RunStudioAsync()
    {
        var studioCommand = new StudioRunner();
        await studioCommand.RunStudioAsync();
    }

    /// <summary>
    /// 升级studio
    /// </summary>
    public static void UpdateStudio()
    {
        var studioCommand = new StudioRunner();
        studioCommand.UpdateStudio();
    }

    /// <summary>
    /// angular 代码生成
    /// </summary>
    /// <param name="url">swagger json地址</param>
    /// <param name="output">ng前端根目录</param>
    /// <returns></returns>
    public async Task GenerateNgAsync(string url = "", string output = "")
    {
        try
        {
            _logger.LogInformation("🚀 Generating ts models and ng services...");
            RequestRunner cmd = new(url, output, RequestLibType.NgHttp);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            _logger.LogInformation(webExp.Message);
            _logger.LogInformation("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            _logger.LogInformation(exp.Message);
            _logger.LogInformation(exp.StackTrace);
        }
    }
    /// <summary>
    /// 请求服务生成
    /// </summary>
    /// <param name="url"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public async Task GenerateRequestAsync(string url = "", string output = "", RequestLibType type = RequestLibType.NgHttp)
    {
        try
        {
            _logger.LogInformation($"🚀 Generating ts models and {type} request services...");
            RequestRunner cmd = new(url, output, type);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            _logger.LogInformation(webExp.Message);
            _logger.LogInformation("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            _logger.LogInformation(exp.Message);
            _logger.LogInformation(exp.StackTrace);
        }
    }

    /// <summary>
    /// dto生成或更新
    /// </summary>
    /// <param name="entityPath"></param>
    public async Task GenerateDtoAsync(string entityPath, string outputPath, bool force)
    {
        var entityInfo = await GetEntityInfoAsync(entityPath);
        var files = await _codeGen.GenerateDtosAsync(entityInfo, outputPath, force);
        _codeGen.GenerateFiles(files);
    }

    private static async Task<EntityInfo> GetEntityInfoAsync(string entityPath)
    {
        var helper = new EntityParseHelper(entityPath);
        var entityInfo = await helper.ParseEntityAsync();
        _ = entityInfo ?? throw new Exception("实体解析失败，请检查实体文件是否正确！");
        return entityInfo;
    }

    /// <summary>
    /// manager代码生成
    /// </summary>
    /// <param name="entityPath">entity path</param>
    /// <param name="sharePath"></param>
    /// <param name="applicationPath"></param>
    /// <returns></returns>
    public async Task GenerateManagerAsync(string entityPath, string sharePath = "",
            string applicationPath = "", bool force = false)
    {
        var entityInfo = await GetEntityInfoAsync(entityPath);
        var files = new List<GenFileInfo>();

        files.AddRange(await _codeGen.GenerateDtosAsync(entityInfo, sharePath, force));
        var tplContent = TplContent.ManagerTpl();
        files.AddRange(_codeGen.GenerateManager(entityInfo, applicationPath, tplContent, force));
        _codeGen.GenerateFiles(files);
    }

    /// <summary>
    /// api项目代码生成
    /// </summary>
    /// <param name="entityPath">实体文件路径</param>
    /// <param name="applicationPath">service目录</param>
    /// <param name="apiPath">网站目录</param>
    /// <param name="suffix">控制器后缀名</param>
    public async Task GenerateApiAsync(string entityPath, string sharePath = "",
            string applicationPath = "", string apiPath = "", bool force = false)
    {
        try
        {
            var entityInfo = await GetEntityInfoAsync(entityPath);
            var files = new List<GenFileInfo>();

            files.AddRange(await _codeGen.GenerateDtosAsync(entityInfo, sharePath, force));
            var tplContent = TplContent.ManagerTpl();
            files.AddRange(_codeGen.GenerateManager(entityInfo, applicationPath, tplContent, force));

            tplContent = TplContent.ControllerTpl();
            var controllerFiles = _codeGen.GenerateController(entityInfo, apiPath, tplContent, force);
            var globalFiles = _codeGen.GenerateApiGlobalUsing(entityInfo, apiPath, true);
            files.Add(controllerFiles);
            files.Add(globalFiles);
            _codeGen.GenerateFiles(files);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "CommandRunner");
        }
    }

    /// <summary>
    /// 生成客户端api client
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="swaggerUrl"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public static async Task GenerateCSharpApiClientAsync(string swaggerUrl, string outputPath, LanguageType language = LanguageType.CSharp)
    {
        ApiClientRunner cmd = new(swaggerUrl, outputPath, language);
        await cmd.RunAsync();
    }
}


