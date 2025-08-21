using System.Text.RegularExpressions;
using Entity.StudioMod;

namespace CodeGenerator.Helper;

/// <summary>
/// 类型解析帮助类
/// </summary>
public class EntityParseHelper
{
    /// <summary>
    /// 类名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    public string? NamespaceName { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AssemblyName { get; set; }
    public FileInfo ProjectFile { get; set; }

    /// <summary>
    /// 类原始注释
    /// </summary>
    public string? Comment { get; set; }
    public string? CommentContent { get; set; }

    public string FilePath { get; set; }

    /// <summary>
    /// 前端对应模块
    /// </summary>
    public string? NgModuleName { get; set; }

    /// <summary>
    /// 前端对应路由
    /// </summary>
    public string? NgRoute { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    public List<PropertyInfo> PropertiesInfo { get; set; } = [];
    public CSharpCompilation Compilation { get; set; }
    public SemanticModel? SemanticModel { get; set; }
    protected SyntaxTree? SyntaxTree { get; set; }
    public IEnumerable<SyntaxNode>? RootNodes { get; set; }
    public CompilationHelper CompilationHelper { get; set; }
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;
    public string[] SpecialTypes = ["DateTime", "DateTimeOffset", "DateOnly", "TimeOnly", "Guid"];

    /// <summary>
    /// 可解析的特性
    /// </summary>
    public string[] ValidAttributes =
    [
        "MaxLength",
        "MinLength",
        "StringLength",
        "Length",
        "Range",
        "AllowedValues",
    ];

    /// <summary>
    /// 解决方案代码文件路径
    /// </summary>
    public List<string> CodeFilesPath { get; set; } = [];

    public EntityParseHelper(string filePath)
    {
        FilePath = filePath;

        FileInfo fileInfo = new(filePath);
        FileInfo? projectFile =
            AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root)
            ?? throw new ArgumentException("can't find project file");
        ProjectFile = projectFile;
        CodeFilesPath = ProjectFile
            .Directory!.GetFiles("*.cs", SearchOption.AllDirectories)
            .Select(f => f.FullName)
            .ToList();
        AssemblyName = GetAssemblyName();
        CompilationHelper = new CompilationHelper(ProjectFile.Directory!.FullName);

        string content = File.ReadAllText(fileInfo.FullName);
        CompilationHelper.LoadContent(content);
        SyntaxTree = CompilationHelper.SyntaxTree;
        Compilation = CompilationHelper.Compilation;
        SemanticModel = CompilationHelper.SemanticModel;
        RootNodes = SyntaxTree?.GetCompilationUnitRoot().DescendantNodes();
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    public string GetAssemblyName()
    {
        return AssemblyHelper.GetAssemblyName(ProjectFile);
    }

    /// <summary>
    /// 加载内容
    /// </summary>
    /// <param name="content"></param>
    public void LoadEntityContent(string content)
    {
        CompilationHelper.LoadContent(content);
        SyntaxTree = CompilationHelper.SyntaxTree;
        Compilation = CompilationHelper.Compilation;
        SemanticModel = CompilationHelper.SemanticModel;
        RootNodes = SyntaxTree?.GetCompilationUnitRoot().DescendantNodes();
    }

    public void Parse()
    {
        // 获取当前类名
        ClassDeclarationSyntax? classDeclarationSyntax = RootNodes
            ?.OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();
        NamespaceName = CompilationHelper.GetNamespace();
        Name = classDeclarationSyntax?.Identifier.ToString();
        Comment = GetClassComment(classDeclarationSyntax);
        CommentContent = GetCommentFromXmlDoc();
        PropertiesInfo = GetPropertiesInfo();
    }

    /// <summary>
    /// 解析实体
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<EntityInfo?> ParseEntityAsync(string? filePath = null)
    {
        filePath ??= FilePath;
        if (File.Exists(filePath))
        {
            string content = await File.ReadAllTextAsync(filePath);
            CompilationHelper = new CompilationHelper(ProjectFile.Directory!.FullName);
            CompilationHelper.LoadContent(content);
            SyntaxTree = CompilationHelper.SyntaxTree;
            Compilation = CompilationHelper.Compilation;
            SemanticModel = CompilationHelper.SemanticModel;
            RootNodes = SyntaxTree?.GetCompilationUnitRoot().DescendantNodes();

            var classDeclarationSyntax = RootNodes
                ?.OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
            Name = classDeclarationSyntax?.Identifier.ToString();
            NamespaceName = CompilationHelper.GetNamespace();
            Comment = GetClassComment(classDeclarationSyntax);

            return new EntityInfo()
            {
                Name = Name!,
                FilePath = filePath,
                AssemblyName = AssemblyName,
                NamespaceName = NamespaceName ?? "",
                Comment = Comment,
                Summary = GetCommentFromXmlDoc(),
                PropertyInfos = GetPropertiesInfo(),
                KeyType = KeyType,
                ModuleName = GetModuleName(),
            };
        }
        return null;
    }

    /// <summary>
    /// 获取枚举members
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public List<IFieldSymbol?>? GetEnumMembers(string name)
    {
        // 获取指定枚举类字段内容
        INamedTypeSymbol? enumSymbol = CompilationHelper.GetEnum(name);
        return enumSymbol
            ?.GetMembers()
            .Where(m => m.Name is not "value__")
            .Select(m => m as IFieldSymbol)
            .ToList();
    }

    public static string GetClassComment(ClassDeclarationSyntax? syntax)
    {
        if (syntax == null)
        {
            return string.Empty;
        }

        SyntaxTriviaList triviaList = syntax.GetLeadingTrivia();
        string comment = triviaList.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(comment) && !comment.StartsWith("///"))
        {
            comment = "/// " + comment;
        }
        return comment;
    }

