using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AterStudio.Components.Pages;

/// <summary>
/// 存储服务
/// </summary>
public class StorageService
{
    private Dictionary<string, object?> Parameters { get; set; } = [];

    [Inject]
    private JSRuntime JS { get; set; } = default!;

    public void SetParameter<T>(string key, T value)
    {
        if (value == null)
        {
            Parameters.Remove(key);
        }
        else
        {
            Parameters[key] = value;
        }
    }

    public T? GetParameter<T>(string key)
    {
        return Parameters.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }
}
