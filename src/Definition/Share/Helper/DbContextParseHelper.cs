using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json.Serialization;
using CodeGenerator.Helper;
using CodeGenerator.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata;
using Spectre.Console;

namespace Share.Helper;

/// <summary>
/// dbcontext parse service
/// </summary>
public class DbContextParseHelper
{
    private readonly FrozenDictionary<string, IModel> _modelMap;
    private readonly DbContextAnalyzer _analyzer;
    private readonly XmlDocHelper _xmlHelper;

    private readonly CompilationHelper _compilation;

    private IEntityType? CurrentEntity { get; set; }
    public IModel CurrentModel { get; private set; }
    public INamedTypeSymbol? DbContextSymbol { get; private set; }
    public string EntityPath { get; init; }
    private string EntityFilePath { get; set; } = string.Empty;

    public DbContextParseHelper(string entityPath, string entityFrameworkPath)
    {
        EntityPath = entityPath;
        if (string.IsNullOrEmpty(entityFrameworkPath))
        {
            throw new ArgumentException(
                "Entity Framework path cannot be null or empty.",
                nameof(entityFrameworkPath)
            );
        }
        _analyzer = new DbContextAnalyzer(entityFrameworkPath);
        _modelMap = _analyzer.GetDbContextModels();
        _xmlHelper = new XmlDocHelper(entityFrameworkPath);
        _compilation = new CompilationHelper(entityPath);
        CurrentModel = _modelMap.FirstOrDefault().Value;
    }