    /// <summary>
    /// get module name
    /// </summary>
    /// <returns></returns>
    public string? GetModuleName()
    {
        var moduleAttribution = CompilationHelper.GetClassAttribution("Module");
        if (moduleAttribution != null && moduleAttribution.Count != 0)
        {
            var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
            if (argument != null)
            {
                return CompilationHelper.GetArgumentValue(argument);
            }
        }
        return null;
    }

    /// <summary>
    /// 获取类的注释
    /// </summary>
    /// <returns></returns>
    private string? GetCommentFromXmlDoc()
    {
        List<XmlCommentMember>? members = AssemblyHelper.GetXmlMembers(ProjectFile.Directory!);
        return members
            ?.Where(m => m.FullName.EndsWith(NamespaceName + "." + Name))
            .Select(s => s.Summary?.Trim())
            .FirstOrDefault();
    }

    /// <summary>
    /// 获取该类的所有属性
    /// </summary>
    /// <returns></returns>
    public List<PropertyInfo> GetPropertiesInfo(string? parentClassName = null)
    {
        List<PropertyInfo> properties = [];
        CompilationUnitSyntax root = SyntaxTree!.GetCompilationUnitRoot();
        string? matchClassName = parentClassName ?? Name;

        ClassDeclarationSyntax? classDeclarationSyntax = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        var propertySyntax =
            classDeclarationSyntax?.DescendantNodes().OfType<PropertyDeclarationSyntax>() ?? [];

        // 如果指定父类名称
        parentClassName ??= CompilationHelper.GetParentClassName();

        List<PropertyInfo>? parentProperties = [];
        if (parentClassName != null)
        {
            string? filePath = CodeFilesPath
                .Where(c => c.EndsWith($"{parentClassName}.cs"))
                .FirstOrDefault();

            if (filePath != null)
            {
                EntityParseHelper entity = new(filePath);
                parentProperties = entity.GetPropertiesInfo();
            }
            else
            {
                //GetParentProperties();
            }
        }
        foreach (var prop in propertySyntax)
        {
            // type and name
            PropertyInfo propertyInfo = ParsePropertyType(prop);
            // attribute and comments text
            propertyInfo.AttributeText = GetAttributeText(prop);
            propertyInfo.CommentXml = GetCommentXml(prop);
            propertyInfo.CommentSummary = GetCommentSummary(prop);

            // weather property has set method?
            propertyInfo.HasSet =
                prop.AccessorList?.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration)
                ?? false;

            // attributes
            ParsePropertyAttributes(prop, propertyInfo);
            properties.Add(propertyInfo);
        }

