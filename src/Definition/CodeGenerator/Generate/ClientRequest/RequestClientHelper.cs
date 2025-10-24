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
        var codeWriter = new CodeWriter();

        foreach (var item in schemas)
        {
            if (item.Value.Enum?.Count > 0)
            {
                var switchCode = ToEnumSwitchString(OpenApiHelper.FormatSchemaKey(item.Key), item.Value);
                codeWriter.AppendLine(switchCode);
            }
        }

        var genContext = new RazorGenContext();
        var model = new CommonViewModel { Content = codeWriter.ToString() };
        return genContext.GenCode(tplContent, model);
    }

    public static string ToEnumSwitchString(string enumType, IOpenApiSchema schema)
    {
        var enumProps = OpenApiHelper.GetEnumProperties(schema);
        if (enumProps == null || enumProps.Count == 0) return string.Empty;

        var codeWriter = new CodeWriter();
        codeWriter.Indent().Indent().Indent();
        codeWriter.AppendLine($"case '{enumType}':");
        codeWriter.Indent();
        codeWriter.AppendLine("switch (value) {");
        codeWriter.Indent();

        foreach (var prop in enumProps)
        {
            codeWriter.AppendLine($"case {prop.DefaultValue}: result = '{prop.CommentSummary}'; break;");
        }

        codeWriter.AppendLine("default: result = '默认'; break;");
        codeWriter.Unindent();
        codeWriter.AppendLine("}");
        codeWriter.AppendLine("break;");

        return codeWriter.ToString();
    }
}
