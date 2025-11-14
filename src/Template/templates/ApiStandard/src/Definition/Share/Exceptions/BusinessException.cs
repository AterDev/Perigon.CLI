using Microsoft.AspNetCore.Http;

namespace Share.Exceptions;

public class BusinessException(
    string errorCode,
    int statusCodes = StatusCodes.Status500InternalServerError
) : Exception()
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCodes { get; } = statusCodes;
}
