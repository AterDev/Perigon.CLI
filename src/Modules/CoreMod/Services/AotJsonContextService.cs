using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RoslynSolution = Microsoft.CodeAnalysis.Solution;

namespace CoreMod.Services;

/// <summary>
/// Generates the explicit System.Text.Json context used by Native AOT services.
/// </summary>
public sealed class AotJsonContextService
{
    private const string GeneratedBegin = "// <perigon-aot-json:generated>";
    private const string GeneratedEnd = "// </perigon-aot-json:generated>";
    private const string GeneratedUsingBegin = "// <perigon-aot-json:usings>";
    private const string GeneratedUsingEnd = "// </perigon-aot-json:usings>";
    private const string ContextFileName = "ApiJsonSerializerContext.cs";
    private const string FixedDictionaryType = "global::System.Collections.Generic.Dictionary<string, string>";

    private static readonly HashSet<string> RouteMethods = new(StringComparer.Ordinal)
    {
        "Map", "MapGet", "MapPost", "MapPut", "MapPatch", "MapDelete", "MapMethods", "MapFallback"
    };

    private static readonly HashSet<string> TypedResultMethods = new(StringComparer.Ordinal)
    {
        "Json", "Ok", "Created", "Accepted", "CreatedAtRoute", "AcceptedAtRoute",
        "BadRequest", "NotFound", "Conflict", "UnprocessableEntity"
    };

