using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace IdentityServer.Components.Pages;

[Authorize]
public class PageBase : ComponentBase
{
    [Inject]
    protected Localizer Localizer { get; set; } = default!;

    protected void ShowMessage(string message)
    {
        // 实现通用消息显示逻辑
    }
}