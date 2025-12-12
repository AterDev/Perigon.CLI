namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// TypeScript 客户端生成基类，包含 TS 特有的辅助方法
/// </summary>
public abstract class TypeScriptClientBase(OpenApiDocument openApi) : ClientRequestBase(openApi)
{
    /// <summary>
    /// 根据已解析的 SchemaMetas 生成 TS 模型文件集合。
    /// </summary>
    public override List<GenFileInfo> GenerateModelFiles()
    {
        ModelFiles.Clear();
        EnumModels.Clear();
        if (SchemaMetas.Count == 0) ParseSchemas();
        int enumCount = 0;
        var tags = ApiTags!.Where(t => t.Name != null);
        foreach (var meta in SchemaMetas)
        {
            string content = Formatter.GenerateModel(meta);
            string dir = "models/" + OpenApiHelper.GetNamespaceFirstPart(meta.Namespace).ToHyphen();
            string fileName = meta.Name.ToHyphen() + $".model.ts";

            var file = new GenFileInfo(fileName, content)
            {
                DirName = dir,
                Content = content,
                ModelName = meta.Name,
            };
            ModelFiles.Add(file);
            if (meta.IsEnum == true)
            {
                EnumModels.Add(meta.Name);
                enumCount++;
            }
        }
        return ModelFiles;
    }