        if (parentProperties != null)
        {
            properties.AddRange(parentProperties);
        }

        return properties.GroupBy(p => p.Name).Select(s => s.FirstOrDefault()!).ToList();
    }

    /// <summary>
    /// 获取原始类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string? GetTypeFromList(string type)
    {
        string pattern = @"\w+?<(?<Type>\w+)>";
        Match match = Regex.Match(type, pattern);
        return match.Success ? (match.Groups["Type"]?.Value) : null;
    }

    /// <summary>
    /// 获取属性注释xml内容
    /// </summary>
    /// <returns></returns>
    public static string GetCommentXml(PropertyDeclarationSyntax syntax)
    {
        DocumentationCommentTriviaSyntax? trivia = syntax
            .GetLeadingTrivia()
            .Select(x => x.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        // 缩进空格
        var whiteTrivia = syntax
            .GetLeadingTrivia()
            .Where(x => x.IsKind(SyntaxKind.WhitespaceTrivia))
            .FirstOrDefault();
        return trivia == null
            ? string.Empty
            : whiteTrivia.ToString() + "///" + trivia.Content.ToString();
    }

    /// <summary>
    /// 获取summary comment
    /// </summary>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public static string? GetCommentSummary(PropertyDeclarationSyntax syntax)
    {
        DocumentationCommentTriviaSyntax? trivia = syntax
            .GetLeadingTrivia()
            .Select(x => x.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();
        if (trivia == null)
        {
            return null;
        }
        XmlElementSyntax? summary = trivia
            .Content.OfType<XmlElementSyntax>()
            .Where(e => e.StartTag.Name.ToString() == "summary")
            .FirstOrDefault();

        if (summary == null)
            return null;

        XmlTextSyntax? contentNode = summary?.ChildNodes().OfType<XmlTextSyntax>().FirstOrDefault();
        return contentNode != null
            ? string.Join(
                ' ',
                contentNode
                    .TextTokens.Where(x => x.IsKind(SyntaxKind.XmlTextLiteralToken))
                    .Select(x => x.Text.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()
            )
            : null;
    }

    /// <summary>
    /// 获取属性特性文本内容
    /// </summary>
    /// <returns></returns>
    public static string GetAttributeText(PropertyDeclarationSyntax syntax)
    {
        List<AttributeListSyntax> attributeListSyntax = syntax.AttributeLists.ToList();
        return string.Join(Environment.NewLine, attributeListSyntax.Select(a => a.ToString()));
    }

    /// <summary>
    /// 对属性的类型进行解析
    /// </summary>
    /// <param name="syntax"></param>
    protected PropertyInfo ParsePropertyType(PropertyDeclarationSyntax syntax)
    {
        string[] listTypes = ["IList", "List", "ICollection", "IEnumerable"];
        string type = syntax.Type.ToString();
        string name = syntax.Identifier.ToString();
        Microsoft.CodeAnalysis.TypeInfo typeInfo = SemanticModel.GetTypeInfo(syntax.Type);
        PropertyInfo propertyInfo = new() { Name = name, Type = type };

        // //解析modifier，如public required ,private virtual
        string modifier1 = syntax.Modifiers.FirstOrDefault().Text;
        string? modifier2 = null;

        if (syntax.Modifiers.Count > 1)
        {
            modifier2 = syntax.Modifiers.LastOrDefault().Text;
        }

        if (string.IsNullOrEmpty(modifier1) || !modifier1.ToLower().Equals("public"))
        {
            propertyInfo.IsPublic = false;
        }
        if (!string.IsNullOrEmpty(modifier2) && modifier2.Trim().ToLower().Equals("required"))
        {
            propertyInfo.IsRequired = true;
        }
        // 移除?，获取类型名
        if (type.EndsWith('?'))
        {
            propertyInfo.Type = type[..^1];
            propertyInfo.IsNullable = true;
        }
        // 是否可空
        if (typeInfo.Type!.Name.Equals("Nullable"))
        {
            propertyInfo.IsNullable = true;
        }
        if (propertyInfo.Type.StartsWith("decimal"))
        {
            propertyInfo.IsDecimal = true;
        }
        // 判断是否为枚举类型(待改进)
        List<string> enums = CompilationHelper.GetAllEnumClasses();
        if (
            typeInfo.Type.TypeKind == TypeKind.Enum
            || typeInfo.Type.BaseType?.Name == "Enum"
            || enums.Any(e => e == propertyInfo.Type)
        )
        {
            propertyInfo.IsEnum = true;
        }
        // 列表的判断
        if (propertyInfo.Type.EndsWith("[]"))
        {
            propertyInfo.IsList = true;
        }
        // 正则匹配 \s+(\w+)<(\w+)>
        string pattern = @"(?<Type>\w+)<(?<GenericType>\w+)>";
        Match match = Regex.Match(type, pattern);
        if (match.Success)
        {
            if (listTypes.Contains(match.Groups["Type"]?.Value))
            {
                propertyInfo.IsList = true;
            }
        }
        // 默认值
        if (syntax.Initializer != null)
        {
            propertyInfo.DefaultValue = syntax.Initializer.Value.ToFullString();

            if (
                propertyInfo.DefaultValue.StartsWith("null!")
                || propertyInfo.DefaultValue.StartsWith("default!")
            )
            {
                propertyInfo.IsRequired = true;
                propertyInfo.IsNullable = false;
            }
        }

        // 导航属性判断
        if (typeInfo.Type is INamedTypeSymbol typeSymbol)
        {
            ParseNavigation(typeSymbol, propertyInfo);
        }

        return propertyInfo;
    }

    /// <summary>
    /// 解析导航属性
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyInfo"></param>
    protected void ParseNavigation(INamedTypeSymbol type, PropertyInfo propertyInfo)
    {
        INamedTypeSymbol? navigationType = type;
        bool hasMany = false;
        // 可空的列表，取泛型类型
        if (propertyInfo.IsNullable && navigationType.TypeArguments.Any())
        {
            navigationType = navigationType.TypeArguments[0] as INamedTypeSymbol;
            if (propertyInfo.IsList && navigationType != null && navigationType.TypeArguments.Any())
            {
                navigationType = navigationType.TypeArguments[0] as INamedTypeSymbol;
                propertyInfo.HasMany = true;
            }
        }
        else if (propertyInfo.IsList && navigationType.TypeArguments.Any())
        {
            navigationType = navigationType.TypeArguments[0] as INamedTypeSymbol;
            hasMany = true;
        }
        // 自定义类型
        if (
            navigationType?.SpecialType == SpecialType.None
            && !SpecialTypes.Contains(navigationType.Name)
            && !propertyInfo.IsEnum
        )
        {
            propertyInfo.NavigationName = navigationType.Name;
            propertyInfo.HasMany = hasMany;

            string? parentClassName = GetParentClassName(navigationType);

            // TODO:暂时用指定名称判断
            if (parentClassName is "EntityBase" or "IEntityBase")
            {
                propertyInfo.IsNavigation = true;
            }
            else
            {
                propertyInfo.IsComplexType = true;
            }
        }
    }

    /// <summary>
    /// 解析属性特性
    /// </summary>
    protected void ParsePropertyAttributes(
        PropertyDeclarationSyntax syntax,
        PropertyInfo propertyInfo
    )
    {
        if (propertyInfo.Type.Equals("string", StringComparison.OrdinalIgnoreCase))
        {
            propertyInfo.MaxLength = 200;
        }

        List<AttributeSyntax> attributes = syntax
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .ToList();
        if (attributes != null && attributes.Count > 0)
        {
            AttributeArgumentSyntax? maxLength = GetAttributeArguments(attributes, "MaxLength")
                ?.FirstOrDefault();
            AttributeArgumentSyntax? minLength = GetAttributeArguments(attributes, "MinLength")
                ?.FirstOrDefault();
            AttributeArgumentSyntax? stringLength = GetAttributeArguments(
                attributes,
                "StringLength"
            )
                ?.FirstOrDefault();

            IEnumerable<AttributeArgumentSyntax>? length = GetAttributeArguments(
                attributes,
                "Length"
            );
            IEnumerable<AttributeArgumentSyntax>? range = GetAttributeArguments(
                attributes,
                "Range"
            );
            IEnumerable<AttributeArgumentSyntax>? allowedValues = GetAttributeArguments(
                attributes,
                "AllowedValues"
            );
            IEnumerable<AttributeArgumentSyntax>? required = GetAttributeArguments(
                attributes,
                "Required"
            );
            IEnumerable<AttributeArgumentSyntax>? key = GetAttributeArguments(attributes, "Key");
            IEnumerable<AttributeArgumentSyntax>? jsonIgnore = GetAttributeArguments(
                attributes,
                "JsonIgnore"
            );

            if (key != null)
            {
                KeyType = propertyInfo.Type.ToLower() switch
                {
                    "string" => EntityKeyType.String,
                    "int" => EntityKeyType.Int,
                    _ => EntityKeyType.Guid,
                };
            }
            if (required != null)
            {
                propertyInfo.IsRequired = true;
            }
            if (jsonIgnore != null)
            {
                propertyInfo.IsJsonIgnore = true;
            }
            if (length != null)
            {
                if (int.TryParse(length.FirstOrDefault()?.ToString(), out int val))
                {
                    propertyInfo.MinLength = val;
                }
                if (int.TryParse(length.Skip(1).FirstOrDefault()?.ToString(), out int max))
                {
                    propertyInfo.MaxLength = max;
                }
            }
            if (range != null)
            {
                if (int.TryParse(range.FirstOrDefault()?.ToString(), out int val))
                {
                    propertyInfo.MinLength = val;
                }
                if (int.TryParse(range.Skip(1).FirstOrDefault()?.ToString(), out int max))
                {
                    propertyInfo.MaxLength = max;
                }
            }
            if (allowedValues != null)
            {
                // TODO:
            }
            if (maxLength != null)
            {
                if (int.TryParse(maxLength.ToString(), out int val))
                {
                    propertyInfo.MaxLength = val;
                }
            }
            if (minLength != null)
            {
                if (int.TryParse(minLength.ToString(), out int val))
                {
                    propertyInfo.MinLength = val;
                }
            }
            if (stringLength != null)
            {
                if (!int.TryParse(stringLength.ToString(), out int val))
                {
                    propertyInfo.MaxLength = val;
                }
            }
        }
    }

    /// <summary>
    /// 获取特性中的参数内容
    /// </summary>
    /// <param name="syntax"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    protected static IEnumerable<AttributeArgumentSyntax>? GetAttributeArguments(
        List<AttributeSyntax> syntax,
        string attributeName
    )
    {
        AttributeSyntax? theSyntax = syntax
            .Where(s => s.Name.ToString().ToLower().Equals(attributeName.ToLower()))
            .FirstOrDefault();
        return theSyntax != null
            ? theSyntax.ArgumentList?.Arguments
                ?? new SeparatedSyntaxList<AttributeArgumentSyntax>()
            : null;
    }

    /// <summary>
    /// 获取父类名称
    /// </summary>
    /// <returns></returns>
    public string? GetParentClassName()
    {
        // 获取当前类名
        ClassDeclarationSyntax? classDeclarationSyntax = RootNodes!
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();
        if (classDeclarationSyntax == null)
            return null;
        var classSymbol = SemanticModel.GetDeclaredSymbol(classDeclarationSyntax!);

        return classSymbol?.BaseType?.Name;
    }

    /// <summary>
    /// 获取父类
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public static string? GetParentClassName(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.BaseType?.Name;
    }

    /// <summary>
    /// 获取最初始基类
    /// </summary>
    /// <returns></returns>
    public static bool HasBaseType(INamedTypeSymbol? baseType, string baseName)
    {
        return baseType != null
            && (
                baseType.Name == baseName
                || baseType.BaseType != null && HasBaseType(baseType.BaseType, baseName)
            );
    }
}
