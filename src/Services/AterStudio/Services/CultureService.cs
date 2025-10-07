using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace AterStudio.Services;

public class CultureService
{
    private readonly NavigationManager _navigationManager;
    
    public event Action? CultureChanged;

    private readonly string[] SupportedCultures = { "zh-CN", "en-US" };
    
    public CultureService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
    
    public string CurrentCulture => CultureInfo.CurrentUICulture.Name;
    
    public bool IsChineseCulture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase);
    
    public string GetCurrentLanguageDisplayName()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper() switch
        {
            "ZH" => "中文",
            "EN" => "English", 
            _ => "中文"
        };
    }
    
    public string GetNextLanguage()
    {
        var currentIndex = Array.IndexOf(SupportedCultures, CurrentCulture);
        var nextIndex = (currentIndex + 1) % SupportedCultures.Length;
        return SupportedCultures[nextIndex];
    }
    
    public string GetNextLanguageDisplayName()
    {
        var nextCulture = GetNextLanguage();
        return nextCulture switch
        {
            "zh-CN" => "中文",
            "en-US" => "English",
            _ => "中文"
        };
    }
    
    public void ToggleCulture()
    {
        var nextCulture = GetNextLanguage();
        SetCulture(nextCulture);
    }
    
    public void SetCulture(string culture)
    {
        if (Array.IndexOf(SupportedCultures, culture) == -1)
            return;

        var uri = new Uri(_navigationManager.Uri)
            .GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
        var query = $"?culture={Uri.EscapeDataString(culture)}&redirectUri={Uri.EscapeDataString(uri)}";

        // 导航到 Culture 控制器设置文化并重定向回当前页面
        _navigationManager.NavigateTo($"/Culture/SetCulture{query}", forceLoad: true);
    }
}