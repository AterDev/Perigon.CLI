using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Ater.Web.Convention.Middleware;

/// <summary>
/// 对微软官方 OpenApi 的特殊处理
/// </summary>
public sealed class OpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;
        HandleNullableEnum(schema, type);
        AddEnumExtension(schema, type);
        // TODO:重复性类型处理
        return Task.CompletedTask;
    }

    private static void HandleNullableEnum(OpenApiSchema schema, Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null && underlyingType.IsEnum)
            {
                schema.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = underlyingType.Name,
                };
                schema.Nullable = true;
                schema.Enum.Clear();
                schema.Properties.Clear();
            }
        }
    }

    private static void AddEnumExtension(OpenApiSchema schema, Type type)
    {
        if (type.IsEnum)
        {
            var enumData = new OpenApiArray();
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.Name != "value__")
                {
                    schema.Enum.Add(new OpenApiString(f.Name));
                    var openApiObj = new OpenApiObject()
                    {
                        ["name"] = new OpenApiString(f.Name),
                        ["value"] = new OpenApiInteger((int)f.GetRawConstantValue()!),
                    };
                    var desAttr = f.CustomAttributes.FirstOrDefault(a =>
                        a.AttributeType == typeof(DescriptionAttribute)
                    );
                    if (desAttr != null)
                    {
                        var des = desAttr.ConstructorArguments.FirstOrDefault();
                        if (des.Value != null)
                        {
                            openApiObj.Add("description", new OpenApiString(des.Value.ToString()));
                        }
                    }
                    enumData.Add(openApiObj);
                }
            }
            schema.Extensions.Add("x-enumData", enumData);
        }
    }
}