    /// <summary>
    /// Generates or updates the JSON context for every C# service below <c>src/Services</c>.
    /// </summary>
    public async Task<IReadOnlyList<string>> GenerateAsync(string root, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var projectRoot = Path.GetFullPath(root);
        var servicesRoot = Path.Combine(projectRoot, "src", "Services");
        if (!Directory.Exists(servicesRoot))
        {
            throw new DirectoryNotFoundException($"Service directory was not found: {servicesRoot}");
        }

        var projectPaths = Directory.EnumerateFiles(servicesRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsBuildPath(path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (projectPaths.Length == 0)
        {
            throw new InvalidOperationException($"No C# service projects were found below {servicesRoot}.");
        }

        RegisterMsBuild();
        using var workspace = MSBuildWorkspace.Create();
        var workspaceFailures = new List<string>();
        workspace.RegisterWorkspaceFailedHandler(eventArgs =>
        {
            if (eventArgs.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                workspaceFailures.Add(eventArgs.Diagnostic.Message);
            }
        });

        var generatedFiles = new List<string>();
        var pendingWrites = new List<(string Path, string Content, string? Original)>();
        foreach (var projectPath in projectPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: cancellationToken);
            ThrowWorkspaceFailures(projectPath, workspaceFailures);

            if (!project.SupportsCompilation)
            {
                throw new InvalidOperationException($"Project does not support compilation: {projectPath}");
            }

            var compilation = await project.GetCompilationAsync(cancellationToken)
                ?? throw new InvalidOperationException($"Roslyn could not create a compilation for {projectPath}.");
            ThrowCompilationErrors(projectPath, compilation);

            var roots = new HashSet<string>(StringComparer.Ordinal)
            {
                FixedDictionaryType
            };
            var contextPath = FindContextPath(project, compilation, out var contextDeclaration);
            if (contextDeclaration is not null)
            {
                AddExistingContextTypes(contextDeclaration, compilation, roots);
            }

            DiscoverEndpointTypes(compilation, roots);
            await DiscoverModuleTypesAsync(project, workspace.CurrentSolution, roots, cancellationToken);

            var outputPath = contextPath ?? Path.Combine(Path.GetDirectoryName(projectPath)!, ContextFileName);
            var original = contextPath is null ? null : contextDeclaration!.SyntaxTree.GetText(cancellationToken).ToString();
            if (contextPath is null && File.Exists(outputPath))
            {
                throw new InvalidOperationException($"Cannot create JSON context because the file already exists: {outputPath}");
            }
            if (original is not null && (!File.Exists(outputPath)
                || !string.Equals(File.ReadAllText(outputPath), original, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException($"JSON context changed during analysis; rerun the command: {outputPath}");
            }
            var content = contextPath is null
                ? CreateContextFile(project, compilation, roots)
                : UpdateContextFile(outputPath, contextDeclaration!, roots, compilation);

            pendingWrites.Add((outputPath, content, original));
            generatedFiles.Add(outputPath);
        }

        foreach (var (path, _, original) in pendingWrites)
        {
            var changed = original is null
                ? File.Exists(path)
                : !File.Exists(path) || !string.Equals(File.ReadAllText(path), original, StringComparison.Ordinal);
            if (changed)
            {
                throw new InvalidOperationException($"JSON context changed during analysis; rerun the command: {path}");
            }
        }

        foreach (var (path, content, original) in pendingWrites)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (!string.Equals(original, content, StringComparison.Ordinal))
            {
                File.WriteAllText(path, content, new UTF8Encoding(false));
            }
        }

        return generatedFiles;
    }

    private static void RegisterMsBuild()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }

    private static bool IsBuildPath(string path)
    {
        var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    private static void ThrowWorkspaceFailures(string projectPath, List<string> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }

        var details = string.Join(Environment.NewLine, failures.Distinct(StringComparer.Ordinal).Take(20));
        failures.Clear();
        throw new InvalidOperationException($"MSBuildWorkspace failed to load {projectPath}:{Environment.NewLine}{details}");
    }

    private static void ThrowCompilationErrors(string projectPath, Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Where(diagnostic => !IsKnownSourceGeneratorDiagnostic(diagnostic, compilation, projectPath))
            .Take(20)
            .Select(diagnostic => diagnostic.ToString())
            .ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                $"Compilation errors prevent JSON analysis for {projectPath}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    private static bool IsKnownSourceGeneratorDiagnostic(
        Diagnostic diagnostic,
        Compilation compilation,
        string projectPath)
    {
        var sourcePath = diagnostic.Location.SourceTree?.FilePath ?? string.Empty;
        var token = diagnostic.Location.IsInSource
            ? diagnostic.Location.SourceTree?.GetText()
                .ToString(diagnostic.Location.SourceSpan)
            : string.Empty;

        if (diagnostic.Id == "CS0234"
            && Path.GetFileName(sourcePath).Equals("GlobalUsings.cs", StringComparison.OrdinalIgnoreCase)
            && token?.Equals("Extension", StringComparison.Ordinal) == true)
        {
            var expectedNamespace = $"{compilation.AssemblyName}.Extension";
            return diagnostic.Location.SourceTree?.GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Any(usingDirective =>
                    usingDirective.Name?.ToString().Equals(expectedNamespace, StringComparison.Ordinal) == true)
                == true;
        }

        if (diagnostic.Id == "CS1061"
            && token is "AddManagers" or "AddModules" or "MapEndpointGroups")
        {
            return true;
        }

        if (diagnostic.Id != "CS0117" || diagnostic.Location.SourceTree is null)
        {
            return false;
        }

        var line = diagnostic.Location.SourceTree.GetText()
            .Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start)
            .ToString();
        return line.Contains("Localizer.", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(token)
            && GetResourceKeys(projectPath).Contains(token);
    }

    private static IReadOnlySet<string> GetResourceKeys(string projectPath)
    {
        var root = FindProjectRoot(projectPath);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var resourcePath in Directory.EnumerateFiles(root, "*.resx", SearchOption.AllDirectories))
        {
            if (IsBuildPath(resourcePath))
            {
                continue;
            }

            try
            {
                var document = XDocument.Load(resourcePath);
                foreach (var data in document.Descendants("data"))
                {
                    var key = data.Attribute("name")?.Value;
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        keys.Add(key);
                    }
                }
            }
            catch (Exception exception) when (
                exception is IOException
                or UnauthorizedAccessException
                or System.Xml.XmlException)
            {
                throw new InvalidOperationException($"Unable to read resource file {resourcePath}.", exception);
            }
        }

