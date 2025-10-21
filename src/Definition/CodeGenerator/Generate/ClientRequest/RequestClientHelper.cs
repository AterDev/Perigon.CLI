namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// 提供与请求客户端生成相关的静态辅助方法: 基础服务模板、枚举Pipe、枚举函数等。
/// 原先位于 RequestGenerate 中, 已拆分实现类后保留静态方法供调用方使用。
/// </summary>
public static class RequestClientHelper
{
    /// <summary>
    /// 获取基础服务模板内容
    /// </summary>
    public static string GetBaseService(RequestClientType libType)
    {
        try
        {
            return libType switch
            {
                RequestClientType.NgHttp => GenerateBase.GetTplContent("angular.base.service.tpl"),
                RequestClientType.Axios => GenerateBase.GetTplContent("RequestService.axios.service.tpl"),
                _ => string.Empty,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("request base service:" + ex.Message + ex.StackTrace + ex.InnerException);
            return string.Empty;
        }
    }

    public static string GetEnumPipeContent(IDictionary<string, IOpenApiSchema> schemas, bool isNgModule = false)
    {
        string tplContent = TplContent.EnumPipeTpl(isNgModule);
        StringBuilder codeBlocks = new();
        foreach (var item in schemas)
        {
            if (item.Value.Enum?.Count > 0)
            {
                codeBlocks.Append(ToEnumSwitchString(OpenApiHelper.FormatSchemaKey(item.Key), item.Value));
            }
        }
        var genContext = new RazorGenContext();
        var model = new CommonViewModel { Content = codeBlocks.ToString() };
        return genContext.GenCode(tplContent, model);
    }

    public static string GetEnumFunctionContent(IDictionary<string, IOpenApiSchema> schemas)
    {
        var cw = new CodeGenerator.Generate.Helper.TsCodeWriter();
        cw.AppendLine("export default function enumToString(value: number, type: string): string {").Indent();
        cw.AppendLine("let result = \"\";");
        cw.AppendLine("switch (type) {").Indent();
        foreach (var item in schemas)
        {
            if (item.Value.Enum?.Count > 0)
            {
                foreach (var line in ToEnumSwitchString(item.Key, item.Value).Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line)) cw.AppendLine(line);
                }
            }
        }
        cw.AppendLine("default:").Indent();
        cw.AppendLine("break;");
        cw.Unindent(); // default block end
        cw.Unindent().AppendLine("}"); // switch end
        cw.AppendLine("return result;");
        cw.Unindent().AppendLine("}");
        return cw.ToString();
    }

    public static string ToEnumSwitchString(string enumType, IOpenApiSchema schema)
    {
        var enumProps = OpenApiHelper.GetEnumProperties(schema);
        if (enumProps == null || enumProps.Count == 0) return string.Empty;
                var sb = new System.Text.StringBuilder();
                foreach (var prop in enumProps)
                {
                        sb.AppendLine($"case {prop.DefaultValue}: result = '{prop.CommentSummary}'; break;");
                }
                sb.AppendLine("default: result = '默认'; break;");
                return $"case '{enumType}':\n  switch (value) {{\n{sb}  }}\n  break;";
    }
}
