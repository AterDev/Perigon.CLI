using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Http.API.Middleware;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var name = new OpenApiArray();
            var enumData = new OpenApiArray();
            FieldInfo[] fields = context.Type.GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.Name != "value__")
                {
                    name.Add(new OpenApiString(f.Name));
                    CustomAttributeData? desAttr = f.CustomAttributes.Where(a => a.AttributeType.Name == "DescriptionAttribute").FirstOrDefault();

                    if (desAttr != null)
                    {
                        CustomAttributeTypedArgument des = desAttr.ConstructorArguments.FirstOrDefault();
                        if (des.Value != null)
                        {
                            enumData.Add(new OpenApiObject()
                            {
                                ["name"] = new OpenApiString(f.Name),
                                ["value"] = new OpenApiInteger((int)f.GetRawConstantValue()!),
                                ["description"] = new OpenApiString(des.Value.ToString())
                            });
                        }
                    }
                }
            }
            model.Extensions.Add("x-enumNames", name);
            model.Extensions.Add("x-enumData", enumData);
        }
        else
        {
            PropertyInfo[] properties = context.Type.GetProperties();

            foreach (KeyValuePair<string, OpenApiSchema> property in model.Properties)
            {
                PropertyInfo? prop = properties.FirstOrDefault(x => x.Name.ToCamelCase() == property.Key);
                if (prop != null)
                {
                    var isRequired = Attribute.IsDefined(prop, typeof(RequiredAttribute));
                    if (isRequired)
                    {
                        property.Value.Nullable = false;
                        _ = model.Required.Add(property.Key);
                    }
                }
            }
        }
    }
}