using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

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

    public static string FormatChanges(SyntaxNode node)
    {
        AdhocWorkspace workspace = new();
        Microsoft.CodeAnalysis.Options.OptionSet options = workspace
            .Options
            // change these values to fit your environment / preferences
            .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, value: true)
            .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, value: "\r\n");
        return Formatter.Format(node, workspace, options).ToFullString();
    }

    /// <summary>
    /// 内容节点编辑
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <param name="editor"></param>
    /// <param name="node"></param>
    /// <param name="replacementNode"></param>
    public static void ReplaceNodeUsing<TNode>(
        DocumentEditor editor,
        TNode node,
        Func<TNode, SyntaxNode> replacementNode
    )
        where TNode : SyntaxNode
    {
        editor.ReplaceNode(node, (n, _) => replacementNode((TNode)n));
    }
}
