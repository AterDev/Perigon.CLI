using System.Text.Json;
using System.Xml.Linq;
using Entity;
using Entity.StudioMod;

namespace CodeGenerator.Helper;

/// <summary>
/// 项目帮助类
/// </summary>
public class AssemblyHelper
{
    /// <summary>
    /// 搜索项目文件.csproj,直到根目录
    /// </summary>
    /// <param name="dir">起始目录</param>
    /// <param name="root">根目录</param>
    /// <returns></returns>
    public static FileInfo? FindProjectFile(DirectoryInfo dir, DirectoryInfo? root = null)
    {
        try
        {
            FileInfo? file = dir.GetFiles($"*{ConstVal.CSharpProjectExtension}")?.FirstOrDefault();
            return root == null ? file : file == null && dir != root ? FindProjectFile(dir.Parent!, root) : file;
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("❌ can't find dir:" + dir.FullName);
            return null;
        }
    }

    /// <summary>
    /// 在项目中寻找文件
    /// </summary>
    /// <param name="projectFilePath"></param>
    /// <param name="searchFileName"></param>
    /// <returns>the search file path,return null if not found </returns>
    public static string? FindFileInProject(string projectFilePath, string searchFileName)
    {
        DirectoryInfo dir = new(Path.GetDirectoryName(projectFilePath)!);
        string[] files = Directory.GetFiles(dir.FullName, searchFileName, SearchOption.AllDirectories);
        return files.Any() ? files[0] : default;
    }

