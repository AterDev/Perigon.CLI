using System.Text;

namespace CodeGenerator.Generate.Helper;

/// <summary>
/// 简单的 TypeScript 代码写入器，统一缩进和换行，避免到处手写空格与 \n。
/// 默认使用 2 空格缩进。
/// </summary>
public class TsCodeWriter
{
    private readonly StringBuilder _sb = new();
    private int _indentLevel = 0;
    private readonly string _indentUnit;
    private bool _lastWasBlank = false;

    public TsCodeWriter(int indentSpaces = 2)
    {
        _indentUnit = new string(' ', indentSpaces);
    }

    public TsCodeWriter Indent()
    {
        _indentLevel++;
        return this;
    }

    public TsCodeWriter Unindent()
    {
        if (_indentLevel > 0) _indentLevel--;
        return this;
    }

    public TsCodeWriter AppendLine(string line = "")
    {
        if (line.Length > 0)
        {
            _sb.Append(string.Concat(Enumerable.Repeat(_indentUnit, _indentLevel)));
            _sb.AppendLine(line);
            _lastWasBlank = false;
        }
        else
        {
            _sb.AppendLine();
            _lastWasBlank = true;
        }
        return this;
    }

    /// <summary>
    /// 如果上一行不是空行则添加一个空行，避免连续空行
    /// </summary>
    public TsCodeWriter AppendBlankIfPreviousNotBlank()
    {
        if (!_lastWasBlank)
        {
            AppendLine(string.Empty);
        }
        return this;
    }

    public TsCodeWriter OpenBlock(string header)
    {
        AppendLine(header + " {");
        Indent();
        return this;
    }

    public TsCodeWriter CloseBlock()
    {
        Unindent();
        AppendLine("}");
        return this;
    }

    /// <summary>
    /// 追加注释。multiline=true 时按行分割，每行前加 *。
    /// </summary>
    public TsCodeWriter AppendComment(string summary, bool multiline = false)
    {
        if (string.IsNullOrWhiteSpace(summary)) return this;
        AppendLine("/**");
        if (multiline)
        {
            foreach (var line in summary.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line)) AppendLine(" * " + line.Trim());
            }
        }
        else
        {
            AppendLine(" * " + summary.Trim());
        }
        AppendLine(" */");
        return this;
    }

    /// <summary>
    /// 批量导入。自动去重与排序。
    /// </summary>
    public TsCodeWriter AppendImports(IEnumerable<string> imports)
    {
        var distinct = imports.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct().OrderBy(s => s);
        foreach (var line in distinct)
        {
            AppendLine(line);
        }
        return this;
    }

    /// <summary>
    /// 对齐函数参数列表: name: type 形式按冒号对齐。
    /// </summary>
    public static string AlignParameters(IEnumerable<(string Name, string Type, bool Optional)> items)
    {
        var list = items.ToList();
        if (list.Count == 0) return string.Empty;
        int maxLen = list.Max(i => i.Name.Length);
        var sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            var it = list[i];
            string opt = it.Optional ? "?" : string.Empty;
            sb.Append(it.Name).Append(opt).Append(new string(' ', maxLen - it.Name.Length)).Append(": ").Append(it.Type);
            if (i < list.Count - 1) sb.Append(", ");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 对齐多行注释块 (每行前添加 *，保持宽度)。
    /// </summary>
    public static string AlignCommentBlock(IEnumerable<string> lines)
    {
        var cleaned = lines.Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        if (cleaned.Count == 0) return string.Empty;
        var sb = new StringBuilder();
        sb.AppendLine("/**");
        foreach (var l in cleaned)
        {
            sb.AppendLine(" * " + l);
        }
        sb.Append(" */");
        return sb.ToString();
    }

    public override string ToString() => _sb.ToString();
}
