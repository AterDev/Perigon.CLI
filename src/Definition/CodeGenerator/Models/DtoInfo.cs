using Entity.StudioMod;

namespace CodeGenerator.Models;

/// <summary>
/// DTO 
/// </summary>
public class DtoInfo
{
    public required string Name { get; set; }
    public string? BaseType { get; set; }
    public List<PropertyInfo> Properties { get; set; } = [];
    public string? Tag { get; set; }
    public string? EntityNamespace { get; set; }
    public string? Comment { get; set; }
    /// <summary>
    /// 原实体含命名空间完整路径
    /// </summary>
    public required string EntityFullName { get; set; }

    /// <summary>
    /// dto class content
    /// </summary>
    /// <param name="nsp"></param>
    /// <param name="entityName"></param>
    /// <param name="isInput"></param>
    /// <returns></returns>
    public string ToDtoContent(string nsp, string entityName = "", bool isInput = false)
    {
        string[] props = Properties?.Select(p => p.ToCsharpLine(isInput)).ToArray()
            ?? [];
        string propStrings = string.Join(string.Empty, props);

        // 对region进行处理
        int regionCount = propStrings.Split("#region").Length - 1;
        int endRegionCount = propStrings.Split("#endregion").Length - 1;
        if (endRegionCount < regionCount)
        {
            propStrings += Environment.NewLine + "\t#endregion";
        }

        string baseType = string.IsNullOrEmpty(BaseType) ? "" : " : " + BaseType;
        string tpl = $$"""
            using {{EntityNamespace}};
            namespace {{nsp}}.{{ConstVal.ModelsDir}}.{{entityName}}Dtos;
            {{Comment}}
            /// <see cref="{{EntityFullName}}"/>
            public class {{Name}}{{baseType}}
            {
            {{propStrings}}    
            }

            """;
        return tpl;
    }

    public ModelInfo ToEntityInfo(ModelInfo entityInfo)
    {
        var res = new ModelInfo()
        {
            Name = Name,
            NamespaceName = EntityNamespace ?? "",
            Md5Hash = HashCrypto.Md5Hash(EntityNamespace + Name),
            FilePath = "dto",
            ModuleName = entityInfo.ModuleName,
            ProjectId = entityInfo.ProjectId,
            Comment = Comment,
            PropertyInfos = Properties.MapTo<List<PropertyInfo>, List<ModelProperty>>()
        };
        res.PropertyInfos.ForEach(p =>
        {
            p.Id = Guid.NewGuid();
            p.ModelInfoId = res.Id;
        });
        return res;
    }
}