    public async Task LoadEntityAsync(string entityFilePath)
    {
        EntityFilePath = entityFilePath ?? throw new ArgumentNullException(nameof(entityFilePath));
        var fileContent = await File.ReadAllTextAsync(EntityFilePath);
        _compilation.LoadContent(fileContent);

        var entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        DbContextSymbol = _analyzer.GetDbContextType(entityName);
        if (DbContextSymbol != null)
        {
            CurrentModel = _modelMap
                .FirstOrDefault(kvp =>
                    kvp.Key.Equals(DbContextSymbol.Name, StringComparison.OrdinalIgnoreCase)
                )
                .Value;
            CurrentEntity = CurrentModel
                .GetEntityTypes()
                .FirstOrDefault(e =>
                    e.ClrType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase)
                );
        }
    }

    public EntityInfo? GetEntityInfo()
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Warning("The Entity is not belongs to any DbContext");
            return null;
        }
        var namespaceName = _compilation.GetNamespace() ?? string.Empty;

        var entityInfo = new EntityInfo
        {
            FilePath = EntityFilePath,
            AssemblyName = CurrentEntity.ClrType.Assembly.GetName().Name,
            DbContextName = DbContextSymbol!.Name,
            DbContextSpaceName = DbContextSymbol.ContainingNamespace.ToString(),
            Name = CurrentEntity.ClrType.Name,
            NamespaceName = CurrentEntity.ClrType.Namespace ?? namespaceName,
            Summary =
                _xmlHelper.GetClassSummary(CurrentEntity.ClrType.FullName ?? namespaceName)
                ?? string.Empty,
        };
        // module attribution
        var moduleAttribution = _compilation.GetClassAttribution("Module");
        if (moduleAttribution != null && moduleAttribution.Count != 0)
        {
            var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
            if (argument != null)
            {
                entityInfo.ModuleName = _compilation.GetArgumentValue(argument);
            }
        }
        // class xml comment
        entityInfo.Comment = EntityParseHelper.GetClassComment(_compilation.ClassNode);
        LoadEntityProperties(entityInfo);
        LoadEntityNavigations(entityInfo);

        return entityInfo;
    }

    private void LoadEntityProperties(EntityInfo entityInfo)
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Error("The currentEntity is null");
            return;
        }
        var props = new List<PropertyInfo>();
        // 解析普通属性
        foreach (var prop in CurrentEntity.GetProperties())
        {
            var info = new PropertyInfo
            {
                Name = prop.Name,
                Type = CSharpAnalysisHelper.ToTypeName(prop.ClrType),
                IsNullable = prop.IsNullable,
                IsForeignKey = prop.IsForeignKey(),
                IsEnum = prop.ClrType.IsEnum,
                HasSet = true,
                MaxLength = prop.GetMaxLength(),
                IsList =
                    prop.ClrType.IsArray
                    || prop.ClrType.IsGenericType
                        && typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.ClrType),
                IsDecimal = prop.ClrType == typeof(decimal),
                IsShadow = prop.IsShadowProperty(),
                IsIndex = prop.IsIndex(),
                CommentSummary =
                    _xmlHelper.GetPropertySummary(
                        CurrentEntity.ClrType.FullName ?? string.Empty,
                        prop.Name
                    ) ?? string.Empty,
            };

            if (prop.PropertyInfo != null)
            {
                ParsePropertyAttributes(info, prop.PropertyInfo);
            }
            ParseSyntaxInfo(info);
            props.Add(info);
        }

        entityInfo.PropertyInfos = props;
    }

    private void LoadEntityNonDbProperties(EntityInfo entityInfo)
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Error("The currentEntity is null");
            return;
        }

        // 其他非映射属性
        var nonDbProps = CurrentEntity
            .ClrType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => !entityInfo.PropertyInfos.Any(ep => ep.Name == p.Name))
            .Where(p => !entityInfo.Navigations.Any(n => n.Name == p.Name))
            .ToList();

        foreach (var prop in nonDbProps)
        {
            var info = new PropertyInfo
            {
                Name = prop.Name,
                Type = CSharpAnalysisHelper.ToTypeName(prop.PropertyType),
                IsNullable =
                    !prop.PropertyType.IsValueType
                    || Nullable.GetUnderlyingType(prop.PropertyType) != null,
                IsForeignKey = false,
                IsEnum = prop.PropertyType.IsEnum,
                HasSet = prop.CanWrite,
                IsList =
                    prop.PropertyType.IsArray
                    || prop.PropertyType.IsGenericType
                        && typeof(System.Collections.IEnumerable).IsAssignableFrom(
                            prop.PropertyType
                        ),
                IsDecimal = prop.PropertyType == typeof(decimal),
            };
            ParsePropertyAttributes(info, prop);
            ParseSyntaxInfo(info);
            entityInfo.PropertyInfos.Add(info);
        }
    }

    private void LoadEntityNavigations(EntityInfo entityInfo)
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Error("The currentEntity is null");
            return;
        }
        var navigations = new List<EntityNavigation>();
        foreach (var nav in CurrentEntity.GetNavigations())
        {
            var navigation = new EntityNavigation
            {
                Name = nav.Name,
                Type = CSharpAnalysisHelper.ToTypeName(nav.ClrType),
                ForeignKey = nav
                    .ForeignKey.Properties.Select(p => p.Name)
                    .Aggregate((current, next) => $"{current}, {next}"),

                IsRequired = nav.ForeignKey.IsRequired,
                IsUnique = nav.ForeignKey.IsUnique,
                IsCollection = nav.IsCollection,
                IsSkipNavigation = nav is ISkipNavigation,
                IsOwnership = nav.ForeignKey.IsOwnership,
            };

            if (nav.TargetEntityType.ClrType.FullName != null)
            {
                navigation.Summary =
                    _xmlHelper.GetPropertySummary(nav.TargetEntityType.ClrType.FullName, nav.Name)
                    ?? string.Empty;
            }
            navigations.Add(navigation);
        }
        entityInfo.Navigations = navigations;
    }

    private void ParsePropertyAttributes(
        PropertyInfo propertyInfo,
        System.Reflection.PropertyInfo prop
    )
    {
        var attributes = prop.GetCustomAttributes();
        foreach (var attr in attributes)
        {
            switch (attr)
            {
                case MinLengthAttribute minLengthAttr:
                    propertyInfo.MinLength = minLengthAttr.Length;
                    break;
                case MaxLengthAttribute maxLengthAttr:
                    propertyInfo.MaxLength = maxLengthAttr.Length;
                    break;
                case RequiredAttribute:
                    propertyInfo.IsRequired = true;
                    break;
                case StringLengthAttribute stringLengthAttr:
                    propertyInfo.MinLength = stringLengthAttr.MinimumLength;
                    propertyInfo.MaxLength = stringLengthAttr.MaximumLength;
                    break;
                case JsonIgnoreAttribute:
                    propertyInfo.IsJsonIgnore = true;
                    break;
            }
        }
    }

    private void ParseSyntaxInfo(PropertyInfo propertyInfo)
    {
        var propertySyntaxList =
            _compilation.ClassNode?.DescendantNodes().OfType<PropertyDeclarationSyntax>() ?? [];
        var propertySyntax = propertySyntaxList.FirstOrDefault(p =>
            p.Identifier.Text == propertyInfo.Name
        );

        if (propertySyntax == null)
        {
            return;
        }
        if (propertySyntax.Initializer != null)
        {
            propertyInfo.DefaultValue = propertySyntax.Initializer.Value.ToFullString();
        }

        propertyInfo.CommentXml = EntityParseHelper.GetCommentXml(propertySyntax!);
        propertyInfo.AttributeText = EntityParseHelper.GetAttributeText(propertySyntax!);

        string? modifier2 = null;

        if (propertySyntax.Modifiers.Count > 1)
        {
            modifier2 = propertySyntax.Modifiers.LastOrDefault().Text;
            if (!string.IsNullOrEmpty(modifier2) && modifier2.Trim().ToLower().Equals("required"))
            {
                propertyInfo.IsRequired = true;
            }
        }
    }
}