    /// <summary>
    /// 获取要导入的依赖
    /// </summary>
    protected List<TypeMeta> GetRefTypes(List<RequestServiceFunction> functions)
    {
        var metaMap = SchemaMetas
            .Where(m => !string.IsNullOrWhiteSpace(m.FullName))
            .GroupBy(m => m.FullName)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        HashSet<string> wanted = [];

        foreach (var f in functions)
        {
            AddIfMatch(f.RequestRefType);
            AddIfMatch(f.ResponseRefType);

            if (f.Params is not null)
            {
                foreach (var p in f.Params)
                {
                    AddIfMatch(p.RefType);
                }
            }

            // 如果响应为泛型，提取其泛型参数的 FullName
            if (!string.IsNullOrWhiteSpace(f.ResponseRefType) && f.ResponseRefType.Contains('`'))
            {
                foreach (var argFullName in ExtractGenericArgumentFullNames(f.ResponseRefType))
                {
                    AddIfMatch(argFullName);
                }
            }
        }

        var result = wanted
            .Where(metaMap.ContainsKey)
            .Select(k => metaMap[k])
            .GroupBy(m => m.FullName)
            .Select(g => g.First())
            .ToList();
        return result;

        void AddIfMatch(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return;
            // 兼容转义的反引号 \u0060 情况
            fullName = fullName.Replace("\u0060", "`");
            if (metaMap.ContainsKey(fullName)) wanted.Add(fullName);
        }

        IEnumerable<string> ExtractGenericArgumentFullNames(string genericFullName)
        {
            int start = genericFullName.IndexOf("[[");
            int end = genericFullName.LastIndexOf("]]");
            if (start < 0 || end <= start) yield break;
            var inner = genericFullName.Substring(start + 2, end - start - 2);
            var args = inner.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var arg in args)
            {
                var argType = arg.Split(',')[0].Trim();
                yield return argType;
            }
        }
    }

    protected string InsertImportModel(TypeMeta typeMeta)
    {
        var dirPath = OpenApiHelper.GetNamespaceFirstPart(typeMeta.Namespace).ToHyphen();
        return $"import {{ {typeMeta.Name} }} from '../models/{dirPath}/{typeMeta.Name.ToHyphen()}.model';{Environment.NewLine}";
    }

    protected FunctionBuildResult BuildFunctionCommon(RequestServiceFunction function, bool addExtOptions)
    {
        string name = function.Name;
        List<FunctionParams>? @params = function.Params;
        string requestType = OpenApiHelper.FormatSchemaKey(function.RequestType);
        string responseType = OpenApiHelper.FormatSchemaKey(function.ResponseType);
        string path = function.Path;

        name = name.Replace(function.Tag + "_", "");
        name = name.ToCamelCase();

        string paramsString = "";
        string paramsComments = "";
        string dataString = "";
        if (@params?.Count > 0)
        {
            paramsString = string.Join(
                ", ",
                @params
                    .OrderByDescending(p => p.IsRequired)
                    .Select(p =>
                        p.IsRequired ? p.Name + ": " + OpenApiHelper.FormatSchemaKey(p.Type)
                        : p.Name + ": " + OpenApiHelper.FormatSchemaKey(p.Type) + " | null"
                    )
                    .ToArray()
            );
            @params.ForEach(p =>
            {
                paramsComments += $" * @param {p.Name} {p.Description ?? OpenApiHelper.FormatSchemaKey(p.Type)}\n";
            });
        }
        if (!string.IsNullOrEmpty(requestType))
        {
            if (@params?.Count > 0)
            {
                paramsString += $", data: {requestType}";
            }
            else
            {
                paramsString = $"data: {requestType}";
            }

            dataString = ", data";
            paramsComments += $" * @param data {requestType}\n";
        }
        if (addExtOptions)
        {
            if (!string.IsNullOrWhiteSpace(paramsComments))
            {
                paramsString += ", ";
            }
            paramsString += "extOptions?: ExtOptions";
        }
        // 泛型占位替换：请求类型、响应类型、参数列表
        //paramsString = ReplaceGenericPlaceholders(paramsString, function);
        requestType = ReplaceGenericPlaceholders(requestType, function);
        responseType = ReplaceGenericPlaceholders(responseType, function);


        string comments =
            $"/**\n * {function.Description ?? name}\n{paramsComments} */";

        List<string?>? paths = @params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
        {
            string origin = $"{{{p}}}";
            path = path.Replace(origin, "$" + origin);
        });
        List<string?>? reqParams = @params
            ?.Where(p => !p.InPath && p.Type != "FormData")
            .Select(p => p.Name)
            ?.ToList();
        if (reqParams != null)
        {
            string queryParams = string.Join(
                "&",
                reqParams.Select(p => $"{p}=${{{p} ?? ''}}").ToArray()
            );
            if (!string.IsNullOrEmpty(queryParams))
            {
                path += "?" + queryParams;
            }
        }
        FunctionParams? file = @params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = ", " + file.Name;
        }

        if (addExtOptions)
        {
            if (string.IsNullOrEmpty(dataString))
            {
                dataString = ", null, extOptions";
            }
            else
            {
                dataString += ", extOptions";
            }
        }

        return new FunctionBuildResult(name, paramsString, comments, dataString, path) { ResponseType = responseType };
    }

    private string ReplaceGenericPlaceholders(string text, RequestServiceFunction function)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        // 支持基于 RequestRefType/ResponseRefType 的泛型替换, 如 PageList<T> -> PageList<EntityDto>
        var replacements = new Dictionary<string, string>();
        ExtractGenericConcrete(function.RequestRefType, function.RequestType);
        ExtractGenericConcrete(function.ResponseRefType, function.ResponseType);
        foreach (var kv in replacements)
        {
            text = text.Replace(kv.Key, kv.Value);
        }
        return text;

        void ExtractGenericConcrete(string? refType, string? tsType)
        {
            if (string.IsNullOrWhiteSpace(refType) || string.IsNullOrWhiteSpace(tsType)) return;
            // 只处理形如 TypeName<Something>
            int lt = tsType.IndexOf('<');
            int gt = tsType.LastIndexOf('>');
            if (lt > 0 && gt > lt)
            {
                var inner = tsType.Substring(lt + 1, gt - lt - 1);
                // 如果 inner 看起来是占位 (T/T1/T2) 则用 refType 的泛型参数解析
                if (inner.All(c => c == 'T' || char.IsDigit(c) || c == ','))
                {
                    // 从 refType 提取真实参数: PageList`1[[Namespace.EntityDto,...]]
                    var realTypes = OpenApiHelper.ExtractAllTypeNames(refType).Skip(1).Select(OpenApiHelper.FormatSchemaKey).ToList();
                    if (realTypes.Count == 1)
                    {
                        replacements[inner] = realTypes[0];
                    }
                    else if (realTypes.Count > 1)
                    {
                        replacements[inner] = string.Join(",", realTypes);
                    }
                }
            }
        }
    }
}
