using CodeGenerator.Helper;
using CodeGenerator.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata;
using Spectre.Console;
using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Share.Helper;

/// <summary>
/// dbcontext parse service
/// </summary>
public class DbContextParseHelper : IDisposable
{
    private FrozenDictionary<string, IModel>? _modelMap;
    private DbContextAnalyzer? _analyzer;
    private readonly XmlDocHelper _xmlHelper;
    private readonly CompilationHelper _compilation;
    private readonly string _entityFrameworkPath;
    private bool _disposed = false;

    // nullable to allow releasing references for unloading shadow load context
    public IModel? CurrentModel { get; private set; }
    public INamedTypeSymbol? DbContextSymbol { get; private set; }
    public string EntityPath { get; init; }
    private string EntityFilePath { get; set; } = string.Empty;

    // 记录并用来避免循环引用解析
    private readonly HashSet<IEntityType> RelationEntityTypes = [];

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
        _entityFrameworkPath = entityFrameworkPath;
        _xmlHelper = new XmlDocHelper(entityFrameworkPath);
        _compilation = new CompilationHelper(entityPath);
    }

    private void EnsureAnalyzerInitialized()
    {
        if (_analyzer == null)
        {
            OutputHelper.Info("🚀 Initializing DbContextAnalyzer...");
            _analyzer = new DbContextAnalyzer(_entityFrameworkPath);
            _modelMap = _analyzer.GetDbContextModels();
            CurrentModel = _modelMap.FirstOrDefault().Value;
            OutputHelper.Info($"✅ DbContextAnalyzer initialized with {_modelMap.Count} models");
        }
    }

    public async Task<IEntityType?> LoadEntityAsync(string entityFilePath)
    {
        EntityFilePath = entityFilePath ?? throw new ArgumentNullException(nameof(entityFilePath));

        try
        {
            OutputHelper.Info($"📂 Loading entity from: {entityFilePath}");
            EnsureAnalyzerInitialized();
            var fileContent = await File.ReadAllTextAsync(EntityFilePath);
            _compilation.LoadContent(fileContent);

            var entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
            OutputHelper.Info($"🔍 Looking for entity: {entityName}");

            DbContextSymbol = _analyzer!.GetDbContextType(entityName);

            if (DbContextSymbol != null)
            {
                OutputHelper.Info($"✅ Found DbContext for {entityName}: {DbContextSymbol.Name}");

                CurrentModel = _modelMap!
                    .FirstOrDefault(kvp =>
                        kvp.Key.Equals(DbContextSymbol.Name, StringComparison.OrdinalIgnoreCase)
                    )
                    .Value;

                var entityType = CurrentModel
                    .GetEntityTypes()
                    .FirstOrDefault(e =>
                        e.ClrType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase)
                    );

                if (entityType != null)
                {
                    OutputHelper.Info($"Successfully loaded entity type: {entityName}");
                }
                else
                {
                    OutputHelper.Warning($"Entity type not found in model: {entityName}");
                }

                return entityType;
            }
            else
            {
                OutputHelper.Error($"{entityName} not found in any DbContext, please Add it to the DbContext.");
                return null;
            }
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"Failed to load entity from {entityFilePath}: {ex.Message}");
            return null;
        }
    }

    public EntityInfo? GetEntityInfo(IEntityType entityType)
    {
        if (entityType == null)
        {
            OutputHelper.Warning("The Entity is not belongs to any DbContext");
            return null;
        }
        var namespaceName = _compilation.GetNamespace() ?? string.Empty;

        var entityInfo = new EntityInfo
        {
            FilePath = EntityFilePath,
            AssemblyName = entityType.ClrType.Assembly.GetName().Name,
            DbContextName = DbContextSymbol!.Name,
            DbContextSpaceName = DbContextSymbol.ContainingNamespace.ToString(),
            Name = entityType.ClrType.Name,
            NamespaceName = entityType.ClrType.Namespace ?? namespaceName,
            Summary =
                _xmlHelper.GetClassSummary(entityType.ClrType.FullName ?? namespaceName)
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
        RelationEntityTypes.Add(entityType);
        LoadEntityProperties(entityInfo, entityType);
        LoadEntityNavigations(entityInfo, entityType);
        return entityInfo;
    }

    private void LoadEntityProperties(EntityInfo entityInfo, IEntityType entityType)
    {
        if (entityType == null)
        {
            OutputHelper.Error("The entityType is null");
            return;
        }
        var props = new List<PropertyInfo>();
        // 解析普通属性
        foreach (var prop in entityType.GetProperties())
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
                    || (prop.ClrType.IsGenericType
                        && typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.ClrType)),
                IsDecimal = prop.ClrType == typeof(decimal),
                IsShadow = prop.IsShadowProperty(),
                IsIndex = prop.IsIndex(),
                CommentSummary =
                    _xmlHelper.GetPropertySummary(
                        entityType.ClrType.FullName ?? string.Empty,
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

    private void LoadEntityNavigations(EntityInfo entityInfo, IEntityType entityType)
    {
        if (entityType == null)
        {
            OutputHelper.Error("The entityType is null");
            return;
        }
        var navigations = new List<EntityNavigation>();

        foreach (var nav in entityType.GetNavigations())
        {
            var navigation = new EntityNavigation
            {
                Name = nav.Name,
                Type = CSharpAnalysisHelper.ToTypeName(nav.ClrType),
                ForeignKey = nav
                    .ForeignKey.Properties.Select(p => p.Name)
                    .Aggregate((current, next) => $"{current}, {next}"),

                ForeignKeyProperties = nav
                    .ForeignKey.Properties.Select(p => new PropertyInfo
                    {
                        Name = p.Name,
                        Type = CSharpAnalysisHelper.ToTypeName(p.ClrType),
                        IsNullable = p.IsNullable,
                        IsForeignKey = true,
                        IsShadow = p.IsShadowProperty(),
                        IsIndex = p.IsIndex(),
                    })
                    .ToList(),
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

            // 同时解析导航属性的属性
            var navEntityType = nav.TargetEntityType;
            if (!RelationEntityTypes.Contains(navEntityType))
            {
                // TODO:未加载导航属性类内容，问题不大
                navigation.EntityInfo = GetEntityInfo(navEntityType);
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


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                OutputHelper.Info("🧹 DbContextParseHelper disposing...");

                // 释放 analyzer
                if (_analyzer != null)
                {
                    _analyzer.Dispose();
                    _analyzer = null;
                    OutputHelper.Info("✅ DbContextAnalyzer disposed");
                }

                // 释放模型引用，便于 ALC 卸载
                _modelMap = null;
                CurrentModel = null;
                DbContextSymbol = null;
                RelationEntityTypes?.Clear();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                OutputHelper.Info("✅ DbContextParseHelper disposed successfully");
            }
            catch (Exception ex)
            {
                OutputHelper.Error($"❌ Error during DbContextParseHelper disposal: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    ~DbContextParseHelper()
    {
        Dispose(false);
    }
}
