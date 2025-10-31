using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Ater.Web.SourceGeneration;

[Generator(LanguageNames.CSharp)]
public class ManagerSourceGen : IIncrementalGenerator
{
    public static HashSet<string> assemblies = [];
    public static HashSet<string> baseList = [];
    public const string BaseManagerName = "ManagerBase";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find manager classes in the current project
        var candidateClasses = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => HasBaseList(node),
                transform: static (ctx, cancellationToken) =>
                {
                    var classDecl = (ClassDeclarationSyntax)ctx.Node;

                    if (
                        ctx.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken)
                            is INamedTypeSymbol symbol
                        && InheritsFromManagerBase(symbol)
                    )
                    {
                        return symbol;
                    }
                    return null;
                }
            )
            .Where(symbol => symbol != null);

        // get manager classes from referenced assemblies(modules)
        var referencedClasses = context.CompilationProvider.SelectMany(
            (compilation, cancellationToken) =>
            {
                var managerClasses = new List<INamedTypeSymbol>();
                foreach (var reference in compilation.References)
                {
                    if (
                        compilation.GetAssemblyOrModuleSymbol(reference)
                            is IAssemblySymbol assemblySymbol
                        && assemblySymbol.Name.EndsWith("Mod", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        if (!assemblies.Contains(assemblySymbol.Name))
                        {
                            assemblies.Add(assemblySymbol.Name);
                        }

                        foreach (var type in GetAllTypes(assemblySymbol.GlobalNamespace))
                        {
                            if (
                                type is INamedTypeSymbol namedType
                                && InheritsFromManagerBase(namedType)
                            )
                            {
                                managerClasses.Add(namedType);
                            }
                        }
                    }
                }
                return managerClasses;
            }
        );

        var allManagerClasses = candidateClasses
            .Collect()
            .Combine(referencedClasses.Collect())
            .Select((pair, token) => pair.Left.Concat(pair.Right));

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(allManagerClasses),
            (spc, pair) =>
            {
                var compilation = pair.Left;
                var classes = pair.Right;

                // 过滤
                if (compilation.Assembly.Name == "Share")
                {
                    return;
                }

                var managerSource = GenerateExtensions(compilation, classes);
                if (!string.IsNullOrWhiteSpace(managerSource))
                {
                    spc.AddSource(
                        "__AterAutoGen__AppManagerServiceExtensions.g.cs",
                        SourceText.From(managerSource!, System.Text.Encoding.UTF8)
                    );
                }

                var modSource = GenerateModExtensions(compilation, assemblies);
                if (!string.IsNullOrWhiteSpace(modSource))
                {
                    spc.AddSource(
                        "__AterAutoGen__ModuleExtensions.g.cs",
                        SourceText.From(modSource!, System.Text.Encoding.UTF8)
                    );
                }
            }
        );
    }

    /// <summary>
    /// 获取命名空间下的所有类型
    /// </summary>
    /// <param name="namespaceSymbol"></param>
    /// <returns></returns>
    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var type in GetAllTypes(nestedNamespace))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol namedType)
            {
                yield return namedType;
            }
        }
    }

    /// <summary>
    /// 判断节点是否包含基类列表
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static bool HasBaseList(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax classDecl && classDecl.BaseList != null)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 筛选出继承自ManagerBase的类
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private static bool InheritsFromManagerBase(INamedTypeSymbol symbol)
    {
        var baseType = symbol.BaseType;
        if (
            (
                baseType?.Name == BaseManagerName
                || baseType?.OriginalDefinition.Name == BaseManagerName
            )
            && symbol.OriginalDefinition.Name != BaseManagerName
        )
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 生成添加Manager的扩展
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    private static string? GenerateExtensions(
        Compilation compilation,
        IEnumerable<INamedTypeSymbol?> symbols
    )
    {
        var namespaceName = compilation.Assembly.Name ?? "Service";
        // Order the classes by name
        var distinctSymbols = symbols
            .Where(s => s != null)
            .Distinct(SymbolEqualityComparer.Default)
            .OrderBy(s => s!.Name)
            .ToList();

        if (distinctSymbols.Count == 0)
        {
            return null;
        }

        var registrations = string.Empty;
        foreach (var symbol in distinctSymbols)
        {
            // Use fully qualified name to avoid ambiguity.
            var fullName = symbol!.ToDisplayString();
            registrations += $"        services.AddScoped(typeof({fullName}));\r\n";
        }

        return $$"""
            // <auto-generated/>
            using Microsoft.Extensions.DependencyInjection;

            namespace {{namespaceName}}.Extension;
            public static partial class __AterAutoGen__AppManagerServiceExtensions
            {
                public static IServiceCollection AddManagers(this IServiceCollection services)
                {
            {{registrations}}
                    return services;
                }
            }
            """;
    }

    /// <summary>
    /// 生成引用并添加Mod扩展方法
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="modAssemblies"></param>
    /// <returns></returns>
    private static string? GenerateModExtensions(
        Compilation compilation,
        HashSet<string> modAssemblies
    )
    {
        var namespaceName = compilation.Assembly.Name ?? "Service";
        var validAssemblies = new List<string>();

        foreach (var assemblyName in modAssemblies)
        {
            var assemblySymbol = compilation.ReferencedAssemblyNames.FirstOrDefault(a =>
                a.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)
            );

            if (assemblySymbol != null)
            {
                var moduleExtensionsClass = GetModuleExtensionsClass(compilation, assemblyName);
                if (
                    moduleExtensionsClass != null
                    && HasRequiredMethod(moduleExtensionsClass, $"Add{assemblyName}")
                )
                {
                    validAssemblies.Add(assemblyName);
                }
            }
        }

        var registrations = string.Empty;
        var usingExpressions = string.Empty;

        if (validAssemblies.Count > 0)
        {
            foreach (var assemblyName in validAssemblies.OrderBy(name => name))
            {
                usingExpressions += $"using {assemblyName};\r\n";
                registrations += $"        builder.Add{assemblyName}();\r\n";
            }
        }

        return $$"""
            // <auto-generated/>
            {{usingExpressions}}
            using Microsoft.Extensions.DependencyInjection;

            namespace {{namespaceName}}.Extension;
            public static partial class __AterAutoGen__ModuleExtensions
            {
                public static IHostApplicationBuilder AddModules(this IHostApplicationBuilder builder)
                {
            {{registrations}}
                    return builder;
                }
            }
            """;
    }

    private static INamedTypeSymbol? GetModuleExtensionsClass(
        Compilation compilation,
        string assemblyName
    )
    {
        // 获取目标程序集符号
        var assemblySymbol = compilation
            .References.Select(reference =>
                compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol
            )
            .FirstOrDefault(assembly =>
                assembly != null
                && assembly.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)
            );

        if (assemblySymbol == null)
        {
            return null;
        }

        // 查找 ModuleExtensions 类
        return GetAllTypes(assemblySymbol.GlobalNamespace)
            .FirstOrDefault(t =>
                t.Name == "ModuleExtensions"
                && t.DeclaredAccessibility == Accessibility.Public
                && t.IsStatic
            );
    }

    private static bool HasRequiredMethod(INamedTypeSymbol moduleExtensionsClass, string methodName)
    {
        return moduleExtensionsClass
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m =>
                m.Name == methodName
                && m.DeclaredAccessibility == Accessibility.Public
                && m.IsStatic
            );
    }
}
