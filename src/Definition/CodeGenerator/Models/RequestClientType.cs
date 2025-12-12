using System.ComponentModel;

public enum RequestClientType
{
    [Description("angular http")] NgHttp,
    [Description("axios")] Axios,
    [Description("csharp")] CSharp,
}