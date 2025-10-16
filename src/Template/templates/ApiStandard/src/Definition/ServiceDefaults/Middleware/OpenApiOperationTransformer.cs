using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ServiceDefaults.Middleware;
/// <summary>
/// Transformer for Microsoft.AspNetCore.OpenApi operations.
/// </summary>
public class OpenApiOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(operation.OperationId))
        {
            var actionDescriptor = context.Description.ActionDescriptor;

            var controller = actionDescriptor.RouteValues.TryGetValue("controller", out var c)
                ? c
                : "UnknownController";
            var action = actionDescriptor.RouteValues.TryGetValue("action", out var a)
                ? a
                : context.Description.RelativePath;

            operation.OperationId = $"{controller}_{action}";
        }
        return Task.CompletedTask;
    }
}