        return keys;
    }

    private static string FindProjectRoot(string projectPath)
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(projectPath) ?? projectPath);
        while (directory.Parent is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props"))
                || Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Path.GetDirectoryName(projectPath) ?? projectPath;
    }

    private static string? FindContextPath(
        Project project,
        Compilation compilation,
        out ClassDeclarationSyntax? contextDeclaration)
    {
        var candidates = new List<(string Path, ClassDeclarationSyntax Declaration, INamedTypeSymbol Symbol)>();
        foreach (var document in project.Documents.Where(document => document.FilePath is not null))
        {
            var tree = document.GetSyntaxTreeAsync().GetAwaiter().GetResult();
            if (tree is null)
            {
                continue;
            }

            var model = compilation.GetSemanticModel(tree);
            foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol
                    || symbol.BaseType?.ToDisplayString() != "System.Text.Json.Serialization.JsonSerializerContext")
                {
                    continue;
                }

                if (!declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    throw new InvalidOperationException(
                        $"ApiJsonSerializerContext must be partial: {document.FilePath}");
                }

                candidates.Add((document.FilePath!, declaration, symbol));
            }
        }

        var distinctSymbols = candidates
            .Select(candidate => candidate.Symbol)
            .Distinct(SymbolEqualityComparer.Default)
            .Count();
        if (distinctSymbols > 1 || candidates.Count > 1)
        {
            throw new InvalidOperationException(
                $"Project {project.FilePath} contains multiple ApiJsonSerializerContext declarations. Split or duplicate contexts are not supported.");
        }

        if (candidates.Count == 0)
        {
            contextDeclaration = null;
            return null;
        }

        contextDeclaration = candidates[0].Declaration;
        return candidates[0].Path;
    }

    private static void AddExistingContextTypes(
        ClassDeclarationSyntax declaration,
        Compilation compilation,
        HashSet<string> roots)
    {
        var model = compilation.GetSemanticModel(declaration.SyntaxTree);
        var source = declaration.SyntaxTree.GetText().ToString();
        var generatedBegin = source.IndexOf(GeneratedBegin, StringComparison.Ordinal);
        var generatedEnd = source.IndexOf(GeneratedEnd, StringComparison.Ordinal);
        foreach (var attribute in declaration.AttributeLists.SelectMany(list => list.Attributes))
        {
            if (generatedBegin >= 0 && generatedEnd > generatedBegin
                && attribute.SpanStart >= generatedBegin
                && attribute.Span.End <= generatedEnd + GeneratedEnd.Length)
            {
                continue;
            }

            if (!(attribute.Name.ToString().EndsWith("JsonSerializable", StringComparison.Ordinal)
                    || attribute.Name.ToString().EndsWith("JsonSerializableAttribute", StringComparison.Ordinal))
                || attribute.ArgumentList is null)
            {
                continue;
            }

            var typeOf = attribute.ArgumentList.Arguments
                .Select(argument => model.GetOperation(argument.Expression))
                .OfType<ITypeOfOperation>()
                .FirstOrDefault();
            if (typeOf?.TypeOperand is ITypeSymbol type)
            {
                roots.Add(ToTypeName(type));
            }
        }
    }

    private static void DiscoverEndpointTypes(Compilation compilation, HashSet<string> roots)
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            if (IsGeneratedTree(tree))
            {
                continue;
            }

            var model = compilation.GetSemanticModel(tree);
            foreach (var invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess is null || !RouteMethods.Contains(memberAccess.Name.Identifier.Text))
                {
                    continue;
                }

                var routeMethod = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol
                    ?? model.GetSymbolInfo(invocation).CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
                if (routeMethod is null || !IsAspNetEndpointMethod(routeMethod))
                {
                    continue;
                }

                var skipCount = memberAccess.Name.Identifier.Text == "MapMethods" ? 2 : 1;
                if (memberAccess.Name.Identifier.Text == "MapFallback" && invocation.ArgumentList.Arguments.Count == 1)
                {
                    skipCount = 0;
                }

                if (invocation.ArgumentList.Arguments.Count <= skipCount)
                {
                    throw new InvalidOperationException($"Endpoint route has no handler: {invocation}");
                }

                foreach (var argument in invocation.ArgumentList.Arguments.Skip(skipCount))
                {
                    var handler = ResolveHandler(model, argument.Expression);
                    if (handler is null)
                    {
                        throw new InvalidOperationException($"Unable to resolve endpoint handler: {argument.Expression}");
                    }

                    AddHandlerTypes(handler, compilation, roots);
                }
            }
        }
    }

    private static IMethodSymbol? ResolveHandler(SemanticModel model, ExpressionSyntax expression)
    {
        var symbolInfo = model.GetSymbolInfo(expression);
        if (symbolInfo.Symbol is IMethodSymbol method)
        {
            return method;
        }

        if (symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault() is IMethodSymbol candidate)
        {
            return candidate;
        }

        return model.GetOperation(expression) is IAnonymousFunctionOperation anonymous
            ? anonymous.Symbol
            : null;
    }

    private static void AddHandlerTypes(IMethodSymbol handler, Compilation compilation, HashSet<string> roots)
    {
        foreach (var parameter in handler.Parameters)
        {
            if (HasBindingAttribute(parameter, "FromServices")
                || HasBindingAttribute(parameter, "FromRoute")
                || HasBindingAttribute(parameter, "FromQuery")
                || HasBindingAttribute(parameter, "FromHeader")
                || HasBindingAttribute(parameter, "FromForm"))
            {
                continue;
            }

            if (HasBindingAttribute(parameter, "FromBody"))
            {
                AddRoot(parameter.Type, roots);
                continue;
            }

            if (!IsInfrastructureType(parameter.Type) && IsLikelyBodyType(parameter.Type))
            {
                AddRoot(parameter.Type, roots);
            }
        }

        AddRoot(handler.ReturnType, roots);
        foreach (var reference in handler.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();
            var model = compilation.GetSemanticModel(syntax.SyntaxTree);
            foreach (var invocation in syntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method
                    || !method.IsStatic
                    || method.ContainingNamespace?.ToDisplayString() != "Microsoft.AspNetCore.Http"
                    || method.ContainingType?.Name is not ("TypedResults" or "Results")
                    || !TypedResultMethods.Contains(method.Name))
                {
                    continue;
                }

                foreach (var typeArgument in method.TypeArguments)
                {
                    AddRoot(typeArgument, roots);
                }
                AddRoot(method.ReturnType, roots);
            }
        }
    }

    private static async Task DiscoverModuleTypesAsync(
        Project serviceProject,
        RoslynSolution solution,
        HashSet<string> roots,
        CancellationToken cancellationToken)
    {
        foreach (var projectReference in serviceProject.ProjectReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var moduleProject = solution.GetProject(projectReference.ProjectId);
            if (moduleProject is null || !moduleProject.AssemblyName.EndsWith("Mod", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var moduleCompilation = await moduleProject.GetCompilationAsync(cancellationToken)
                ?? throw new InvalidOperationException($"Could not compile referenced module {moduleProject.FilePath}.");
            ThrowCompilationErrors(moduleProject.FilePath ?? moduleProject.Name, moduleCompilation);
            var assembly = moduleCompilation.Assembly;
            var extensions = GetNamespaceTypes(assembly.GlobalNamespace)
                .FirstOrDefault(type => type.Name == "ModuleExtensions"
                    && type.DeclaredAccessibility == Accessibility.Public
                    && type.IsStatic
                    && type.GetMembers().OfType<IMethodSymbol>().Any(method =>
                        method.Name == "Add" + assembly.Name
                        && method.IsStatic
                        && method.DeclaredAccessibility == Accessibility.Public));
            if (extensions is null)
            {
                continue;
            }

            var modelsNamespaceName = extensions.ContainingNamespace.IsGlobalNamespace
                ? "Models"
                : extensions.ContainingNamespace.ToDisplayString() + ".Models";
            var modelsNamespace = FindNamespace(assembly.GlobalNamespace, modelsNamespaceName);
            if (modelsNamespace is null)
            {
                continue;
            }

            foreach (var type in GetNamespaceTypes(modelsNamespace)
                .Where(type => type.TypeKind == TypeKind.Class)
                .Where(IsPubliclyAccessible)
                .Where(type => !IsOpenGeneric(type)))
            {
                roots.Add(ToTypeName(type));
            }
        }
    }

    private static void AddRoot(ITypeSymbol? type, HashSet<string> roots)
    {
        if (type is null)
        {
            return;
        }

        if (type is INamedTypeSymbol named
            && (IsTaskLike(named) || IsApiResult(named) || IsHttpResultWrapper(named)))
        {
            foreach (var argument in named.TypeArguments)
            {
                AddRoot(argument, roots);
            }
            return;
        }

        if (type.SpecialType is SpecialType.System_Object or SpecialType.System_Void
            || type.TypeKind is TypeKind.Error or TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.TypeParameter
            || type.IsRefLikeType
            || IsIResult(type)
            || IsOpenGeneric(type)
            || type is INamedTypeSymbol { IsAnonymousType: true })
        {
            return;
        }

        roots.Add(ToTypeName(type));
    }

    private static bool IsLikelyBodyType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return type is IArrayTypeSymbol;
        }

        var ns = named.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return named.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
            || ns.Contains(".Models", StringComparison.Ordinal)
            || named.GetAttributes().Any(attribute =>
                attribute.AttributeClass?.Name is "ValidatableTypeAttribute" or "FromBodyAttribute");
    }

    private static bool IsInfrastructureType(ITypeSymbol type)
    {
        if (type.SpecialType != SpecialType.None)
        {
            return true;
        }

        if (type is not INamedTypeSymbol named)
        {
            return true;
        }

        var name = named.Name;
        var ns = named.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return name is "HttpContext" or "HttpRequest" or "HttpResponse" or "ClaimsPrincipal"
            or "CancellationToken" or "IServiceProvider"
            || name.EndsWith("Manager", StringComparison.Ordinal)
            || name.EndsWith("Service", StringComparison.Ordinal)
            || ns.StartsWith("Microsoft.Extensions", StringComparison.Ordinal);
    }

    private static bool HasBindingAttribute(IParameterSymbol parameter, string name) =>
        parameter.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == name + "Attribute");

    private static bool IsEndpointType(INamedTypeSymbol type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.Name == "RestEndpointBase")
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAspNetEndpointMethod(IMethodSymbol method)
    {
        var namespaceName = method.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return namespaceName.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal);
    }

    private static bool IsGeneratedTree(SyntaxTree tree)
    {
        var path = tree.FilePath;
        return path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{Path.AltDirectorySeparatorChar}obj{Path.AltDirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTaskLike(INamedTypeSymbol type) =>
        type.Name is "Task" or "ValueTask" && type.TypeArguments.Length == 1;

    private static bool IsApiResult(INamedTypeSymbol type) =>
        type.Name == "ApiResult" && type.TypeArguments.Length == 1 && type.ContainingType?.Name == "RestEndpointBase";

    private static bool IsHttpResultWrapper(INamedTypeSymbol type) =>
        type.ContainingNamespace?.ToDisplayString() == "Microsoft.AspNetCore.Http.HttpResults"
        && type.Name is "Results" or "Ok" or "Created" or "Accepted" or "BadRequest"
            or "NotFound" or "Conflict" or "UnprocessableEntity" or "JsonHttpResult"
            or "CreatedAtRoute" or "AcceptedAtRoute";

    private static bool IsIResult(ITypeSymbol type) => type is INamedTypeSymbol named
        && (named.Name == "IResult" && named.ContainingNamespace?.ToDisplayString() == "Microsoft.AspNetCore.Http"
            || named.AllInterfaces.Any(@interface =>
                @interface.Name == "IResult" && @interface.ContainingNamespace?.ToDisplayString() == "Microsoft.AspNetCore.Http"));

    private static bool IsOpenGeneric(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return IsOpenGeneric(array.ElementType);
        }

        if (type is not INamedTypeSymbol named)
        {
            return type.TypeKind == TypeKind.TypeParameter;
        }

        return named.IsUnboundGenericType
            || named.TypeArguments.Any(IsOpenGeneric)
            || named.ContainingType is not null && IsOpenGeneric(named.ContainingType);
    }

    private static string ToTypeName(ITypeSymbol type) =>
        type.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static IEnumerable<INamedTypeSymbol> GetNamespaceTypes(INamespaceSymbol @namespace)
    {
        foreach (var member in @namespace.GetMembers())
        {
            if (member is INamespaceSymbol nested)
            {
                foreach (var type in GetNamespaceTypes(nested))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;
                foreach (var nestedType in GetNestedTypes(type))
                {
                    yield return nestedType;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (var descendant in GetNestedTypes(nested))
            {
                yield return descendant;
            }
        }
    }

    private static bool IsPubliclyAccessible(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }
        }

        return true;
    }

    private static INamespaceSymbol? FindNamespace(INamespaceSymbol root, string name)
    {
        var current = root;
        foreach (var segment in name.Split('.'))
        {
            current = current.GetNamespaceMembers().FirstOrDefault(candidate => candidate.Name == segment);
            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    private static string UpdateContextFile(
        string path,
        ClassDeclarationSyntax declaration,
        HashSet<string> roots,
        Compilation compilation)
    {
        var source = File.ReadAllText(path);
        var generatedTypes = roots
            .Where(type => !IsExistingContextType(declaration, compilation, type))
            .OrderBy(type => type, StringComparer.Ordinal)
            .ToArray();
        var namingTypes = new HashSet<string>(roots, StringComparer.Ordinal);
        var model = compilation.GetSemanticModel(declaration.SyntaxTree);
        var names = generatedTypes
            .SelectMany(type => Regex.Matches(type, @"global::[A-Za-z_][A-Za-z0-9_.]*").Select(match => match.Value))
            .Select(fullName => fullName[(fullName.LastIndexOf('.') + 1)..])
            .Distinct(StringComparer.Ordinal);
        foreach (var name in names)
        {
            foreach (var visible in model.LookupNamespacesAndTypes(declaration.SpanStart, name: name).OfType<INamedTypeSymbol>())
            {
                namingTypes.Add(ToTypeName(visible));
            }
        }
        var usingBlock = CreateGeneratedUsingBlock(generatedTypes, source, namingTypes, compilation);
        source = InsertGeneratedUsings(source, usingBlock);
        var updatedRoot = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        var updatedDeclaration = updatedRoot.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(candidate => candidate.Identifier.ValueText == declaration.Identifier.ValueText);
        if (updatedDeclaration is null)
        {
            throw new InvalidOperationException($"Cannot locate context class '{declaration.Identifier.ValueText}' after updating generated usings.");
        }

        var block = CreateGeneratedBlock(generatedTypes, namingTypes, compilation);
        var begin = source.IndexOf(GeneratedBegin, StringComparison.Ordinal);
        var end = source.IndexOf(GeneratedEnd, StringComparison.Ordinal);
        if ((begin >= 0) != (end >= 0) || end >= 0 && end < begin
            || begin >= 0 && source.IndexOf(GeneratedBegin, begin + GeneratedBegin.Length, StringComparison.Ordinal) >= 0
            || end >= 0 && source.IndexOf(GeneratedEnd, end + GeneratedEnd.Length, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException($"Invalid generated JSON region in {path}.");
        }

        if (begin >= 0)
        {
            end += GeneratedEnd.Length;
            source = source[..begin] + block + source[end..];
            return source;
        }

        var offset = updatedDeclaration.SpanStart;
        source = source[..offset] + block + Environment.NewLine + source[offset..];
        return source;
    }

    private static bool IsExistingContextType(ClassDeclarationSyntax declaration, Compilation compilation, string typeName)
    {
        var model = compilation.GetSemanticModel(declaration.SyntaxTree);
        var source = declaration.SyntaxTree.GetText().ToString();
        var generatedBegin = source.IndexOf(GeneratedBegin, StringComparison.Ordinal);
        var generatedEnd = source.IndexOf(GeneratedEnd, StringComparison.Ordinal);
        foreach (var attribute in declaration.AttributeLists.SelectMany(list => list.Attributes))
        {
            if (generatedBegin >= 0 && generatedEnd > generatedBegin
                && attribute.SpanStart >= generatedBegin
                && attribute.Span.End <= generatedEnd + GeneratedEnd.Length)
            {
                continue;
            }

            if (!(attribute.Name.ToString().EndsWith("JsonSerializable", StringComparison.Ordinal)
                    || attribute.Name.ToString().EndsWith("JsonSerializableAttribute", StringComparison.Ordinal))
                || attribute.ArgumentList is null)
            {
                continue;
            }

            var typeOf = attribute.ArgumentList.Arguments
                .Select(argument => model.GetOperation(argument.Expression))
                .OfType<ITypeOfOperation>()
                .FirstOrDefault();
            if (typeOf?.TypeOperand is ITypeSymbol type
                && string.Equals(ToTypeName(type), typeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string CreateContextFile(Project project, Compilation compilation, HashSet<string> roots)
    {
        var endpointNamespace = compilation.GetSymbolsWithName("MapEndpoints", SymbolFilter.Member)
            .OfType<IMethodSymbol>()
            .Select(method => method.ContainingNamespace.ToDisplayString())
            .FirstOrDefault(namespaceValue => namespaceValue.EndsWith(".Endpoints", StringComparison.Ordinal));
        var namespaceName = project.DefaultNamespace
            ?? (endpointNamespace is null ? null : endpointNamespace[..^".Endpoints".Length])
            ?? compilation.AssemblyName
            ?? "ApiService";
        if (!namespaceName.Split('.').All(SyntaxFacts.IsValidIdentifier))
        {
            throw new InvalidOperationException($"Cannot generate a JSON context with namespace '{namespaceName}' in {project.FilePath}.");
        }
        var generatedTypes = roots.OrderBy(type => type, StringComparer.Ordinal).ToArray();
        var usingBlock = CreateGeneratedUsingBlock(generatedTypes, string.Empty, roots, compilation);
        return $"{usingBlock}{Environment.NewLine}namespace {namespaceName};{Environment.NewLine}{Environment.NewLine}{CreateGeneratedBlock(generatedTypes, roots, compilation)}{Environment.NewLine}public partial class ApiJsonSerializerContext : JsonSerializerContext{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}";
    }

    private static string CreateGeneratedBlock(
        IEnumerable<string> types,
        IEnumerable<string> allTypes,
        Compilation compilation)
    {
        var distinctTypes = types.Distinct(StringComparer.Ordinal).ToArray();
        var replacements = BuildTypeReplacements(allTypes, compilation);
        var builder = new StringBuilder();
        builder.AppendLine(GeneratedBegin);
        foreach (var type in distinctTypes)
        {
            var renderedType = RenderType(type, replacements);
            var hasAlias = replacements.Any(replacement =>
                replacement.Value.StartsWith("__PerigonJsonType_", StringComparison.Ordinal)
                && type.Contains(replacement.Key, StringComparison.Ordinal));
            var propertyName = hasAlias
                ? $", TypeInfoPropertyName = \"{GetTypeInfoPropertyName(type)}\""
                : string.Empty;
            builder.AppendLine($"[JsonSerializable(typeof({renderedType}){propertyName})]");
        }
        builder.Append(GeneratedEnd);
        return builder.ToString();
    }

    private static string CreateGeneratedUsingBlock(
        IEnumerable<string> types,
        string source,
        IEnumerable<string> allTypes,
        Compilation compilation)
    {
        var generatedTypes = types.ToArray();
        var replacements = BuildTypeReplacements(allTypes, compilation)
            .Where(pair => generatedTypes.Any(type => type.Contains(pair.Key, StringComparison.Ordinal)))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var sourceWithoutGeneratedUsings = source;
        var generatedBegin = source.IndexOf(GeneratedUsingBegin, StringComparison.Ordinal);
        var generatedEnd = source.IndexOf(GeneratedUsingEnd, StringComparison.Ordinal);
        if (generatedBegin >= 0 && generatedEnd > generatedBegin)
        {
            sourceWithoutGeneratedUsings = source[..generatedBegin]
                + source[(generatedEnd + GeneratedUsingEnd.Length)..];
        }

        var existingUsings = Regex.Matches(sourceWithoutGeneratedUsings, @"(?m)^\s*using\s+([A-Za-z_][A-Za-z0-9_.]*)\s*;\s*$")
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
        var lines = replacements
            .Select(pair =>
            {
                if (pair.Value.StartsWith("__PerigonJsonType_", StringComparison.Ordinal))
                {
                    return $"using {pair.Value} = {pair.Key["global::".Length..]};";
                }

                var namespaceName = GetContainingNamespace(pair.Key, compilation);
                return $"using {namespaceName}";
            })
            .Append("using System.Text.Json.Serialization;")
            .Select(line => line.EndsWith(";", StringComparison.Ordinal) ? line : line + ";")
            .Where(line => !line.StartsWith("using ", StringComparison.Ordinal)
                || !existingUsings.Contains(line["using ".Length..^1]))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToArray();
        return string.Join(
            Environment.NewLine,
            new[] { GeneratedUsingBegin }.Concat(lines).Concat(new[] { GeneratedUsingEnd }));
    }

    private static string InsertGeneratedUsings(string source, string usingBlock)
    {
        if (string.IsNullOrWhiteSpace(usingBlock))
        {
            return source;
        }

        var begin = source.IndexOf(GeneratedUsingBegin, StringComparison.Ordinal);
        var end = source.IndexOf(GeneratedUsingEnd, StringComparison.Ordinal);
        if ((begin >= 0) != (end >= 0) || end >= 0 && end < begin
            || begin >= 0 && source.IndexOf(GeneratedUsingBegin, begin + GeneratedUsingBegin.Length, StringComparison.Ordinal) >= 0
            || end >= 0 && source.IndexOf(GeneratedUsingEnd, end + GeneratedUsingEnd.Length, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException("Invalid generated JSON using region.");
        }

        if (begin >= 0)
        {
            end += GeneratedUsingEnd.Length;
            return source[..begin] + usingBlock + source[end..];
        }

        var syntaxRoot = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        var namespaceIndex = syntaxRoot.Members
            .OfType<BaseNamespaceDeclarationSyntax>()
            // SpanStart excludes leading trivia. FullSpan.Start can point into a
            // leading newline/comment and split the first using directive when
            // the source was created from a raw string or has a BOM.
            .Select(member => member.SpanStart)
            .FirstOrDefault(-1);
        if (namespaceIndex < 0)
        {
            namespaceIndex = syntaxRoot.Members.FirstOrDefault()?.SpanStart ?? source.Length;
        }

        return source.Insert(namespaceIndex, usingBlock + Environment.NewLine + Environment.NewLine);
    }

    private static string RenderType(string type, IReadOnlyDictionary<string, string> replacements)
    {
        var renderedType = type;
        foreach (var replacement in replacements.OrderByDescending(item => item.Key.Length))
        {
            renderedType = renderedType.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        return renderedType;
    }

    private static Dictionary<string, string> BuildTypeReplacements(
        IEnumerable<string> types,
        Compilation compilation)
    {
        var fullNames = types
            .SelectMany(type => Regex.Matches(type, @"global::[A-Za-z_][A-Za-z0-9_.]*")
                .Select(match => match.Value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var conflictingNamespaces = fullNames
            .GroupBy(fullName => GetTypeReferenceName(fullName, compilation), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(fullName => GetContainingNamespace(fullName, compilation)))
            .ToHashSet(StringComparer.Ordinal);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in fullNames.GroupBy(
            fullName => GetTypeReferenceName(fullName, compilation),
            StringComparer.Ordinal))
        {
            if (group.Count() == 1
                && !conflictingNamespaces.Contains(GetContainingNamespace(group.Single(), compilation)))
            {
                result[group.Single()] = group.Key;
                continue;
            }

            foreach (var fullName in group)
            {
                result[fullName] = "__PerigonJsonType_" + GetTypeInfoPropertyName(fullName)[^12..];
            }
        }

        return result;
    }

    private static string GetTypeReferenceName(string fullName, Compilation compilation)
    {
        var symbol = ResolveNamedType(compilation, fullName);
        if (symbol is null)
        {
            return fullName[(fullName.LastIndexOf('.') + 1)..];
        }

        var nestedNames = new Stack<string>();
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            nestedNames.Push(current.Name);
        }

        return string.Join('.', nestedNames);
    }

    private static string GetContainingNamespace(string fullName, Compilation compilation)
    {
        var symbol = ResolveNamedType(compilation, fullName);
        if (symbol?.ContainingNamespace is { IsGlobalNamespace: false } @namespace)
        {
            return @namespace.ToDisplayString();
        }

        var simpleName = fullName["global::".Length..];
        return simpleName[..simpleName.LastIndexOf('.')];
    }

    private static INamedTypeSymbol? ResolveNamedType(Compilation compilation, string fullName)
    {
        var metadataName = fullName["global::".Length..];
        while (true)
        {
            for (var arity = 0; arity <= 4; arity++)
            {
                var candidate = arity == 0 ? metadataName : $"{metadataName}`{arity}";
                if (compilation.GetTypeByMetadataName(candidate) is { } symbol)
                    return symbol;
            }

            var separator = metadataName.LastIndexOf('.');
            if (separator < 0)
                return null;
            metadataName = metadataName[..separator] + "+" + metadataName[(separator + 1)..];
        }
    }

    private static string GetTypeInfoPropertyName(string typeName)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(typeName)));
        return $"__PerigonJson_{hash}";
    }
}
