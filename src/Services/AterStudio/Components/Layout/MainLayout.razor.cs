using Microsoft.JSInterop;

namespace AterStudio.Components.Layout;

public partial class MainLayout : IDisposable
{
    public DesignThemeModes Mode { get; set; } = DesignThemeModes.System;
    private bool isDarkMode = false;
    private IDialogReference? _dialog;

    protected override void OnInitialized()
    {
        MessageService.OnMessageItemsUpdated += UpdateCount;
    }

    private async Task OpenNotificationCenterAsync()
    {
        _dialog = await DialogService.ShowPanelAsync<NotificationCenterPanel>(
            new DialogParameters<GlobalState>()
            {
                Alignment = HorizontalAlignment.Right,
                Title = $"Notifications",
                PrimaryAction = null,
                SecondaryAction = null,
                ShowDismiss = true,
                Width = "400px",
            }
        );
        DialogResult result = await _dialog.Result;
        HandlePanel(result);
    }

    private void UpdateCount()
    {
        InvokeAsync(StateHasChanged);
    }

    private static void HandlePanel(DialogResult result)
    {
        if (result.Cancelled)
        {
            return;
        }

        if (result.Data is not null)
        {
            return;
        }
    }

    public void Dispose()
    {
        MessageService.OnMessageItemsUpdated -= UpdateCount;
    }

    private void ThemeChanged(LuminanceChangedEventArgs e)
    {
        isDarkMode = e.IsDark;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var themeMode = await JS.InvokeAsync<string>("getCurrentThemeMode");
            isDarkMode = themeMode == "dark";
            StateHasChanged();
        }
    }
}
