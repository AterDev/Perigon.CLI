// -----------------------------------------------------------------------------
// File: JsonHelper.cs
// Author: Ater Team
// Description: Provides utility methods for common operations on JsonElement and JsonNode, supporting multi-level path access and node CRUD.
// -----------------------------------------------------------------------------
using System.Text.Json.Nodes;

namespace Ater.AspNetCore.Utils;

/// <summary>
/// JsonHelper provides utility methods for <see cref="System.Text.Json.JsonElement"/> and <see cref="JsonNode"/>.
/// Supports multi-level path access and node CRUD operations.
/// </summary>
public class JsonHelper
{
    /// <summary>
    /// The current JsonElement instance being operated on.
    /// </summary>
    public JsonElement JsonElement { get; set; }

    /// <summary>
    /// Construct JsonHelper from a JsonDocument.
    /// </summary>
    /// <param name="jsonDocument">The JsonDocument instance.</param>
    public JsonHelper(JsonDocument jsonDocument)
    {
        JsonElement = jsonDocument.RootElement;
    }

    /// <summary>
    /// Construct JsonHelper from a JsonElement.
    /// </summary>
    /// <param name="jsonElement">The JsonElement instance.</param>
    public JsonHelper(JsonElement jsonElement)
    {
        JsonElement = jsonElement;
    }

    /// <summary>
    /// Get the JsonElement node at the specified multi-level path.
    /// </summary>
    /// <param name="keyPath">Dot-separated multi-level path, e.g. "a.b.c"</param>
    /// <returns>The corresponding JsonElement node, or null if not found.</returns>
    public JsonElement? GetJsonNode(string keyPath)
    {
        var paths = keyPath.Split('.');
        var current = JsonElement;
        foreach (var path in paths)
        {
            if (
                current.ValueKind == JsonValueKind.Object
                && current.TryGetProperty(path, out var next)
            )
            {
                current = next;
            }
            else
            {
                return null;
            }
        }
        return current;
    }

    /// <summary>
    /// Get the string value at the specified multi-level path.
    /// </summary>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <returns>The string value, or null if not found.</returns>
    public string? GetJsonString(string keyPath)
    {
        var node = GetJsonNode(keyPath);
        return node?.GetString();
    }

    /// <summary>
    /// Get the long value at the specified multi-level path.
    /// </summary>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <returns>The long value, or null if not found or type mismatch.</returns>
    public long? GetJsonInt64(string keyPath)
    {
        var node = GetJsonNode(keyPath);
        return node?.ValueKind == JsonValueKind.Number ? node.Value.GetInt64() : null;
    }

    /// <summary>
    /// Get the int value at the specified multi-level path.
    /// </summary>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <returns>The int value, or null if not found or type mismatch.</returns>
    public int? GetJsonInt32(string keyPath)
    {
        var node = GetJsonNode(keyPath);
        return node?.ValueKind == JsonValueKind.Number ? node.Value.GetInt32() : null;
    }

    /// <summary>
    /// Add or update a JsonNode at the specified multi-level path.
    /// </summary>
    /// <param name="root">The root node.</param>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <exception cref="ArgumentNullException">Thrown if root is null.</exception>
    public static void AddOrUpdateJsonNode(JsonNode root, string keyPath, object? newValue)
    {
        ArgumentNullException.ThrowIfNull(root, nameof(root));
        var paths = keyPath.Split('.');
        var current = root;
        for (var i = 0; i < paths.Length - 1; i++)
        {
            if (current[paths[i]] is JsonObject obj)
            {
                current = obj;
            }
            else
            {
                var newNode = new JsonObject();
                current[paths[i]] = newNode;
                current = newNode;
            }
        }
        current[paths[^1]] = JsonValue.Create(newValue);
    }

    /// <summary>
    /// Get the value at the specified multi-level path, supporting value types.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="node">The root node.</param>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <returns>The value of the target type, or default if not found.</returns>
    public static T? GetValue<T>(JsonNode node, string keyPath)
    {
        var section = GetSectionNode(node, keyPath);
        return section is null ? default : section.GetValue<T>();
    }

    /// <summary>
    /// Get the JsonNode at the specified multi-level path.
    /// </summary>
    /// <param name="node">The root node.</param>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <returns>The corresponding JsonNode, or null if not found.</returns>
    public static JsonNode? GetSectionNode(JsonNode node, string keyPath)
    {
        if (node is null)
        {
            return null;
        }
        var paths = keyPath.Split('.');
        var current = node;
        foreach (var path in paths)
        {
            if (current[path] is JsonNode nextNode)
            {
                current = nextNode;
            }
            else
            {
                return null;
            }
        }
        return current;
    }

    /// <summary>
    /// Try to get the value at the specified multi-level path, avoiding exceptions.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="node">The root node.</param>
    /// <param name="keyPath">Dot-separated multi-level path.</param>
    /// <param name="value">The output value of the target type.</param>
    /// <returns>True if successful, otherwise false.</returns>
    public static bool TryGetValue<T>(JsonNode node, string keyPath, out T? value)
    {
        value = default;
        try
        {
            var section = GetSectionNode(node, keyPath);
            if (section is not null)
            {
                value = section.GetValue<T>();
                return true;
            }
        }
        catch { }
        return false;
    }
}
