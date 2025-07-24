using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Nodes;
using Ater.Common.Utils;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AterStudio;

public sealed class EnumOpenApiTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.JsonTypeInfo.Type.IsEnum)
        {
            var name = new JsonArray();
            var enumData = new JsonArray();
            FieldInfo[] fields = context.JsonTypeInfo.Type.GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.Name != "value__")
                {
                    name.Add(JsonValue.Create(f.Name));
                    CustomAttributeData? desAttr = f
                        .CustomAttributes.Where(a => a.AttributeType.Name == "DescriptionAttribute")
                        .FirstOrDefault();

                    if (desAttr != null)
                    {
                        CustomAttributeTypedArgument des =
                            desAttr.ConstructorArguments.FirstOrDefault();
                        if (des.Value != null)
                        {
                            enumData.Add(
                                new JsonObject()
                                {
                                    ["name"] = JsonValue.Create(f.Name),
                                    ["value"] = JsonValue.Create(f.GetRawConstantValue()),
                                    ["description"] = JsonValue.Create(des.Value.ToString()),
                                }
                            );
                        }
                    }
                }
            }
            schema.Extensions.Add("x-enumNames", new JsonNodeExtension(name));
            schema.Extensions.Add("x-enumData", new JsonNodeExtension(enumData));
        }
        else
        {
            PropertyInfo[] properties = context.JsonTypeInfo.Type.GetProperties();

            foreach (var property in schema.Properties)
            {
                PropertyInfo? prop = properties.FirstOrDefault(x =>
                    x.Name.ToCamelCase() == property.Key
                );
                if (prop != null)
                {
                    var isRequired = Attribute.IsDefined(prop, typeof(RequiredAttribute));
                    if (isRequired)
                    {
                        //property.Value.Nullable = false;
                        _ = schema.Required.Add(property.Key);
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}
