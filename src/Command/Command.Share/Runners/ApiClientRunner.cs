using CodeGenerator.Models;
using Share.Helper;

namespace Command.Share.Runners;
/// <summary>
/// 客户端请求生成
/// </summary>
public class ApiClientRunner : RunnerBase
{
    /// <summary>
    /// swagger文档链接
    /// </summary>
    public string DocUrl { get; set; }
    /// <summary>
    /// 文档名称 swagger/{documentName}/swagger.json
    /// </summary>
    public string DocName { get; set; }

    public OpenApiDocument? ApiDocument { get; set; }

    public LanguageType LanguageType { get; set; } = LanguageType.CSharp;

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputPath { get; set; }

    public ApiClientRunner(string docUrl, string output, LanguageType languageType)
    {
        DocUrl = docUrl;
        DocName = docUrl.Split('/').Reverse().Skip(1).First();

        OutputPath = Path.Combine(output, DocName.ToPascalCase() + "API");
        LanguageType = languageType;

        Instructions.Add($"  🔹 Generate CSharp Rest Request Service.");
    }
    public async Task RunAsync()
    {
        string openApiContent = "";
        if (DocUrl.StartsWith("http://") || DocUrl.StartsWith("https://"))
        {
            using HttpClient http = new();
            openApiContent = await http.GetStringAsync(DocUrl);
        }
        else
        {
            openApiContent = File.ReadAllText(DocUrl);
        }
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        ApiDocument = new OpenApiStringReader().Read(openApiContent, out _);


        Console.WriteLine(Instructions[0]);
        await GenerateRequestServicesAsync();
        await GenerateCommonFilesAsync();
        Console.WriteLine("😀 Api Client generate completed!" + Environment.NewLine);
    }

    public async Task GenerateCommonFilesAsync()
    {
        string nspName = new DirectoryInfo(OutputPath).Name;
        string baseContent = CSHttpClientGenerate.GetBaseService(nspName);
        string globalUsingContent = CSHttpClientGenerate.GetGlobalUsing(DocName.ToPascalCase() + "API");

        string dir = Path.Combine(OutputPath, "Services");
        await GenerateFileAsync(dir, "BaseService.cs", baseContent, true);

        await GenerateFileAsync(OutputPath, "GlobalUsings.cs", globalUsingContent, false);
    }

    public async Task GenerateRequestServicesAsync()
    {
        CSHttpClientGenerate gen = new(ApiDocument!);
        // 获取请求服务并生成文件
        string nspName = new DirectoryInfo(OutputPath).Name;
        List<GenFileInfo> services = gen.GetServices(nspName);

        string dir = Path.Combine(OutputPath, "Services");
        IOHelper.DeleteDirectory(dir);

        foreach (GenFileInfo service in services)
        {
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
        var serviceNames = services.Select(s => s.Name.TrimEnd(".cs".ToCharArray())).ToList();
        string extensionContent = CSHttpClientGenerate.GetExtensionContent(nspName, serviceNames);
        await GenerateFileAsync(OutputPath, "Extension.cs", extensionContent, false);

        dir = Path.Combine(OutputPath, "Models");
        IOHelper.DeleteDirectory(dir);
        List<GenFileInfo> models = gen.GetModelFiles(nspName);
        foreach (GenFileInfo model in models)
        {
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }

        string csProjectContent = CSHttpClientGenerate.GetCsprojContent();
        await GenerateFileAsync(OutputPath, $"{DocName.ToPascalCase()}API.csproj", csProjectContent, true);
    }
}

public enum LanguageType
{
    CSharp
}