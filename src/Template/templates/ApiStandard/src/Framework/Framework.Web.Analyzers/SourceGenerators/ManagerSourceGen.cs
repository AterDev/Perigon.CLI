using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Framework.Web.Analyzers.SourceGenerators
{
    [Generator(LanguageNames.CSharp)]
    public class ManagerSourceGen : IIncrementalGenerator
    {
        public static HashSet<string> assemblies = [];
        public static HashSet<string> baseList = [];
        public const string BaseManagerName = "TestManager";

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

            context.RegisterSourceOutput(allManagerClasses, (spc, classes) =>
            {
                string? source = GenerateExtensions(classes);
                if (!string.IsNullOrWhiteSpace(source))
                {
                    spc.AddSource("AppManagerServiceExtensions.g.cs", SourceText.From(source!, System.Text.Encoding.UTF8));
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
            if ((baseType?.Name == "ManagerBase"
                || baseType?.OriginalDefinition.Name == "ManagerBase")
                && symbol.OriginalDefinition.Name != "ManagerBase")
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

            return $@"// <auto-generated/>
using Microsoft.Extensions.DependencyInjection;

namespace Http.API.Extensions;
public static partial class AppManagerServiceExtensions
{{
    public static void AddManagers(this IServiceCollection services)
    {{
{registrations}
    }}
}}";
        }
    }
}
