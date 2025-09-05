using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

public class ListToolsHandler
{
    public ValueTask<ListToolsResult> Handle(
        RequestContext<ListToolsRequestParams> request,
        CancellationToken cancellationToken
    )
    {
        return default;
    }
}
