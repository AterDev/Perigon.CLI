using System.Reflection;

namespace Ater.Common.Utils;

/// <summary>
/// 枚举帮助类
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// 枚举转数组
    /// </summary>
    /// <returns></returns>
    public static List<EnumDictionary> ToList(Type type)
    {
        List<EnumDictionary> result = [];
        var enumNames = Enum.GetNames(type);
        var values = Enum.GetValues(type);

        if (enumNames != null)
        {
            for (var i = 0; i < enumNames.Length; i++)
            {
                var field = type.GetField(enumNames[i], BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    var attribute = field.GetCustomAttribute<DescriptionAttribute>(true);
                    result.Add(
                        new EnumDictionary
                        {
                            Name = field.Name,
                            Description = attribute?.Description ?? field.Name,
                            Value = Convert.ToInt32(values.GetValue(i)),
                        }
                    );
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 获取枚举描述
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum value)
    {
        var field = value
            .GetType()
            .GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
        if (
            field != null
            && field.GetCustomAttribute<DescriptionAttribute>(true)
                is DescriptionAttribute attribute
        )
        {
            return attribute.Description;
        }
        return value.ToString();
    }

    /// <summary>
    /// 获取程序集中所有枚举类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static Type[] GetEnumTypes(this Assembly assembly)
    {
        return assembly.GetTypes().Where(t => t.IsEnum).ToArray();
    }

    /// <summary>
    /// 获取程序集所有枚举信息
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, List<EnumDictionary>> GetAllEnumInfo(
        List<string>? myAssemblies = null
    )
    {
        Dictionary<string, List<EnumDictionary>> res = [];
        myAssemblies ??= ["Share.dll", "Entity.dll"];

        List<Type> allTypes = [];
        var assemblies = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => myAssemblies.Contains(a.ManifestModule.Name))
            .ToList();
        assemblies.ForEach(assembly =>
        {
            Type[] types = assembly.GetEnumTypes();
            allTypes.AddRange(types);
        });

        foreach (Type type in allTypes)
        {
            List<EnumDictionary> enumDict = ToList(type);
            res.Add(type.Name, enumDict);
        }
        return res;
    }
}

public class EnumDictionary
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Value { get; set; }
}
