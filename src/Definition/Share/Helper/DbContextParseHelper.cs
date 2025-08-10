using System.Collections.Frozen;
using CodeGenerator.Helper;
using CodeGenerator.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Share.Helper;

/// <summary>
/// dbcontext parse service
/// </summary>
public class DbContextParseHelper
{
    private readonly FrozenDictionary<string, IModel> _modelMap;
    private readonly DbContextAnalyzer _analyzer;
    private readonly XmlDocHelper _xmlHelper;

    private IEntityType? CurrentEntity { get; set; }
    public IModel CurrentModel { get; private set; }
    public string? DbContextName { get; private set; }
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
        CurrentModel = _modelMap.FirstOrDefault().Value;
    }

    public void LoadEntity(string entityFilePath)
    {
        EntityFilePath = entityFilePath ?? throw new ArgumentNullException(nameof(entityFilePath));
        var entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        var dbContextSymbol = _analyzer.GetDbContextType(entityName);
        if (dbContextSymbol != null)
        {
            DbContextName = dbContextSymbol.Name;
            CurrentModel = _modelMap
                .FirstOrDefault(kvp =>
                    kvp.Key.Equals(DbContextName, StringComparison.OrdinalIgnoreCase)
                )
                .Value;

            CurrentEntity = CurrentModel
                .GetEntityTypes()
                .FirstOrDefault(e =>
                    e.ClrType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase)
                );
        }
    }

    public async Task<EntityInfo?> GetEntityInfo()
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Warning("The Entity is not belongs to any DbContext");
            return null;
        }
        var entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        var compilationHelper = new CompilationHelper(EntityPath);
        var fileContent = await File.ReadAllTextAsync(EntityFilePath);

        compilationHelper.LoadContent(fileContent);
        var namespaceName = compilationHelper.GetNamespace() ?? string.Empty;
        var entityFullName = $"{namespaceName}.{entityName}";

        var entityInfo = new EntityInfo
        {
            FilePath = EntityFilePath,
            Name = CurrentEntity.ClrType.Name,
            NamespaceName = CurrentEntity.ClrType.Namespace ?? namespaceName,
            Summary =
                _xmlHelper.GetClassSummary(CurrentEntity.ClrType.FullName ?? namespaceName)
                ?? string.Empty,
        };
        // module attribution
        var moduleAttribution = compilationHelper.GetClassAttribution("Module");
        if (moduleAttribution != null && moduleAttribution.Count != 0)
        {
            var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
            if (argument != null)
            {
                entityInfo.ModuleName = compilationHelper.GetArgumentValue(argument);
            }
        }
        // class xml comment
        entityInfo.Comment = EntityParseHelper.GetClassComment(compilationHelper.ClassNode);
        LoadEntityProperties(entityInfo);
        LoadEntityNavigations(entityInfo);

        return entityInfo;
    }

    public void LoadEntityProperties(EntityInfo entityInfo)
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Error("The currentEntity is null");
            return;
        }
        var result = new List<PropertyInfo>();
        // 解析普通属性
        foreach (var prop in CurrentEntity.GetProperties())
        {
            var info = new PropertyInfo
            {
                Name = prop.Name,
                Type = CSharpAnalysisHelper.ToTypeName(prop.ClrType),
                IsNullable = prop.IsNullable,
                IsRequired = !prop.IsNullable,
                IsForeignKey = prop.IsForeignKey(),
                IsEnum = prop.ClrType.IsEnum,
                IsList =
                    prop.ClrType.IsArray
                    || prop.ClrType.IsGenericType
                        && typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.ClrType),
                IsDecimal = prop.ClrType == typeof(decimal),
                // 可扩展更多属性
            };
            result.Add(info);
        }
        entityInfo.PropertyInfos = result;
    }

    public void LoadEntityNavigations(EntityInfo entityInfo)
    {
        if (CurrentEntity == null)
        {
            OutputHelper.Error("The currentEntity is null");
            return;
        }
        var result = new List<EntityNavigation>();
        foreach (var nav in CurrentEntity.GetNavigations())
        {
            var navigation = new EntityNavigation
            {
                Name = nav.Name,
                Type =
                    nav.TargetEntityType.ClrType.Name
                    + (
                        nav.TargetEntityType.ClrType.IsGenericType
                        && nav.TargetEntityType.ClrType.GetGenericTypeDefinition()
                            == typeof(Nullable<>)
                            ? "?"
                            : string.Empty
                    ),
                ForeignKey = nav
                    .ForeignKey.Properties.Select(p => p.Name)
                    .Aggregate((current, next) => $"{current}, {next}"),
                IsShadow = nav.IsShadowProperty(),
                IsRequired = nav.ForeignKey.IsRequired,
                IsUnique = nav.ForeignKey.IsUnique,
                IsCollection = nav.IsCollection,
                IsSkipNavigation = nav is ISkipNavigation,
            };

            if (nav.TargetEntityType.ClrType.FullName != null)
            {
                navigation.Summary =
                    _xmlHelper.GetPropertySummary(nav.TargetEntityType.ClrType.FullName, nav.Name)
                    ?? string.Empty;
            }
            result.Add(navigation);
        }
        entityInfo.Navigations = result;
    }
}
