using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ServiceDefaults.Middleware;

/// <summary>
/// this is for swashbuckle to custom enum
/// </summary>
public class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumItems = new List<EnumItem>();
            foreach (
                FieldInfo f in context.Type.GetFields(BindingFlags.Public | BindingFlags.Static)
            )
            {
                if (f.Name == "value__")
                {
                    continue;
                }
                var raw = f.GetRawConstantValue();
                if (raw is null)
                {
                    continue;
                }
                int value = Convert.ToInt32(raw);
                string? description = null;
                var desAttr = f.CustomAttributes.FirstOrDefault(a =>
                    a.AttributeType.Name == "DescriptionAttribute"
                );
                if (desAttr != null)
                {
                    var arg = desAttr.ConstructorArguments.FirstOrDefault();
                    if (arg.Value != null)
                    {
                        description = arg.Value.ToString();
                    }
                }
                // Fallback description to name if still null
                description ??= f.Name;
                enumItems.Add(new EnumItem(f.Name, value, description));
            }
            model.Extensions?["x-enumData"] = new EnumDataExtension(enumItems);
            if (model.Extensions is null && model is OpenApiSchema concrete)
            {
                concrete.Extensions = new Dictionary<string, IOpenApiExtension>();
            }
            model.Extensions?["x-enumData"] = new EnumDataExtension(enumItems);
        }
        else
        {
            if (model.Properties is null || model.Properties.Count == 0)
            {
                return;
            }

            PropertyInfo[] properties = context.Type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            );
            foreach (var kvp in model.Properties)
            {
                if (kvp.Value is null)
                {
                    continue;
                }
                PropertyInfo? prop = properties.FirstOrDefault(x =>
                    x.Name.ToCamelCase() == kvp.Key
                );
                if (prop is null)
                {
                    continue;
                }
                bool isRequired = Attribute.IsDefined(prop, typeof(RequiredAttribute));
                if (isRequired && model.Required is not null && !model.Required.Contains(kvp.Key))
                {
                    model.Required.Add(kvp.Key);
                }
            }
        }
    }

    private sealed record EnumItem(string Name, int Value, string? Description);

    private sealed class EnumDataExtension : IOpenApiExtension
    {
        private readonly IReadOnlyList<EnumItem> _items;

        public EnumDataExtension(IReadOnlyList<EnumItem> items)
        {
            _items = items;
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartArray();
            foreach (var item in _items)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(item.Name);
                writer.WritePropertyName("value");
                writer.WriteValue(item.Value);
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    writer.WritePropertyName("description");
                    writer.WriteValue(item.Description);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
