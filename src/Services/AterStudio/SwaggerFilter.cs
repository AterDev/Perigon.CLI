using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace AterStudio;

public class SwaggerFilter
{
}

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            OpenApiArray enumData = [];
            var fields = context.Type.GetFields();
            int i = 0;
            foreach (var f in fields)
            {
                if (f.Name != "value__")
                {
                    var description = string.Empty;
                    var desAttr = f.CustomAttributes.Where(a => a.AttributeType.Name == "DescriptionAttribute").FirstOrDefault();
                    desAttr ??= f.CustomAttributes.Where(a => a.AttributeType.Name == "DisplayAttribute").FirstOrDefault();
                    if (desAttr != null)
                    {
                        var des = desAttr.ConstructorArguments.FirstOrDefault();
                        if (des.Value != null)
                        {
                            description = des.Value.ToString();
                        }
                    }
                    var value = f.GetRawConstantValue();
                    var intValue = value == null ? i : (int)value;
                    enumData.Add(new OpenApiObject()
                    {
                        ["name"] = new OpenApiString(f.Name),
                        ["value"] = new OpenApiInteger(intValue),
                        ["description"] = new OpenApiString(description)
                    });
                    i++;
                }
            }
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
