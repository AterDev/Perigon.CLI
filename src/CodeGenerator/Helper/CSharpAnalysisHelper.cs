namespace CodeGenerator.Helper;

/// <summary>
/// c# 分析帮助类
/// </summary>
public class CSharpAnalysisHelper
{
    /// <summary>
    /// 获取接口类的继承信息
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="tree"></param>
    /// <returns></returns>
    public static async Task<INamedTypeSymbol?> GetBaseInterfaceInfoAsync(
        CSharpCompilation compilation,
        SyntaxTree tree
    )
    {
        SyntaxNode root = await tree.GetRootAsync();
        compilation = compilation.AddSyntaxTrees(tree);
        var semanticModel = compilation.GetSemanticModel(tree);

        InterfaceDeclarationSyntax? interfaceDeclaration = root
            ?.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault();
        if (interfaceDeclaration != null)
        {
            TypeSyntax? baseInterface = interfaceDeclaration.BaseList?.Types.First().Type;
            if (baseInterface == null)
            {
                return default;
            }
            var baseType = semanticModel.GetTypeInfo(baseInterface).Type as INamedTypeSymbol;
            return baseType;
        }
        return default;
    }

    /// <summary>
    /// convert clr type to C# type name
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ToTypeName(Type type)
    {
        var typeMap = new Dictionary<string, string>
        {
            ["Int32"] = "int",
            ["Int64"] = "long",
            ["Int16"] = "short",
            ["UInt32"] = "uint",
            ["UInt64"] = "ulong",
            ["UInt16"] = "ushort",
            ["Boolean"] = "bool",
            ["String"] = "string",
            ["Decimal"] = "decimal",
            ["Double"] = "double",
            ["Single"] = "float",
            ["Byte"] = "byte",
            ["SByte"] = "sbyte",
            ["Char"] = "char",
            ["Object"] = "object",
            ["DateTime"] = "DateTime",
            ["Guid"] = "Guid",
        };

        // 处理可空类型
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = type.GetGenericArguments()[0];
            return ToTypeName(underlyingType);
        }
        // 处理数组类型
        if (type.IsArray)
        {
            return ToTypeName(type.GetElementType()!) + "[]";
        }
        // 处理泛型类型（如 List<int?>, Dictionary<string?, int?>）
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name.Split('`')[0];
            var genericArgs = type.GetGenericArguments().Select(ToTypeName);
            return $"{genericTypeName}<{string.Join(", ", genericArgs)}>";
        }
        // 处理基本类型和 string
        return typeMap.TryGetValue(type.Name, out var csharpType) ? csharpType : type.Name;
    }
}
