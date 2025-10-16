using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;


namespace ServiceDefaults.Middleware;

/// <summary>
/// Transformer for Microsoft.AspNetCore.OpenApi Schema
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
        AddEnumExtension(schema, type);
        return Task.CompletedTask;
    }

    private static void AddEnumExtension(OpenApiSchema schema, Type type)
    {
        if (!type.IsEnum)
        {
            return;
        }
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();

        var enumItems = new List<EnumItem>();
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var raw = field.GetRawConstantValue();
            if (raw is null)
            {
                continue;
            }

            var value = Convert.ToInt32(raw);
            string? description = null;
            var desAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (desAttr is not null && !string.IsNullOrWhiteSpace(desAttr.Description))
            {
                description = desAttr.Description;
            }
            enumItems.Add(new EnumItem(field.Name, value, description));
        }

        if (schema.Enum is null || schema.Enum.Count == 0)
        {
            schema.Enum = [];
            foreach (var item in enumItems)
            {
                schema.Enum.Add(JsonValue.Create(item.Value));
            }
        }
        schema.Extensions["x-enumData"] = new EnumDataExtension(enumItems);
    }

    private sealed record EnumItem(string Name, int Value, string? Description);

    /// <summary>
    /// 自定义扩展写出器
    /// </summary>
    private sealed class EnumDataExtension(IReadOnlyList<EnumItem> items) : IOpenApiExtension
    {
        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            WriteInternal(writer);
        }

        private void WriteInternal(IOpenApiWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in items)
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
