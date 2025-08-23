using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Ater.Web.Convention.Middleware;

public sealed class EnumOpenApiTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;
        // 移除多余的可空类型的枚举结构
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type)!;

            if (underlyingType.IsEnum)
            {
                schema.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = underlyingType.Name,
                };

                schema.Nullable = true;
                schema.Type = null;
                schema.Enum.Clear();
                schema.Properties.Clear();
            }
        }

        if (type.IsEnum)
        {
            schema.Type = "enum";
            var name = new OpenApiArray();
            var enumData = new OpenApiArray();
            FieldInfo[] fields = context.JsonTypeInfo.Type.GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.Name != "value__")
                {
                    //name.Add(new OpenApiString(f.Name));

                    schema.Enum.Add(new OpenApiString(f.Name));
                    CustomAttributeData? desAttr = f
                        .CustomAttributes.Where(a => a.AttributeType.Name == "DescriptionAttribute")
                        .FirstOrDefault();

                    var openApiObj = new OpenApiObject()
                    {
                        ["name"] = new OpenApiString(f.Name),
                        ["value"] = new OpenApiInteger((int)f.GetRawConstantValue()!),
                    };
                    if (desAttr != null)
                    {
                        CustomAttributeTypedArgument des =
                            desAttr.ConstructorArguments.FirstOrDefault();
                        if (des.Value != null)
                        {
                            openApiObj.Add("description", new OpenApiString(des.Value.ToString()));
                        }
                    }
                    enumData.Add(openApiObj);
                }
            }

            //schema.Extensions.Add("x-enumNames", name);
            schema.Extensions.Add("x-enumData", enumData);
        }
        else
        {
            PropertyInfo[] properties = context.JsonTypeInfo.Type.GetProperties();

            foreach (KeyValuePair<string, OpenApiSchema> property in schema.Properties)
            {
                PropertyInfo? prop = properties.FirstOrDefault(x =>
                    x.Name.ToCamelCase() == property.Key
                );
                if (prop != null)
                {
                    var isRequired = Attribute.IsDefined(prop, typeof(RequiredAttribute));
                    if (isRequired)
                    {
                        property.Value.Nullable = false;
                        _ = schema.Required.Add(property.Key);
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}
