using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Ater.Web.Convention.Middleware;

public class NullableEnumTansfomer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;
        // 拦截 Nullable<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type)!;

            // 强制 schema 引用 underlyingType
            schema.Reference = new OpenApiReference
            {
                Type = ReferenceType.Schema,
                Id = underlyingType.Name,
            };

            // 加上 nullable 标记
            schema.Nullable = true;

            // ⚠️ 清掉冗余定义
            schema.Type = null;
            schema.Enum.Clear();
            schema.Properties.Clear();
        }

        return Task.CompletedTask;
    }
}
