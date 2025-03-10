using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace CodeGenerator.Generate;
/// <summary>
/// generate typescript model file
/// </summary>
public class TSModelGenerate : GenerateBase
{
    public Dictionary<string, string?> ModelDictionary { get; set; } = [];

    public OpenApiDocument OpenApi { get; set; }

    public TSModelGenerate(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        foreach (KeyValuePair<string, OpenApiPathItem> path in openApi.Paths)
        {
            foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
            {
                string? tag = operation.Value.Tags.FirstOrDefault()?.Name;

                OpenApiSchema? requestSchema = operation.Value.RequestBody?.Content.Values.FirstOrDefault()?.Schema;
                OpenApiSchema? responseSchema = operation.Value.Responses.FirstOrDefault().Value
                     ?.Content.FirstOrDefault().Value
                     ?.Schema;
                (string? RequestType, string? requestRefType) = RequestGenerate.GetTypescriptParamType(requestSchema);
                (string? ResponseType, string? responseRefType) = RequestGenerate.GetTypescriptParamType(responseSchema);

                // 存储对应的Tag
                // 请求dto
                if (requestRefType != null && !string.IsNullOrEmpty(requestRefType))
                {
                    _ = ModelDictionary.TryAdd(requestRefType, tag);
                }
                // 返回dto
                if (responseRefType != null && !string.IsNullOrEmpty(responseRefType))
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
    public Dictionary<string, string?>? GetRelationModels(OpenApiSchema? schema, string? tag = "")
    {
        if (schema == null)
        {
            return default;
        }

        Dictionary<string, string?> dic = [];
        // 父类
        if (schema.AllOf != null)
        {
            OpenApiSchema? parent = schema.AllOf.FirstOrDefault();
            if (parent != null)
            {
                if (!dic.ContainsKey(parent.Reference.Id))
                {
                    dic.Add(parent.Reference.Id, null);
                }

            }
        }
        // 属性中的类型
        List<OpenApiSchema> props = schema.Properties.Where(p => p.Value.OneOf != null)
            .Select(s => s.Value).ToList();
        if (props != null)
        {
            foreach (OpenApiSchema? prop in props)
            {
                if (prop.OneOf.Any())
                {
                    if (!dic.ContainsKey(prop.OneOf.FirstOrDefault()!.Reference.Id))
                    {
                        dic.Add(prop.OneOf.FirstOrDefault()!.Reference.Id, tag);
                    }

                }
            }
        }
        // 数组
        List<OpenApiSchema> arr = schema.Properties.Where(p => p.Value.Type == "array")
            .Select(s => s.Value).ToList();
        if (arr != null)
        {
            foreach (OpenApiSchema? item in arr)
            {
                if (item.Items.OneOf.Any())
                {
                    if (!dic.ContainsKey(item.Items.OneOf.FirstOrDefault()!.Reference.Id))
                    {
                        dic.Add(item.Items.OneOf.FirstOrDefault()!.Reference.Id, tag);
                    }
                }
            }
        }

        return dic;
    }
    /// <summary>
    /// 生成ts interface
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GenerateInterfaceFile(string schemaKey, OpenApiSchema schema)
    {
        // 文件名及内容
        string fileName = schemaKey.ToHyphen() + ".model.ts";
        string tsContent;
        string? path = GetDirName(schemaKey)?.ToHyphen();
        if (schema.Enum.Count > 0)
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
    /// 根据类型schema，找到对应所属的目录
    /// </summary>
    /// <param name="searchKey"></param>
    /// <returns></returns>
    private string? GetDirName(string searchKey)
    {
        string? dirName = ModelDictionary.Where(m => m.Key.StartsWith(searchKey))
            .Select(m => m.Value)
            .FirstOrDefault();
        return dirName;
    }

    /// <summary>
    /// 将 Schemas 转换成 ts 接口
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="name"></param>
    /// <param name="onlyProps"></param>
    /// <returns></returns>
    public string ToInterfaceString(OpenApiSchema schema, string name = "", bool onlyProps = false)
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        string extendString = "";
        string importString = "";// 需要导入的关联接口
        string relatePath = "../../";

        // 不在控制器中的类型，则在根目录生成，相对目录也从根目录开始
        if (string.IsNullOrEmpty(GetDirName(name)))
        {
            relatePath = "../";
        }

        if (schema.AllOf.Count > 0)
        {
            string? extend = schema.AllOf.First()?.Reference?.Id;
            if (!string.IsNullOrEmpty(extend))
            {
                extendString = "extends " + extend + " ";
                // 如果是自引用，不需要导入
                if (extend != name)
                {
                    string? dirName = GetDirName(name);
                    dirName = dirName.NotEmpty() ? dirName!.ToHyphen() + "/" : "";
                    importString += @$"import {{ {extend} }} from '{relatePath}models/{extend.ToHyphen()}.model';"
                        + Environment.NewLine;
                }
            }
        }
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment = @$"/**
 * {schema.Description}
 */
";
        }
        List<TsProperty> props = GetTsProperties(schema);
        props.ForEach(p =>
        {
            propertyString += p.ToProperty();
        });
        // 去重
        var importsProps = props.Where(p => !string.IsNullOrEmpty(p.Reference))
            .GroupBy(p => p.Reference)
            .Select(g => new
            {
                g.First().IsEnum,
                g.First().Reference
            })
            .ToList();
        importsProps.ForEach(ip =>
        {
            // 引用的导入，自引用不需要导入
            if (ip.Reference != name)
            {
                string? dirName = GetDirName(ip.Reference);
                dirName = dirName.NotEmpty() ? dirName!.ToHyphen() + "/" : "";
                if (ip.IsEnum)
                {
                    dirName = "enum/";
                }

                importString += @$"import {{ {ip.Reference} }} from '{relatePath}{dirName}models/{ip.Reference.ToHyphen()}.model';"
                + Environment.NewLine;
            }
        });

        res = @$"{importString}{comment}export interface {name} {extendString}{{
{propertyString}
}}
";
        return res;
    }

