using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace AterStudio;

public sealed class EnumOpenApiTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {

        throw new NotImplementedException();
    }
}
