using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components.Components.Tooltip;
using Microsoft.JSInterop;
using System.Text.Json;

namespace IdentityServer.Components.Pages;

public class PageBase : ComponentBase
{
    [Inject]
    protected Localizer Localizer { get; set; } = default!;

    [Inject]
    protected IToastService ToastService { get; set; } = default!;

    [Inject]
    protected IDialogService DialogService { get; set; } = default!;

    [Inject]
    protected IMessageService MessageService { get; set; } = default!;

    [Inject]
    protected ITooltipService TooltipService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    public JsonSerializerOptions IndentedJsonOptions { get; } =
        new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// 用于展示多个语言键对应的文本
    /// </summary>
    /// <param name="key1"></param>
    /// <param name="key2"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public string Lang(string key1, string key2, string? separator = "")
    {
        var value1 = Localizer.Get(key1);
        var value2 = Localizer.Get(key2);
        return string.IsNullOrEmpty(separator)
            ? $"{value1}{value2}"
            : $"{value1}{separator}{value2}";
    }

    public string Lang(string key)
    {
        return Localizer.Get(key);
    }

    public string ToJson(object obj)
    {
        return JsonSerializer.Serialize(obj, IndentedJsonOptions);
    }

    protected async Task PreventEnterSubmit(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await JS.InvokeVoidAsync("preventEnterSubmit", e);
        }
    }

    public async Task CopyToClipboardAsync(string text)
    {
        await JS.InvokeVoidAsync("copyTextToClipboard", text);
        ToastService.ShowSuccess(Lang(LanguageKey.Copy, LanguageKey.Success, " "));
    }
}