    /// <summary>
    /// 生成enum
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ToEnumString(OpenApiSchema schema, string name = "")
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment = @$"/**
 * {schema.Description}
 */
";
        }
        // 先判断x-enumData
        KeyValuePair<string, IOpenApiExtension> enumData = schema.Extensions
            .Where(e => e.Key == "x-enumData")
            .FirstOrDefault();

        KeyValuePair<string, IOpenApiExtension> enumNames = schema.Extensions
            .Where(e => e.Key == "x-enumNames")
            .FirstOrDefault();


        if (enumData.Value is OpenApiArray values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var obj = (OpenApiObject)values[i]!;
                string enumName = ((OpenApiString)obj["name"]).Value ?? "";
                int enumValue = ((OpenApiInteger)obj["value"]).Value;
                string enumDesc = ((OpenApiString)obj["description"]).Value ?? "";
                propertyString += $"""  
                      /** {enumDesc} */
                      {enumName} = {enumValue},

                    """;
            }
        }

        res = @$"{comment}export enum {name} {{
{propertyString}
}}
";
        return res;
    }

    /// <summary>
    /// 获取所有属性
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static List<TsProperty> GetTsProperties(OpenApiSchema schema)
    {
        List<TsProperty> tsProperties = [];
        // 继承的需要递归 从AllOf中获取属性
        if (schema.AllOf.Count > 1)
        {
            // 自己的属性在1中
            tsProperties.AddRange(GetTsProperties(schema.AllOf[1]));
        }

        if (schema.Properties.Count > 0)
        {
            // 泛型处理
            foreach (KeyValuePair<string, OpenApiSchema> prop in schema.Properties)
            {
                string type = OpenApiHelper.ConvertToTypescriptType(prop.Value);
                string propComments = "";
                string name = prop.Key;

                if (!string.IsNullOrEmpty(prop.Value.Description))
                {
                    propComments = @$"  /**
   * {prop.Value.Description}
   */
";
                }
                TsProperty property = new()
                {
                    Comments = propComments,
                    IsNullable = prop.Value.Nullable,
                    Name = name,
                    Type = type
                };
                // 是否是关联属性
                OpenApiSchema? refType = prop.Value.OneOf?.FirstOrDefault();
                // 列表中的类型
                if (prop.Value.Items?.Reference != null)
                {
                    refType = prop.Value.Items;
                }

                if (prop.Value.Items?.OneOf.Count > 0)
                {
                    refType = prop.Value.Items.OneOf.FirstOrDefault();
                }

                if (refType?.Reference != null)
                {
                    property.Reference = refType.Reference.Id;
                }

                if (prop.Value.Reference != null)
                {
                    property.Reference = prop.Value.Reference.Id;
                }

                if (prop.Value.Enum.Any() ||
                    (refType != null && refType.Enum.Any()))
                {
                    property.IsEnum = true;
                }

                // 可空处理
                tsProperties.Add(property);
            }
        }
        // 重写的属性去重
        List<TsProperty?> res = tsProperties.GroupBy(p => p.Name)
            .Select(s => s.FirstOrDefault()).ToList();
        return res!;
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
        // 引用的类型可空
        string name = Name + (IsNullable ? "?: " : ": ");
        if (!string.IsNullOrEmpty(Reference))
        {
            name = Name + "?: ";
        }

        return $"{Comments}  {name}{Type};" + Environment.NewLine;
    }
}
