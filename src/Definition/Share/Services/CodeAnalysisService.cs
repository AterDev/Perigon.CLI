using System.Collections.Concurrent;
using CodeGenerator.Helper;
using Entity;

namespace Share.Services;

/// <summary>
/// 代码解析服务
/// </summary>
public class CodeAnalysisService(ILogger<CodeAnalysisService> logger)
{
    private readonly ILogger<CodeAnalysisService> _logger = logger;

    /// <summary>
    /// get entity file info
    /// </summary>
    /// <param name="entityPath"></param>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    public static List<EntityFile> GetEntityFiles(string entityPath, List<string> filePaths)
    {
        var entityFiles = new ConcurrentBag<EntityFile>();
        Parallel.ForEach(
            filePaths,
            path =>
            {
                string content = File.ReadAllText(path);
                var compilation = new CompilationHelper(entityPath);
                compilation.LoadContent(content);
                if (compilation.IsEntityClass())
                {
                    var comment = RegexSource
                        .SummaryCommentRegex()
                        .Match(content)
                        ?.Groups[1]
                        ?.Value.Trim();
                    comment = comment?.Replace("/", "").Trim();

                    var entityFile = new EntityFile
                    {
                        Name = Path.GetFileName(path),
                        FullName = path,
                        Content = content,
                        Comment = comment,
                        ModuleName = EntityParseHelper.GetEntityModuleName(path),
                    };

                    entityFiles.Add(entityFile);
                }
            }
        );
        return [.. entityFiles];
    }

    /// <summary>
    /// 分析实体信息
    /// </summary>
    /// <param name="entityFiles"></param>
    /// <returns></returns>
    public static async Task<List<EntityInfo>> GetEntityInfosAsync(List<string> entityFiles)
    {
        var entityInfos = new ConcurrentBag<EntityInfo>();
        await Parallel.ForEachAsync(
            entityFiles,
            async (entityFile, token) =>
            {
                var parse = new EntityParseHelper(entityFile);
                var entityInfo = await parse.ParseEntityAsync();
                if (entityInfo != null)
                {
                    entityInfos.Add(entityInfo);
                }
            }
        );
        return [.. entityInfos];
    }

    public static EntityFile? GetEntityFile(string entityPath, string filePath)
    {
        return GetEntityFiles(entityPath, [filePath]).FirstOrDefault();
    }

    /// <summary>
    /// get entity files path
    /// </summary>
    /// <param name="entityAssemblyPath"></param>
    /// <returns></returns>
    public static List<string> GetEntityFilePaths(string entityAssemblyPath)
    {
        return Directory
            .GetFiles(entityAssemblyPath, "*.cs", SearchOption.AllDirectories)
            .Where(f =>
                !(
                    f.EndsWith(".g.cs")
                    || f.EndsWith(".AssemblyAttributes.cs")
                    || f.EndsWith(".AssemblyInfo.cs")
                    || f.EndsWith(ConstVal.GlobalUsingsFile)
                    || f.EndsWith("EntityBase.cs")
                )
            )
            .ToList();
    }
}
