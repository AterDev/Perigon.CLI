using Entity;
using CodeGenerator.Models;
using CodeGenerator.Helper;

namespace CodeGenerator.Generate;

/// <summary>
/// generate typescript model file
/// </summary>
public class TSModelGenerate : GenerateBase
{
    public Dictionary<string, string?> ModelDictionary { get; set; } = [];

    public OpenApiDocument OpenApi { get; set; }

    private readonly ITypeNameFormatter _typeFormatter = new TypeScriptTypeNameFormatter();

    public TSModelGenerate(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        foreach (var path in openApi.Paths)
        {
            if (path.Value.Operations == null || path.Value.Operations.Count == 0)
            {
                continue;
            }
            foreach (var operation in path.Value.Operations)
            {
                string? tag = operation.Value?.Tags?.FirstOrDefault()?.Name;

                var requestSchema = operation
                    .Value?.RequestBody?.Content?.Values.FirstOrDefault()
                    ?.Schema;
                var responseSchema = operation
                    .Value?.Responses?.FirstOrDefault()
                    .Value?.Content?.FirstOrDefault()
                    .Value?.Schema;
                var requestRefType = OpenApiHelper.GetRootRef(requestSchema);
                var responseRefType = OpenApiHelper.GetRootRef(responseSchema);
                if (!string.IsNullOrWhiteSpace(requestRefType))
                {
                    _ = ModelDictionary.TryAdd(requestRefType, tag);
                }
                if (!string.IsNullOrWhiteSpace(responseRefType))
                {
                    _ = ModelDictionary.TryAdd(responseRefType, tag);
                }

                Dictionary<string, string?>? relationModels = GetRelationModels(requestSchema, tag);
                if (relationModels != null)
                {
                    foreach (KeyValuePair<string, string?> item in relationModels)
                    {
                        _ = ModelDictionary.TryAdd(item.Key, item.Value);
                    }
                }
                relationModels = GetRelationModels(responseSchema, tag);
                if (relationModels != null)
                {
                    foreach (KeyValuePair<string, string?> item in relationModels)
                    {
                        _ = ModelDictionary.TryAdd(item.Key, item.Value);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取相关联的模型
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string?>? GetRelationModels(
        IOpenApiSchema? schema,
        string? tag = ""
    )
    {
        if (schema == null)
        {
            return default;
        }

        Dictionary<string, string?> dic = [];
        // 父类
        if (schema.AllOf != null)
        {
            var parent = schema.AllOf.FirstOrDefault();
            if (parent is not null and OpenApiSchemaReference reference)
            {
                if (reference.Reference.Id != null)
                {
                    if (!dic.ContainsKey(reference.Reference.Id))
                    {
                        dic.Add(reference.Reference.Id, null);
                    }
                }
            }
        }
        // 属性中的类型
        var props = schema
            .Properties?.Where(p => p.Value.OneOf != null)
            .Select(s => s.Value)
            .ToList();
        if (props != null)
        {
            foreach (var prop in props)
            {
                if (
                    prop.OneOf?.Count > 0
                    && prop.OneOf.FirstOrDefault() is OpenApiSchemaReference reference
                )
                {
                    var refId = reference.Reference.Id ?? "";
                    dic.TryAdd(refId, tag);
                }
            }
        }
        // 数组
        var arr = schema
            .Properties?.Where(p => p.Value.Type == JsonSchemaType.Array)
            .Select(s => s.Value)
            .ToList();
        if (arr != null)
        {
            foreach (IOpenApiSchema? item in arr)
            {
                if (
                    item.Items?.OneOf != null
                    && item.Items.OneOf.Count > 0
                    && item.OneOf?.FirstOrDefault() is OpenApiSchemaReference reference
                )
                {
                    var refId = reference.Reference.Id ?? "";
                    dic.TryAdd(refId, tag);
                }
            }
        }
        return dic;
    }

    /// <summary>
    /// 生成ts interface
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GenerateInterfaceFile(string schemaKey, IOpenApiSchema schema)
    {
        var formatName = OpenApiHelper.FormatSchemaKey(schemaKey);
        // 文件名及内容
        string fileName = formatName.ToHyphen() + ".model.ts";
        string tsContent;
        string? path = "models";
        if (schema.Enum?.Count > 0 || (schema.Extensions != null && schema.Extensions.ContainsKey("x-enumData")))
        {
            tsContent = ToEnumString(schema, schemaKey);
            path = "enum";
        }
        else
        {
            tsContent = ToInterfaceString(schema, schemaKey);
        }
        GenFileInfo file = new(fileName, tsContent)
        {
            DirName = path ?? "",
            FullName = path ?? "",
            Content = tsContent,
            ModelName = schemaKey,
        };

        return file;
    }

    /// <summary>
    /// 将 Schemas 转换成 ts 接口
    /// </summary>
    /// <param schemaKey="schema"></param>
    /// <param schemaKey="schemaKey"></param>
    /// <param schemaKey="onlyProps"></param>
    /// <returns></returns>
    public string ToInterfaceString(IOpenApiSchema schema, string schemaKey = "", bool onlyProps = false)
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        string extendString = "";
        string importString = ""; // 需要导入的关联接口

        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment =
                @$"/**
 * {schema.Description}
 */
";
        }
        var props = OpenApiHelper.ParseProperties(schema);
        if (props.Count == 0)
        {
            return string.Empty;
        }

        var tsProps = new List<TsProperty>();
        foreach (var p in props)
        {
            // 使用 TypeScript 格式化器
            var formattedType = _typeFormatter.Format(p.Type ?? "any", p.IsEnum, p.IsList, p.IsNullable);
            var tsProperty = new TsProperty
            {
                Type = formattedType,
                Name = p.Name,
                IsEnum = p.IsEnum,
                Reference = p.NavigationName ?? string.Empty,
                IsNullable = p.IsNullable,
                Comments = $"/** {p.CommentSummary ?? p.Name} */",
            };
            // 特殊处理(openapi $ref没有可空说明)
            if (
                schemaKey.EndsWith(ConstVal.FilterDto, StringComparison.OrdinalIgnoreCase)
                || schemaKey.EndsWith(ConstVal.UpdateDto, StringComparison.OrdinalIgnoreCase)
            )
            {
                tsProperty.IsNullable = true;
            }
            tsProps.Add(tsProperty);
            propertyString += tsProperty.ToProperty();
        }

        // 去重
        var importsProps = tsProps
            .Where(p => !string.IsNullOrEmpty(p.Reference))
            .DistinctBy(p => p.Reference)
            .ToList();
        importsProps.ForEach(ip =>
        {
            // 引用的导入，自引用不需要导入
            var refType = OpenApiHelper.FormatSchemaKey(ip.Reference);
            if (ip.Reference != schemaKey)
            {
                string dirName = "";
                string relatePath = "./";
                if (ip.IsEnum)
                {
                    relatePath = "../";
                    dirName = "enum/";
                }

                importString +=
                    @$"import {{ {refType} }} from '{relatePath}{dirName}{refType.ToHyphen()}.model';"
                    + Environment.NewLine;
            }
        });

        res =
            @$"{importString}{comment}export interface {OpenApiHelper.FormatSchemaKey(schemaKey)} {extendString}{{
{propertyString}
}}
";
        return res;
    }

    /// <summary>
    /// 生成enum
    /// </summary>
    /// <param schemaKey="schema"></param>
    /// <param schemaKey="schemaKey"></param>
    /// <returns></returns>
    public static string ToEnumString(IOpenApiSchema schema, string schemaKey = "")
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        schemaKey = OpenApiHelper.FormatSchemaKey(schemaKey);
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment =
                @$"/**
 * {schema.Description}
 */
";
        }
        // 先判断x-enumData
        var enumData = schema.Extensions?.Where(e => e.Key == "x-enumData").FirstOrDefault();

        if (enumData == null)
        {
            return $"{comment}export enum {schemaKey} {{}}";
        }

        var enumProps = OpenApiHelper.GetEnumProperties(schema);
        foreach (var item in enumProps)
        {
            propertyString += $"""
                  /** {item.CommentSummary} */
                  {item.Name} = {item.DefaultValue},

                """;
        }

        res =
            @$"{comment}export enum {schemaKey} {{
{propertyString}
}}
";
        return res;
    }
}

public class TsProperty
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string Reference { get; set; } = string.Empty;
    public bool IsEnum { get; set; }
    public bool IsNullable { get; set; }
    public string? Comments { get; set; }

    public string ToProperty()
    {
        string name = Name + (IsNullable ? "?: " : ": ");
        string renderType = Type ?? "any";
        // 已包含 null 则不重复添加
        if (IsNullable && !renderType.Contains("| null"))
        {
            renderType += " | null";
        }
        return $"""
              {Comments}
              {name}{renderType};

            """;
    }
}
