using System.Diagnostics.CodeAnalysis;

namespace Ater.AspNetCore.Utils;

/// <summary>
/// 快捷帮助工具
/// </summary>
public static class Utils
{
    /// <summary>
    /// has no empty item in the list
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static bool NoEmptyItem([NotNullWhen(true)] params string?[] items)
    {
        if (items == null || items.Length == 0)
        {
            return false;
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return false;
            }
        }
        return true;
    }
}
