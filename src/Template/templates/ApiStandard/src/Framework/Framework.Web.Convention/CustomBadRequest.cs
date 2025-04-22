using Microsoft.AspNetCore.Mvc;

namespace Framework.Web.Convention;

public class CustomBadRequest : ObjectResult
{
    public CustomBadRequest(ActionContext context, object? value) : base(value)
    {
        StatusCode = 400;
        Value = new
        {
            Title = "请求参数错误",
            Detail = GetErrorMessage(context),
            Status = 400,
            TraceId = context.HttpContext.TraceIdentifier
        };
    }

    private static string GetErrorMessage(ActionContext context)
    {
        var errorMessages = context.ModelState.Values.Select(x => x.Errors.FirstOrDefault()?.ErrorMessage).ToList();

        return string.Join(";",
            errorMessages.Where(e => !string.IsNullOrEmpty(e)).ToArray());

    }
}
