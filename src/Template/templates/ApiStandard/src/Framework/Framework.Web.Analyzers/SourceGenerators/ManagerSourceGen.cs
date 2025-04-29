using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Framework.Web.Analyzers.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public class ManagerSourceGen : IIncrementalGenerator
{
    public static HashSet<string> assemblies = [];
    public static HashSet<string> baseList = [];
    public const string BaseManagerName = "ManagerBase";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find candidate classes in the current assembly
        var candidateClasses = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => HasBaseList(node),
            transform: static (ctx, cancellationToken) =>
            {
                var classDecl = (ClassDeclarationSyntax)ctx.Node;
                var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken) as INamedTypeSymbol;

                if (symbol != null && InheritsFromManagerBase(symbol))
                {
                    return symbol;
                }
                return null;
            }
        ).Where(symbol => symbol != null);

        var referencedClasses = context.CompilationProvider.SelectMany((compilation, cancellationToken) =>
        {
            var managerClasses = new List<INamedTypeSymbol>();

            foreach (var reference in compilation.References)
            {
                var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;

                if (assemblySymbol != null &&
                    (assemblySymbol.Name.EndsWith("Mod", StringComparison.OrdinalIgnoreCase) ||
                    assemblySymbol.Name.Equals("SharedModule", StringComparison.OrdinalIgnoreCase)))
                {

                    if (!assemblies.Contains(assemblySymbol.Name))
                    {
                        assemblies.Add(assemblySymbol.Name);

                    }

                    foreach (var type in GetAllTypes(assemblySymbol.GlobalNamespace))
                    {
                        if (type is INamedTypeSymbol namedType && InheritsFromManagerBase(namedType))
                        {
                            managerClasses.Add(namedType);
                        }
                    }
                }
            }
            return managerClasses;
        });

        // Combine current and referenced classes into a single collection with explicit type arguments
        var allManagerClasses = candidateClasses.Collect()
            .Combine(referencedClasses.Collect())
            .Select((pair, token) => pair.Item1.Concat(pair.Item2)
            );

        context.RegisterSourceOutput(context.CompilationProvider.Combine(allManagerClasses), (spc, pair) =>
        {
            var compilation = pair.Left;
            var classes = pair.Right;

            string? managerSource = GenerateExtensions(classes);
            if (!string.IsNullOrWhiteSpace(managerSource))
            {
                spc.AddSource("__AterAutoGen__AppManagerServiceExtensions.g.cs", SourceText.From(managerSource!, System.Text.Encoding.UTF8));
            }

            string? modSource = GenerateModExtensions(compilation, assemblies);
            if (!string.IsNullOrWhiteSpace(modSource))
            {
                spc.AddSource("__AterAutoGen__ModuleExtensions.g.cs", SourceText.From(modSource!, System.Text.Encoding.UTF8));
            }
        });
    }

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
        if ((baseType?.Name == BaseManagerName
            || baseType?.OriginalDefinition.Name == BaseManagerName)
            && symbol.OriginalDefinition.Name != BaseManagerName)
        {
            return true;
        }
        return false;
    }
    private static string? GenerateExtensions(IEnumerable<INamedTypeSymbol?> symbols)
    {
        // Order the classes by name
        var distinctSymbols = symbols
            .Where(s => s != null)
            .Distinct(SymbolEqualityComparer.Default)
            .OrderBy(s => s!.Name)
            .ToList();

        if (distinctSymbols == null || distinctSymbols.Count == 0)
        {
            return null;
        }
        var registrations = string.Empty;
        foreach (var symbol in distinctSymbols)
        {
            // Use fully qualified name to avoid ambiguity.
            string fullName = symbol!.ToDisplayString();
            registrations += $"        services.AddScoped(typeof({fullName}));\r\n";
        }

        return $$"""
            // <auto-generated/>
            using Microsoft.Extensions.DependencyInjection;

            namespace Http.API.Extensions;
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

    private static string? GenerateModExtensions(Compilation compilation, HashSet<string> modAssemblies)
    {
        var validAssemblies = new List<string>();

        foreach (var assemblyName in modAssemblies)
        {
            var assemblySymbol = compilation.ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

            if (assemblySymbol != null)
            {
                var moduleExtensionsClass = GetModuleExtensionsClass(compilation, assemblyName);
                if (moduleExtensionsClass != null && HasRequiredMethod(moduleExtensionsClass, $"Add{assemblyName}"))
                {
                    validAssemblies.Add(assemblyName);
                }
            }
        }

        if (validAssemblies.Count == 0)
        {
            return null;
        }

        var registrations = string.Empty;
        var usingExpressions = string.Empty;
        foreach (var assemblyName in validAssemblies.OrderBy(name => name))
        {
            usingExpressions += $"using {assemblyName};\r\n";
            registrations += $"        builder.Add{assemblyName}();\r\n";
        }

        return $$"""
        // <auto-generated/>
        {{usingExpressions}}
        using Microsoft.Extensions.DependencyInjection;

        namespace Http.API.Extensions;
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

    private static INamedTypeSymbol? GetModuleExtensionsClass(Compilation compilation, string assemblyName)
    {
        // 获取目标程序集符号
        var assemblySymbol = compilation.References
            .Select(reference => compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol)
            .FirstOrDefault(assembly => assembly != null
                && assembly.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

        if (assemblySymbol == null)
        {
            return null;
        }

        // 查找 ModuleExtensions 类
        var test = GetAllTypes(assemblySymbol.GlobalNamespace).ToList();
        return GetAllTypes(assemblySymbol.GlobalNamespace)
            .FirstOrDefault(t => t.Name == "ModuleExtensions" &&
                                 t.DeclaredAccessibility == Accessibility.Public &&
                                 t.IsStatic);
    }

    private static bool HasRequiredMethod(INamedTypeSymbol moduleExtensionsClass, string methodName)
    {
        return moduleExtensionsClass.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m.Name == methodName && m.DeclaredAccessibility == Accessibility.Public && m.IsStatic);
    }
}
