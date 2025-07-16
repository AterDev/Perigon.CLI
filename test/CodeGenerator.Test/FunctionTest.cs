using Microsoft.OpenApi.Readers;
using Share.Services;

namespace CodeGenerator.Test;

public class FunctionTest
{
    [Fact]
    public void TestString()
    {
        string? a = string.Empty;
        string? b = string.Empty;

        bool c = a == b;
        Assert.True(c);
    }

    [Fact]
    public void Should_parse_openApi()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Data\openapi.json");
        string openApiContent = File.ReadAllText(filePath);
        // 过滤特殊符号
        openApiContent = openApiContent.Replace("«", "").Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
        var helper = new OpenApiService(apiDocument);
        var apis = helper.RestApiGroups;
        Assert.NotNull(helper.RestApiGroups);
    }

    [Fact]
    public void Should_parse_enum()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        EntityParseHelper helper = new(filePath);
        helper.Parse();
        List<IFieldSymbol?>? members = helper.GetEnumMembers("EnumType");
        Assert.NotNull(members);
        bool condition = members.Any(m => m!.Name.Equals("Default"));
        Assert.True(condition);
    }

    [Theory]
    [InlineData("IList<abc>")]
    [InlineData("List<abc>")]
    [InlineData("ICollection<abc>")]
    [InlineData("IEnumerable<abc>")]
    public void Should_regex_listType(string type)
    {
        string? originType = EntityParseHelper.GetTypeFromList(type);
        Assert.Equal("abc", originType);
    }

    [Fact]
    public void Should_get_projectType()
    {
        string current = PathHelper.GetProjectPath();

        string projectFile = Path.Combine(current, "CodeGenerator.Test.csproj");

        string? type = AssemblyHelper.GetProjectType(new System.IO.FileInfo(projectFile));
        Assert.Equal("console", type);
    }

    [Fact]
    public void Should_Parse_Interface()
    {
        string projectPath = @"C:\codes\ater.web\templates\apistd\src\Application\";
        string interfaceName = "ISystemConfigManager";
        string filePath = Path.Combine(projectPath, "IManager", interfaceName + ".cs");

        CompilationHelper compilation = new(projectPath);
        compilation.LoadContent(File.ReadAllText(filePath));

        bool exist = compilation.MethodExist("Task<SystemConfig?> GetOwnedAsync(Guid id);");
        Assert.True(exist);

        compilation.InsertInterfaceMethod("Task<SystemConfig?> GetOwnedAsync(int id);");
        var content = compilation.SyntaxRoot!.ToFullString();

        Console.WriteLine();
    }

    [Fact]
    public void Test_JsonNode()
    {
        string jsonString = """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft": "Warning",
                  "Microsoft.Hosting.Lifetime": "Information"
                }
              },
                // 应用组件配置
                "Components": {
                  // 数据支持: pgsql/sqlserver
                  "Database": "postgresql",
                  // 缓存支持: redis/memory/none
                  "Cache": "redis",
                  // 日志支持: otlp/none
                  "Logging": "none",
                  // 是否使用swagger
                  "Swagger": true,
                  // 是否使用 jwt 验证
                  "Jwt": true
                },
                "ConnectionStrings": {
                  "Default": "Server=localhost;Port=5432;Database=v7._0;User Id=postgres;Password=root;",
                  "Redis": "localhost:6379",
                  "RedisInstanceName": "Dev"
                },
              "AllowedHosts": "*"
            }

            """;
        JsonNode? jsonNode = JsonNode.Parse(
            jsonString,
            documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }
        );
        var section = JsonHelper.GetSectionNode(jsonNode!, "ConnectionStrings");
        var value = JsonHelper.GetValue<string>(section!, "RedisInstanceName");
        JsonHelper.AddOrUpdateJsonNode(jsonNode!, "AllowedHosts", "111");
        var changeValaue = JsonHelper.GetValue<string>(jsonNode!, "AllowedHosts");
        Assert.Equal("Dev", value);
        Assert.Equal("111", changeValaue);
    }

    [Fact]
    public async Task Should_Analysis_CodeAsync()
    {
        string content = File.ReadAllText(
            @"D:\codes\v7.0\src\Application\IManager\ISystemRoleManager.cs"
        );
        SyntaxTree tree = CSharpSyntaxTree.ParseText(content);
        PortableExecutableReference mscorlib = MetadataReference.CreateFromFile(
            typeof(object).Assembly.Location
        );
        CSharpCompilation compilation = CSharpCompilation.Create(
            "MyCompilation",
            syntaxTrees: new[] { tree },
            references: new[] { mscorlib }
        );
        await CSharpAnalysisHelper.GetBaseInterfaceInfoAsync(compilation, tree);
    }

    [Fact]
    public void Should_ConvertJsonToCsharp()
    {
        string json = """
            {
              "description": {
                "title": "Contiguous U.S., Average Temperature",
                "units": "Degrees Fahrenheit",
                "base_period": "1901-2000",
                "time":"12:00:12",
                "date":"2013-12-12",
                "datetime":"2022-12-12T12:22:22Z"
              },
              "list":[
                  {
                      "text":"abc",
                      "des":"des",
                      "detail":null
                  }
              ],
              "enum":[1,2,3,4],
              "summary": {
                "title": "Contiguous U.S., Average Temperature",
                "units": "Degrees Fahrenheit",
                "base_period": "1901-2000",
                "abc":"222"
              },
              "data": {
                "189512": {
                  "value": "50.34",
                  "anomaly": "-1.68"
                },
                "189612": {
                  "value": "51.99",
                  "anomaly": "-0.03"
                },
                "188712": {
                  "value": "51.56",
                  "anomaly": "-0.46"
                },
                "adf":{
                    "aaa":"1111"
                }
              }
            }
            """;

        CSharpCovertHelper helper = new();
        if (CSharpCovertHelper.CheckJson(json))
        {
            JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            helper.ToCsharpClassContent(jsonElement);

            List<JsonMetadata> meta = helper.JsonMetadataList;
            helper.GenerateClass(jsonElement);

            List<string> classcodes = helper.ClassCodes;
            Console.WriteLine(classcodes.Count);
        }
    }

    [Fact]
    public void GetBaseLists_Should_Return_BaseTypes()
    {
        // Arrange
        CompilationHelper compilationHelper = new("./");
        compilationHelper.LoadContent("public class MyClass : MyBaseClass, IEntityBase { }");

        // Act
        List<BaseTypeSyntax>? baseTypes = compilationHelper.GetBaseLists();

        // Assert
        Assert.NotNull(baseTypes);

        Assert.Equal("MyBaseClass", baseTypes[0].ToString());

        baseTypes = compilationHelper.GetBaseLists("IEntityBase");
        Assert.NotNull(baseTypes);
        Assert.Equal("IEntityBase", baseTypes[0].ToString());
    }
}

public class Model
{
    public string Namespace { get; set; } = "";
    public string? EntityName { get; set; }
}
