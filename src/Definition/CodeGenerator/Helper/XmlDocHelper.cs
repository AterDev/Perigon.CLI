namespace CodeGenerator.Helper;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

public class XmlDocHelper
{
    public FrozenDictionary<string, string> SummaryMap { get; }

    public XmlDocHelper(string path)
    {
        var map = new Dictionary<string, string>();
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var xmlFiles = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
        foreach (var file in xmlFiles)
        {
            try
            {
                var xdoc = XDocument.Load(file);
                var members = xdoc.Descendants("member");
                foreach (var member in members)
                {
                    var name = member.Attribute("name")?.Value;
                    var summary = member.Element("summary")?.Value?.Trim();
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(summary))
                    {
                        map[name] = summary;
                    }
                }
            }
            catch (Exception)
            {
                // 可记录日志，忽略解析失败的文件
            }
        }
        SummaryMap = map.ToFrozenDictionary();
    }

    public string? GetClassSummary(string fullTypeName) => TryGetSummary($"T:{fullTypeName}");

    public string? GetPropertySummary(string fullTypeName, string propertyName) =>
        TryGetSummary($"P:{fullTypeName}.{propertyName}");

    public string? GetEnumFieldSummary(string fullEnumTypeName, string fieldName) =>
        TryGetSummary($"F:{fullEnumTypeName}.{fieldName}");

    private string? TryGetSummary(string memberName)
    {
        return SummaryMap.TryGetValue(memberName, out var summary) ? Normalize(summary) : null;
    }

    private static string Normalize(string summary)
    {
        // Collapse whitespace and trim
        var normalized = string.Join(
            " ",
            summary.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries)
        );
        return normalized.Trim();
    }
}