    /// <summary>
    /// 解析项目文件xml 获取名称,没有自定义则取文件名
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GetAssemblyName(FileInfo file)
    {
        XElement xml = XElement.Load(file.FullName);
        XElement? node = xml.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Elements())
            .Where(el => el.Name.LocalName.Equals("AssemblyName"))
            .FirstOrDefault();
        // 默认名称
        string name = Path.GetFileNameWithoutExtension(file.Name);
        if (node != null)
        {
            if (!node.Value.Contains("$(MSBuildProjectName)"))
            {
                name = node.Value;
            }
        }
        return name;
    }

    /// <summary>
    /// 获取项目类型
    /// </summary>
    /// <param name="file"></param>
    /// <returns>oneOf: null/web/console</returns>
    public static string? GetProjectType(FileInfo file)
    {
        XElement xml = XElement.Load(file.FullName);
        var sdk = xml.Attribute("Sdk")?.Value;
        // TODO:仅判断是否为web
        return sdk == null ? null :
            sdk.EndsWith("Sdk.Web")
            ? "web"
            : "console";
    }

    public static string? GetAssemblyName(DirectoryInfo dir)
    {
        FileInfo? file = FindProjectFile(dir);
        return file == null ? null : GetAssemblyName(file);
    }

    /// <summary>
    /// 获取命名空间名称
    /// 优先级，配置>项目名
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string? GetNamespaceName(DirectoryInfo dir)
    {
        FileInfo? file = FindProjectFile(dir);
        if (file == null)
        {
            return null;
        }
        XElement xml = XElement.Load(file.FullName);
        XElement? node = xml.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Elements())
            .Where(el => el.Name.LocalName.Equals("RootNamespace"))
            .FirstOrDefault();
        // 默认名称
        string name = Path.GetFileNameWithoutExtension(file.Name);
        if (node != null)
        {
            if (!node.Value.Contains("MSBuildProjectName"))
            {
                name = node.Value;
            }
        }
        return name;
    }

    /// <summary>
    /// 获取解决方案文件，从当前目录向根目录搜索
    /// </summary>
    /// <param name="dir">当前目录</param>
    /// <param name="root">要目录</param>
    /// <returns></returns>
    public static FileInfo? GetSlnFile(DirectoryInfo dir, DirectoryInfo? root = null)
    {
        try
        {
            FileInfo? file = dir.GetFiles("*.sln")?.FirstOrDefault();
            file ??= dir.GetFiles("*.slnx")?.FirstOrDefault();
            return root == null ? file
                : file == null && dir != root ? GetSlnFile(dir.Parent!, root) : file;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// 获取git根目录
    /// </summary>
    /// <param name="dir">搜索目录，从该目录向上递归搜索</param>
    /// <returns></returns>
    public static DirectoryInfo? GetGitRoot(DirectoryInfo dir)
    {
        try
        {
            DirectoryInfo? directory = dir.GetDirectories(".git").FirstOrDefault();
            return directory != null
                ? directory.Parent
                : directory == null && dir.Root != dir && dir.Parent != null ? GetGitRoot(dir.Parent) : default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// 获取当前工具运行版本
    /// </summary>
    /// <returns></returns>
    public static string GetCurrentToolVersion()
    {
        string? version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return version != null
            ? version.Split('+')[0]
            : Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
            ?? Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
            ?? string.Empty;
    }

    /// <summary>
    /// 获取解决方案版本
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <returns></returns>
    public static async Task<string?> GetSolutionVersionAsync(string solutionPath)
    {
        string configFilePath = Path.Combine(solutionPath, ConstVal.ConfigFileName);
        if (File.Exists(configFilePath))
        {
            string configJson = await File.ReadAllTextAsync(configFilePath);
            var config = JsonSerializer.Deserialize<ProjectConfig>(configJson);
            return config?.Version;
        }
        return default;
    }

    /// <summary>
    /// 获取当前项目下的 xml 注释中的members
    /// </summary>
    /// <returns></returns>
    public static List<XmlCommentMember>? GetXmlMembers(DirectoryInfo dir)
    {
        FileInfo? projectFile = dir.GetFiles($"*{ConstVal.CSharpProjectExtension}")?.FirstOrDefault();
        if (projectFile != null)
        {
            string assemblyName = GetAssemblyName(projectFile);
            FileInfo? xmlFile = dir.GetFiles($"{assemblyName}.xml", SearchOption.AllDirectories).FirstOrDefault();
            if (xmlFile != null)
            {
                XElement xml = XElement.Load(xmlFile.FullName);
                List<XmlCommentMember> members = xml.Descendants("member")
                    .Select(s => new XmlCommentMember
                    {
                        FullName = s.Attribute("name")?.Value ?? "",
                        Summary = s.Element("summary")?.Value

                    }).ToList();
                return members;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取studio目录
    /// </summary>
    /// <returns></returns>
    public static string GetStudioPath()
    {
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appPath, ConstVal.StudioDir, ConstVal.Version);
    }

    /// <summary>
    /// get csproject targetFramework 
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public static string? GetTargetFramework(string projectPath)
    {
        XElement xml = XElement.Load(projectPath);
        var targetFramework = xml.Descendants("TargetFramework").FirstOrDefault();
        return targetFramework?.Value;
    }

    /// <summary>
    /// 生成文件
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="fileName"></param>
    /// <param name="content"></param>
    /// <param name="cover"></param>
    /// <returns></returns>
    public static async Task GenerateFileAsync(string dir, string fileName, string content, bool cover = false)
    {
        if (!Directory.Exists(dir))
        {
            _ = Directory.CreateDirectory(dir);
        }
        string filePath = Path.Combine(dir, fileName);
        if (!File.Exists(filePath) || cover)
        {
            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine(@$"  ℹ️ generate file {fileName}.");
        }
        else
        {
            Console.WriteLine($"  🦘 Skip exist file: {fileName}.");
        }
    }

    public static async Task GenerateFileAsync(string filePath, string content, bool cover = false)
    {
        string fileName = Path.GetFileName(filePath);
        if (!File.Exists(filePath) || cover)
        {
            await File.WriteAllTextAsync(filePath, content);
            if (cover)
            {
                Console.WriteLine(@$"  ℹ️ update file {fileName}.");
            }
            else
            {
                Console.WriteLine(@$"  ✅ generate file {fileName}.");
            }

        }
        else
        {
            Console.WriteLine($"  🦘 Skip exist file: {fileName}.");
        }
    }

    /// <summary>
    /// 获取 dotnet tool 路径
    /// </summary>
    /// <returns></returns>
    public static string GetToolPath()
    {
        string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // 版本号
        string version = GetCurrentToolVersion();
        return Path.Combine(
            userPath,
            ".dotnet/tools/.store",
            ConstVal.PackageId,
            version,
            ConstVal.PackageId,
            version,
            "tools",
            ConstVal.NetVersion,
            "any");
    }

    /// <summary>
    /// get solution type
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static SolutionType GetSolutionType(string? filePath)
    {
        if (filePath.IsEmpty()) return SolutionType.Else;
        string fileName = Path.GetFileName(filePath);
        string fileExt = Path.GetExtension(filePath);
        return (SolutionType)(fileName == ConstVal.NodeProjectFile
            ? SolutionType.Node
            : fileExt switch
            {
                ConstVal.SolutionExtension or ConstVal.CSharpProjectExtension or ConstVal.SolutionXMLExtension => (SolutionType?)SolutionType.DotNet,
                _ => (SolutionType?)SolutionType.Else,
            });
    }

    /// <summary>
    /// 移除项目包
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="packageNames"></param>
    public static void RemovePackageReference(string projectPath, string[] packageNames)
    {
        packageNames.ToList().ForEach(package =>
        {
            if (!ProcessHelper.RunCommand("dotnet", $"remove {projectPath} package {string.Join(" ", packageNames)}", out string error))
            {
                Console.WriteLine("dotnet remove error:" + error);
            }
        });
    }

    /// <summary>
    /// 将目录结构转换成命名空间
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetNamespaceByPath(string path)
    {
        return path.Replace("src", "")
            .Replace(Path.PathSeparator, '.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Trim('.');
    }
}
public class XmlCommentMember
{
    public string FullName { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
