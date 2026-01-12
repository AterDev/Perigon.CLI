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
    public ICollection<string> UserEntities { get; set; }

    public DtoCodeGenerate(EntityInfo entityInfo, ICollection<string>? userEntities)
    {
        Namespace = entityInfo.GetDtoNamespace();
        EntityInfo = entityInfo;
        KeyType = EntityInfo.KeyType switch
        {
            EntityKeyType.Int => "Int",
            EntityKeyType.String => "String",
            _ => "Guid",
        };
        UserEntities = userEntities ?? [];
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
            Comment = FormatComment(EntityInfo.Comment, ConstVal.DetailDto),
            Tag = EntityInfo.Name,
            Properties =
                EntityInfo
                    .PropertyInfos?.Where(p => p.Name is not ConstVal.IsDeleted)
                    .Where(p => !p.IsJsonIgnore && !p.IsNavigation)
                    .Where(p => !EntityInfo.IgnoreTypes.Contains(p.Type))
                    .Where(p => !(p.IsList && p.IsNavigation))
                    .Select(p => p with { })
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
            Comment = FormatComment(EntityInfo.Comment, ConstVal.ItemDto),
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
                    .Select(p => p with { })
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
        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.FilterDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, ConstVal.FilterDto),
            Tag = EntityInfo.Name,
            BaseType = ConstVal.FilterBase,
            Properties = EntityInfo.GetFilterProperties(),
        };

        // 筛选条件调整为可空
        foreach (PropertyInfo item in dto.Properties)
        {
            item.IsNullable = true;
            item.IsRequired = false;
        }
        EntityInfo
            .GetRequiredNavigationProperties()
            ?.ForEach(item =>
            {
                item.IsNullable = true;
                item.Type = "Guid";
                dto.Properties.RemoveAll(p => p.Name.Equals(item.Name));
                dto.Properties.Add(item);
            });
        return dto;
    }

    public DtoInfo GetAddDto()
    {
        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.AddDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, ConstVal.AddDto),
            Tag = EntityInfo.Name,
            Properties =
                EntityInfo
                    .PropertyInfos?.Where(p =>
                        !p.IsShadow
                        && !p.IsNavigation
                        && !EntityInfo.IgnoreTypes.Contains(p.Type)
                        && !EntityInfo.IgnoreProperties.Contains(p.Name)
                    )
                    .Select(p => p with { })
                    .ToList() ?? [],
        };

        EntityInfo
            .GetRequiredNavigationProperties()
            .Where(r => !UserEntities.Contains(r.Type))
            .ToList()
            .ForEach(item =>
            {
                item.Type = "Guid";
                dto.Properties.RemoveAll(p => p.Name.Equals(item.Name));
                dto.Properties.Add(item);
            });

        return dto;
    }

    /// <summary>
    /// UpdateDto
    /// </summary>
    /// <returns></returns>
    public DtoInfo GetUpdateDto()
    {
        DtoInfo dto = new()
        {
            EntityFullName = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + ConstVal.UpdateDto,
            EntityNamespace = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, ConstVal.UpdateDto),
            Tag = EntityInfo.Name,
            // 处理非 required的都设置为 nullable
            Properties = EntityInfo
                .PropertyInfos.Where(p =>
                    !p.IsShadow
                    && !p.IsNavigation
                    && !EntityInfo.IgnoreTypes.Contains(p.Type)
                    && !EntityInfo.IgnoreProperties.Contains(p.Name)
                )
                .Select(p => p with { })
                .ToList(),
        };

        EntityInfo
            .GetRequiredNavigationProperties()
            .Where(r => !UserEntities.Contains(r.Type))
            .ToList()
            .ForEach(item =>
            {
                item.Type = "Guid";
                dto.Properties.RemoveAll(p => p.Name.Equals(item.Name));
                dto.Properties.Add(item);
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
