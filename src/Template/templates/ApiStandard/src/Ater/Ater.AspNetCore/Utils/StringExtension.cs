using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ater.AspNetCore.Utils;

public static partial class StringExtension
{
    /// <summary>
    /// to hyphen style: HelloWord->hello-word
    /// </summary>
    /// <param name="str"></param>
    /// <param name="separator">分隔符</param>
    /// <returns></returns>
    public static string ToHyphen(this string str, char separator = '-')
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> span = str.AsSpan();
        StringBuilder builder = new();
        var upperNumber = 0;
        for (var i = 0; i < span.Length; i++)
        {
            var item = span[i];
            var pre = i >= 1 ? span[i - 1] : 'a';
            if (char.IsUpper(item) && char.IsLower(pre))
            {
                upperNumber++;
                if (upperNumber > 1)
                {
                    _ = builder.Append(separator);
                }
            }
            else if (item is '_' or ' ')
            {
                _ = builder.Append(separator);
            }
            _ = builder.Append(char.ToLower(item));
        }
        return builder.ToString();
    }

    /// <summary>
    /// to snake lower style: HelloWord->hello_word
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSnakeLower(this string str)
    {
        return str.ToHyphen('_');
    }

    /// <summary>
    /// to Pascalcase style:hello-word->HelloWord
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> span = str.AsSpan();
        StringBuilder resultBuilder = new();
        foreach (var c in span)
        {
            _ = !char.IsLetterOrDigit(c) ? resultBuilder.Append(' ') : resultBuilder.Append(c);
        }
        var result = resultBuilder.ToString();
        result = string.Join(
            string.Empty,
            result.Split(' ').Select(r => r.ToUpperFirst()).ToArray()
        );
        return result;
    }

    /// <summary>
    /// to camelcase style:hello-word->hellowWord
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }
        str = str.ToPascalCase();
        return char.ToLower(str[0]) + str[1..];
    }

    public static string ToUpperFirst(this string str)
    {
        return string.IsNullOrWhiteSpace(str) ? string.Empty : char.ToUpper(str[0]) + str[1..];
    }

    /// <summary>
    /// 计算两字符串相似度
    /// <param name="source">原字符串</param>
    /// <param name="target">对比字符串</param>
    /// <returns>返回0-1.0</returns>
    /// </summary>
    public static double Similarity(this string source, string target)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
        {
            return 1.0;
        }
        else if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
        {
            return 0.0;
        }

        if (source.Equals(target))
        {
            return 1.0;
        }

        var stepsToSame = source.ComputeLevenshteinDistance(target);
        return 1.0 - (stepsToSame / (double)Math.Max(source.Length, target.Length));
    }

    /// <summary>
    /// 计算两字符串转变距离
    /// </summary>
    public static int ComputeLevenshteinDistance(this string source, string target)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
        {
            return 1;
        }
        else if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
        {
            return 0;
        }

        if (source.Equals(target))
        {
            return source.Length;
        }

        ReadOnlySpan<char> sourceSpan = source.AsSpan();
        ReadOnlySpan<char> targetSpan = target.AsSpan();

        var sourceWordCount = sourceSpan.Length;
        var targetWordCount = targetSpan.Length;

        if (sourceWordCount == 0)
        {
            return targetWordCount;
        }

        if (targetWordCount == 0)
        {
            return sourceWordCount;
        }

        var distance = new int[sourceWordCount + 1, targetWordCount + 1];

        for (var i = 0; i <= sourceWordCount; distance[i, 0] = i++) { }
        for (var j = 0; j <= targetWordCount; distance[0, j] = j++) { }

        for (var i = 1; i <= sourceWordCount; i++)
        {
            for (var j = 1; j <= targetWordCount; j++)
            {
                var cost = targetSpan[j - 1] == sourceSpan[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost
                );
            }
        }

        return distance[sourceWordCount, targetWordCount];
    }

    /// <summary>
    /// 对比字符串相似度
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static double GetSimilar(this string source, string target)
    {
        ReadOnlySpan<char> longerSpan,
            shorterSpan;
        if (source.Length > target.Length)
        {
            longerSpan = source.AsSpan();
            shorterSpan = target.AsSpan();
        }
        else
        {
            longerSpan = target.AsSpan();
            shorterSpan = source.AsSpan();
        }

        var sameNum = 0;
        for (var i = 0; i < shorterSpan.Length; i++)
        {
            foreach (var item in longerSpan)
            {
                if (shorterSpan[i] == item)
                {
                    sameNum++;
                    break;
                }
            }
        }
        return sameNum / (double)shorterSpan.Length;
    }

    /// <summary>
    /// 字符串是否为 null/empty/whitespace
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool NotEmpty([NotNullWhen(true)] this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// IsEmail
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsEmail(this string str)
    {
        return EmailRegx().IsMatch(str);
    }

    /// <summary>
    /// 保留字符串中的数字和字母
    /// </summary>
    /// <param name="str"></param>
    /// <param name="additionChars">额外字符</param>
    /// <returns></returns>
    public static string KeepDigitsAndLetters(this string str, char[]? additionChars = null)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        ReadOnlySpan<char> span = str.AsSpan();
        var result = new StringBuilder();
        foreach (var item in span)
        {
            if (
                char.IsLetterOrDigit(item)
                || (additionChars != null && additionChars.Contains(item))
            )
            {
                _ = result.Append(item);
            }
        }
        return result.ToString();
    }

    private static readonly string[] dateFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
    ];

    /// <summary>
    /// 字符串转换为 DateTimeOffset UTC 时间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static DateTimeOffset? ToDateTimeOffset(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return null;
        }

        ReadOnlySpan<char> span = str.AsSpan();

        if (
            DateTimeOffset.TryParseExact(span, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite, out DateTimeOffset dt)
        )
        {
            return dt.ToUniversalTime();
        }
        else if (span.Contains('/'))
        {
            if (
                DateTimeOffset.TryParseExact(
                    span,
                    ["MM/dd/yyyy HH:mm", "M/d/yyyy HH:mm", "M/dd/yyyy", "M/d/yy", "M/d/yy HH:mm"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowInnerWhite,
                    out dt
                )
            )
            {
                return dt.ToUniversalTime();
            }
        }
        else if (span.Contains('.'))
        {
            if (
                DateTimeOffset.TryParseExact(
                    span,
                    ["yyyy.MM.dd"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowInnerWhite,
                    out dt
                )
            )
            {
                return dt.ToUniversalTime();
            }
        }
        return null;
    }

    /// <summary>
    /// FromBase64
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string? FromBase64String(this string str)
    {
        ReadOnlySpan<byte> span = Convert.TryFromBase64String(
            str,
            new Span<byte>(new byte[str.Length * 3 / 4]),
            out var bytesWritten
        )
            ? new Span<byte>(new byte[str.Length * 3 / 4], 0, bytesWritten)
            : ReadOnlySpan<byte>.Empty;

        return span.IsEmpty ? null : Encoding.UTF8.GetString(span);
    }

    /// <summary>
    /// 计算字符串表达式，仅支持整数加减法
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static int CalculateExpression(this string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return 0;
        }

        ReadOnlySpan<char> span = expression.AsSpan();
        var result = 0;
        var operand = 0;
        var operation = '+';

        for (var i = 0; i < span.Length; i++)
        {
            var currentChar = span[i];

            if (char.IsDigit(currentChar))
            {
                operand = (operand * 10) + (currentChar - '0');
            }
            else if (currentChar is '+' or '-')
            {
                result = ApplyOperation(result, operand, operation);
                operand = 0;
                operation = currentChar;
            }
            else if (!char.IsWhiteSpace(currentChar))
            {
                return 0;
            }
        }

        return ApplyOperation(result, operand, operation);
    }

    private static int ApplyOperation(
        int result,
        int operand,
        char operation
)
    {
        return operation switch
        {
            '+' => result + operand,
            '-' => result - operand,
            _ => throw new FormatException($"Invalid operator: {operation}"),
        };
    }

    [GeneratedRegex(@"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$")]
    private static partial Regex EmailRegx();
}
