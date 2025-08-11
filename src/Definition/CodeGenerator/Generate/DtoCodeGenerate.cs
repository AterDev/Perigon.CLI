using System.Text.RegularExpressions;
using Entity;
using Entity.StudioMod;

namespace CodeGenerator.Generate;

/// <summary>
/// dto generate
/// </summary>
public partial class DtoCodeGenerate
{
    public EntityInfo EntityInfo { get; init; }
    public string KeyType { get; set; } = ConstVal.Guid;

    /// <summary>
    /// dto 输出的 程序集名称
    /// </summary>
    public string Namespace { get; set; }

    public DtoCodeGenerate(EntityInfo entityInfo)
    {
        Namespace = entityInfo.GetDtoNamespace();
        EntityInfo = entityInfo;
        KeyType = EntityInfo.KeyType switch
        {
            EntityKeyType.Int => "Int",
            EntityKeyType.String => "String",
            _ => "Guid",
        };
    }

    /// <summary>
    /// 注释内容替换
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="extendString"></param>
    /// <returns></returns>
    private static string FormatComment(string? comment, string extendString = "")
    {
        if (comment == null)
        {
            return "";
        }

        Regex regex = SummaryCommentRegex();
        Match match = regex.Match(comment);
        if (match.Success)
        {
            string summary = match.Groups["comment"].Value;
            string newComment = summary.Replace("表", "") + extendString;
            comment = comment.Replace(summary, newComment);
        }
        return comment;
    }

    /// <summary>
    /// the detail dto
    /// </summary>
    /// <returns></returns>
    public DtoInfo GetDetailDto()
    {
        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.DetailDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, " Detail"),
            Tag = EntityInfo.Name,
            Properties =
                EntityInfo
                    .PropertyInfos?.Where(p => p.Name is not ConstVal.IsDeleted)
                    .Where(p => !p.IsJsonIgnore)
                    .Where(p => !EntityInfo.IgnoreTypes.Contains(p.Type))
                    .Where(p => !(p.IsList && p.IsNavigation))
                    .ToList() ?? [],
        };

        return dto;
    }

    /// <summary>
    /// the list item dto
    /// </summary>
    /// <returns></returns>
    public DtoInfo GetItemDto()
    {
        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.ItemDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, " ListItem"),
            Tag = EntityInfo.Name,
            Properties =
                EntityInfo
                    .PropertyInfos?.Where(p =>
                        p.Name is not ConstVal.IsDeleted and not ConstVal.UpdatedTime
                    )
                    .Where(p => !p.IsJsonIgnore && !EntityInfo.IgnoreTypes.Contains(p.Type))
                    .Where(p =>
                        !p.IsList
                        && (p.MaxLength is not (not null and >= 200))
                        && (!p.Name.EndsWith("Id") || p.Name.Equals("Id"))
                        && !p.IsNavigation
                    )
                    .ToList() ?? [],
        };

        return dto;
    }

    /// <summary>
    /// the filter dto
    /// </summary>
    /// <returns></returns>
    public DtoInfo GetFilterDto()
    {
        List<PropertyInfo>? referenceProps = EntityInfo
            .PropertyInfos?.Where(p => p.IsNavigation && !p.IsList)
            .Where(p => !p.IsJsonIgnore)
            .Select(s => new PropertyInfo() { Name = s.Name + "Id", Type = KeyType + "?" })
            .ToList();

        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.FilterDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, " Filter"),
            Tag = EntityInfo.Name,
            BaseType = ConstVal.FilterBase,
            Properties =
                EntityInfo.GetFilterProperties().Select(p => p.Adapt<PropertyInfo>()).ToList()
                ?? [],
        };

        // 筛选条件调整为可空
        foreach (PropertyInfo item in dto.Properties)
        {
            item.IsNullable = true;
            item.IsRequired = false;
        }
        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });
        return dto;
    }

    public DtoInfo GetAddDto()
    {
        // 导航属性处理
        List<PropertyInfo>? referenceProps = EntityInfo
            .Navigations?.Where(p => !p.IsCollection && !p.IsSkipNavigation)
            .Select(n => new PropertyInfo()
            {
                Name = n.ForeignKey,
                Type = n.Type,
                IsRequired = n.IsRequired,
                IsNullable = false,
                DefaultValue = "",
            })
            .ToList();

        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.AddDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, " AddDto"),
            Tag = EntityInfo.Name,
            Properties =
                EntityInfo
                    .PropertyInfos?.Where(p =>
                        !p.IsNavigation
                        && p.HasSet
                        && !EntityInfo.IgnoreTypes.Contains(p.Type)
                        && p.Name != ConstVal.Id
                        && p.Name != ConstVal.CreatedTime
                        && p.Name != ConstVal.UpdatedTime
                        && p.Name != ConstVal.IsDeleted
                    )
                    .ToList() ?? [],
        };

        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });

        return dto;
    }

    /// <summary>
    /// 更新dto
    /// 导航属性 Name+Id,过滤列表属性
    /// </summary>
    /// <returns></returns>
    public DtoInfo GetUpdateDto()
    {
        // 导航属性处理
        List<PropertyInfo>? referenceProps = EntityInfo
            .PropertyInfos?.Where(p => !p.IsJsonIgnore)
            .Where(p =>
                p.IsNavigation
                && (p.IsRequired || !p.IsNullable || !string.IsNullOrWhiteSpace(p.DefaultValue))
            )
            .Where(p => !p.Type.Equals("User") && !p.Type.Equals("SystemUser"))
            .Select(s => new PropertyInfo()
            {
                Name = s.NavigationName + (s.IsList ? "Ids" : "Id"),
                Type = s.IsList ? $"List<{KeyType}>" : KeyType,
                IsRequired = s.IsRequired,
                IsNullable = s.IsNullable,
            })
            .ToList();
        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.UpdateDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, " UpdateDTO"),
            Tag = EntityInfo.Name,
            // 处理非 required的都设置为 nullable
            Properties =
                EntityInfo
                    .PropertyInfos?.Where(p =>
                        !p.IsNavigation
                        && p.HasSet
                        && !EntityInfo.IgnoreTypes.Contains(p.Type)
                        && p.Name != ConstVal.Id
                        && p.Name != ConstVal.CreatedTime
                        && p.Name != ConstVal.UpdatedTime
                        && p.Name != ConstVal.IsDeleted
                    )
                    .Select(p => p.Adapt<PropertyInfo>())
                    .ToList() ?? [],
        };

        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });

        foreach (PropertyInfo item in dto.Properties)
        {
            item.IsNullable = true;
        }
        return dto;
    }

    public List<string> GetGlobalUsings()
    {
        return
        [
            "global using System;",
            "global using System.Text.Json;",
            "global using System.ComponentModel.DataAnnotations;",
            $"global using {Namespace}.{ConstVal.ModelsDir};",
            $"global using {ConstVal.CoreLibName}.{ConstVal.ModelsDir};",
            $"global using {EntityInfo.NamespaceName};",
        ];
    }

    [GeneratedRegex(@"/// <summary>\r\n/// (?<comment>.*)\r\n/// </summary>")]
    private static partial Regex SummaryCommentRegex();
}
