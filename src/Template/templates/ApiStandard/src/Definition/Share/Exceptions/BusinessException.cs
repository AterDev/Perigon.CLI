using Microsoft.AspNetCore.Http;

namespace Share.Exceptions;

/// <summary>
/// throw new BusinessException when business error occurs
/// </summary>
/// <param name="errorCode">the key of language const</param>
/// <param name="statusCodes"></param>
public class BusinessException(
    string errorCode,
    int statusCodes = StatusCodes.Status500InternalServerError
) : Exception()
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCodes { get; } = statusCodes;
}
