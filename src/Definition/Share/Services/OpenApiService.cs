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
                    (string RequestType, string? RequestRefType) = OpenApiHelper.ParseParamTSType(
                        requestBody.Content?.Values.FirstOrDefault()?.Schema
                    );
                    // 关联的类型
                    var model = ModelInfos.FirstOrDefault(m => m.Name == RequestRefType);

                    if (model == null)
                    {
                        apiInfo.RequestInfo = new TypeMeta { Name = RequestType };

                        if (!string.IsNullOrWhiteSpace(RequestType))
                        {
                            apiInfo.RequestInfo.PropertyInfos =
                            [
                                new PropertyInfo
                                {
                                    Name = RequestType,
                                    Type = RequestRefType ?? RequestType,
                                },
                            ];
                        }
                    }
                    else
                    {
                        apiInfo.RequestInfo = model;
                    }
                }
                // 响应类型
                if (responseBody != null)
                {
                    (string ResponseType, string? ResponseRefType) = OpenApiHelper.ParseParamTSType(
                        responseBody.FirstOrDefault().Value?.Content?.FirstOrDefault().Value?.Schema
                    );
                    // 关联的类型
                    var model = ModelInfos.FirstOrDefault(m => m.Name == ResponseRefType);

                    // 返回内容没有对应类型
                    if (model == null)
                    {
                        apiInfo.ResponseInfo = new TypeMeta { Name = ResponseType };
                        if (!string.IsNullOrWhiteSpace(ResponseType))
                        {
                            apiInfo.ResponseInfo.PropertyInfos =
                            [
                                new PropertyInfo
                                {
                                    Name = ResponseType,
                                    Type = ResponseRefType ?? ResponseType,
                                },
                            ];
                        }
                    }
                    else
                    {
                        apiInfo.ResponseInfo = model;
                    }
                    if (ResponseType.EndsWith("[]"))
                    {
                        apiInfo.ResponseInfo.IsList = true;
                    }
                }
                // 请求的参数
                if (requestParameters != null)
                {
                    List<PropertyInfo>? parameters = requestParameters
                        ?.Select(p =>
                        {
                            string? location = p.In?.GetDisplayName();
                            bool? inpath = location?.ToLower()?.Equals("path");
                            (string type, string? _) = OpenApiHelper.ParseParamTSType(p.Schema);
                            return new PropertyInfo
                            {
                                CommentSummary = p.Description,
                                Name = p.Name ?? "",
                                IsRequired = p.Required,
                                Type = type,
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
            string name = schema.Key;
            string? description =
                schema.Value.Description ?? schema.Value.AllOf?.LastOrDefault()?.Description;
            description = description?.Replace("\n", " ") ?? "";
            List<PropertyInfo> props = OpenApiHelper.ParseProperties(schema.Value, true);

            // 处理Required内容
            schema
                .Value.Required?.ToList()
                .ForEach(required =>
                {
                    var prop = props.FirstOrDefault(p => p.Name == required);
                    prop?.IsRequired = true;
                });

            var model = new TypeMeta()
            {
                Name = name,
                PropertyInfos = props,
                Comment = description,
            };
            // 判断是否为枚举类
            var enumNode = schema.Value.Enum;
            var enumExtension = schema.Value.Extensions?.FirstOrDefault(e => e.Key == "x-enumData").Value;
            if (enumNode?.Count > 0 || enumExtension != null)
            {
                model.IsEnum = true;
                model.PropertyInfos = OpenApiHelper.GetEnumProperties(schema.Value);
            }
            models.Add(model);
        }
        return models;
    }
}
