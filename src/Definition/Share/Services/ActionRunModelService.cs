using CodeGenerator;
using CodeGenerator.Models;

namespace Share.Services;

/// <summary>
/// Build and enrich <see cref="ActionRunModel"/> without database operations.
/// </summary>
public class ActionRunModelService(IProjectContext projectContext)
{
    private static readonly string[] DtoMatchFiles =
    [
        "AddDto.cs",
        "UpdateDto.cs",
        "DetailDto.cs",
        "ItemDto.cs",
        "FilterDto.cs",
    ];

    /// <summary>
    /// Create action run model and merge variables by key.
    /// </summary>
    public ActionRunModel Create(
        IEnumerable<Variable>? sourceVariables = null,
        IEnumerable<Variable>? extraVariables = null
    )
    {
        var variables = (sourceVariables ?? [])
            .Concat(extraVariables ?? [])
            .DistinctBy(v => v.Key)
            .ToList();

        return new ActionRunModel { Variables = variables };
    }

    /// <summary>
    /// Apply model info from dto/meta source.
    /// </summary>
    public void ApplyModelInfo(ActionRunModel model, TypeMeta modelInfo)
    {
        model.ModelName = modelInfo.Name;
        model.PropertyInfos = modelInfo.PropertyInfos;
        model.Description = modelInfo.CommentSummary ?? modelInfo.Comment;

        AddStandardModelVariables(model, modelInfo.Name);
    }

    /// <summary>
    /// Load entity info (and dto info if exists) from source file path.
    /// </summary>
    public async Task<bool> TryApplyEntityInfoAsync(ActionRunModel model, string sourceFilePath)
    {
        var entityInfo = (await CodeAnalysisService.GetEntityInfosAsync([sourceFilePath])).FirstOrDefault();
        if (entityInfo == null)
        {
            return false;
        }

        model.ModelName = entityInfo.Name;
        model.Namespace = entityInfo.NamespaceName;
        model.PropertyInfos = entityInfo.PropertyInfos;
        model.Description = entityInfo.Summary;

        AddStandardModelVariables(model, entityInfo.Name);

        var dtoPath = projectContext.GetDtoPath(entityInfo.Name, entityInfo.ModuleName);
        if (!Directory.Exists(dtoPath))
        {
            return true;
        }

        var dtoFiles = Directory
            .GetFiles(dtoPath, "*Dto.cs", SearchOption.AllDirectories)
            .Where(q => DtoMatchFiles.Any(m => Path.GetFileName(q).EndsWith(m)))
            .ToList();

        var dtoInfos = await CodeAnalysisService.GetEntityInfosAsync(dtoFiles);

        model.AddPropertyInfos =
            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("AddDto"))?.PropertyInfos ?? [];

        model.UpdatePropertyInfos =
            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("UpdateDto"))?.PropertyInfos ?? [];

        model.DetailPropertyInfos =
            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("DetailDto"))?.PropertyInfos ?? [];

        model.ItemPropertyInfos =
            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("ItemDto"))?.PropertyInfos ?? [];

        model.FilterPropertyInfos =
            dtoInfos.FirstOrDefault(q => q.Name.EndsWith("FilterDto"))?.PropertyInfos ?? [];

        return true;
    }

    private static void AddStandardModelVariables(ActionRunModel model, string modelName)
    {
        model.Variables.RemoveAll(v => v.Key is "ModelName" or "ModelNameHyphen");
        model.Variables.Add(new Variable { Key = "ModelName", Value = modelName });
        model.Variables.Add(new Variable { Key = "ModelNameHyphen", Value = modelName.ToHyphen() });
    }
}