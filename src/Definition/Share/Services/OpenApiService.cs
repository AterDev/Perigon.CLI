using CodeGenerator.Helper;
using Microsoft.OpenApi;

namespace Share.Services;

/// <summary>
/// openapi 解析帮助类
/// </summary>
public class OpenApiService
{
    public OpenApiDocument OpenApi { get; set; }

    /// <summary>
    /// 接口信息
    /// </summary>
    public List<RestApiGroup> RestApiGroups { get; set; }

    /// <summary>
    /// 所有请求及返回类型信息
    /// </summary>
    public List<TypeMeta> ModelInfos { get; set; }

    /// <summary>
    /// tag信息
    /// </summary>
    public List<ApiDocTag> OpenApiTags { get; set; } = [];

    public OpenApiService(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        OpenApiTags =
            openApi
                .Tags?.Select(s => new ApiDocTag
                {
                    Name = s.Name ?? "",
                    Description = s.Description,
                })
                .ToList() ?? [];
        ModelInfos = ParseModels();
        RestApiGroups = GetRestApiGroups();
    }

    /// <summary>
    /// 接口信息
    /// </summary>
    /// <returns></returns>
    public List<RestApiGroup> GetRestApiGroups()
    {
        List<RestApiInfo> apiInfos = [];
        foreach (var path in OpenApi.Paths)
        {
            if (path.Value.Operations == null || path.Value.Operations.Count == 0)
            {
                continue;
            }
            foreach (var operation in path.Value.Operations)
            {
                RestApiInfo apiInfo = new()
                {
                    Summary = operation.Value?.Summary,
                    HttpMethod = operation.Key,
                    OperationId = operation.Value?.OperationId ?? (operation.Key + path.Key),
                    Router = path.Key,
                    Tag = operation.Value?.Tags?.FirstOrDefault()?.Name,
                };

                // 处理请求内容
                var requestBody = operation.Value?.RequestBody;
                var requestParameters = operation.Value?.Parameters;
                var responseBody = operation.Value?.Responses;

                // 请求类型
                if (requestBody != null)
                {
                    var bodySchema = requestBody.Content?.Values.FirstOrDefault()?.Schema;
                    if (bodySchema != null)
                    {
                        if (bodySchema is OpenApiSchemaReference bodyRef && bodyRef.Reference.Id is not null)
                        {
                            apiInfo.RequestInfo = ModelInfos.FirstOrDefault(m => m.Name == bodyRef.Reference.Id)
                                ?? OpenApiHelper.ParseSchemaToTypeMeta(bodyRef.Reference.Id, bodySchema);
                        }
                        else
                        {
                            apiInfo.RequestInfo = OpenApiHelper.ParseSchemaToTypeMeta("Request", bodySchema);
                        }
                    }
                }
                // 响应类型
                if (responseBody != null)
                {
                    var firstResp = responseBody.FirstOrDefault().Value?.Content?.FirstOrDefault().Value?.Schema;
                    if (firstResp != null)
                    {
                        if (firstResp is OpenApiSchemaReference respRef && respRef.Reference.Id is not null)
                        {
                            apiInfo.ResponseInfo = ModelInfos.FirstOrDefault(m => m.Name == respRef.Reference.Id)
                                ?? OpenApiHelper.ParseSchemaToTypeMeta(respRef.Reference.Id, firstResp);
                        }
                        else
                        {
                            apiInfo.ResponseInfo = OpenApiHelper.ParseSchemaToTypeMeta("Response", firstResp);
                        }
                    }
                }
                // 请求的参数
                if (requestParameters != null)
                {
                    var parameters = requestParameters
                        .Select(p =>
                        {
                            var schema = p.Schema;
                            string type = OpenApiHelper.MapToCSharpType(schema);
                            if (schema is OpenApiSchemaReference r && r.Reference.Id is not null)
                            {
                                type = OpenApiHelper.FormatSchemaKey(r.Reference.Id);
                            }
                            bool isNullable = !p.Required;
                            if (schema?.Type.HasValue == true && schema.Type.Value.HasFlag(JsonSchemaType.Null))
                            {
                                isNullable = true;
                            }
                            return new PropertyInfo
                            {
                                Name = p.Name ?? string.Empty,
                                Type = type,
                                CommentSummary = p.Description,
                                IsRequired = p.Required,
                                IsNullable = isNullable,
                            };
                        })
                        .ToList();
                    apiInfo.QueryParameters = parameters;
                }
                apiInfos.Add(apiInfo);
            }
        }
        List<RestApiGroup> apiGroups = [];
        OpenApiTags.ForEach(tag =>
        {
            RestApiGroup group = new()
            {
                Name = tag.Name,
                Description = tag.Description,
                ApiInfos = apiInfos.Where(a => a.Tag == tag.Name).ToList(),
            };

            apiGroups.Add(group);
        });
        // tag不在OpenApiTags中的api infos
        List<string> tags = OpenApiTags.Select(t => t.Name).ToList();
        List<RestApiInfo> noTagApisInfo = apiInfos
            .Where(a => a.Tag != null && !tags.Contains(a.Tag))
            .ToList();

        if (noTagApisInfo.Count != 0)
        {
            RestApiGroup group = new()
            {
                Name = "No Tags",
                Description = "无tag分组接口",
                ApiInfos = noTagApisInfo,
            };
            apiGroups.Add(group);
        }
        return apiGroups;
    }

    /// <summary>
    /// 解析模型
    /// </summary>
    /// <returns></returns>
    public List<TypeMeta> ParseModels()
    {
        List<TypeMeta> models = [];
        if (OpenApi.Components?.Schemas == null)
        {
            return models;
        }
        foreach (var schema in OpenApi.Components.Schemas)
        {
            var modelMeta = OpenApiHelper.ParseSchemaToTypeMeta(schema.Key, schema.Value);
            // 标准化 comment 内容
            if (!string.IsNullOrWhiteSpace(modelMeta.Comment))
            {
                modelMeta.Comment = modelMeta.Comment.Replace("\n", " ");
            }
            models.Add(modelMeta);
        }
        return models;
    }

    private static string firstType(IOpenApiSchema? schema)
    {
        return schema == null ? "object" : OpenApiHelper.FormatSchemaKey(schema.Type?.ToString() ?? "object");
    }
}
