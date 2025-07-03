using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenIddict.Abstractions;

namespace IdentityServer.Components.Pages.ClientApp;

public partial class AddClientApp
{
    private EditContext? editContext;
    private ClientAppAddDto? AddDto;
    private bool formValid = false;

    /// <summary>
    /// application types
    /// </summary>
    private readonly IReadOnlyList<string> _applicationTypes =
    [
        OpenIddictConstants.ApplicationTypes.Web,
        OpenIddictConstants.ApplicationTypes.Native,
    ];

    /// <summary>
    /// client types
    /// </summary>
    private readonly IReadOnlyList<string> _clientTypes =
    [
        OpenIddictConstants.ClientTypes.Public,
        OpenIddictConstants.ClientTypes.Confidential,
    ];

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Inject]
    private ApplicationManager ApplicationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        AddDto ??= new();
        editContext = new(AddDto);
        editContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (editContext is not null)
        {
            formValid = editContext.Validate();
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        if (editContext is not null)
        {
            editContext.OnFieldChanged -= HandleFieldChanged;
        }
    }

    private async Task SaveAsync()
    {
        if (!editContext!.Validate())
        {
            ToastService.ShowError(Lang(Localizer.FormValidFailed));
            return;
        }

        if (AddDto != null)
        {
            var res = await ApplicationManager.CreateAsync(AddDto);
            if (res is not null)
            {
                await Dialog.CloseAsync(res);
            }
            else
            {
                ToastService.ShowError(Lang(Localizer.Add, Localizer.Failed));
            }
        }
    }

    private async Task CancelAsync()
    {
        await Dialog.CancelAsync();
    }
}
