using Microsoft.AspNetCore.Http;

namespace Ater.Web.Convention.Abstraction;

public interface IUserContext
{
    /// <summary>
    /// 用户ID
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// 组织ID
    /// </summary>
    Guid? GroupId { get; }

    Guid? TenantId { get; }

    /// <summary>
    /// 用户名
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 邮箱
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// 是否为管理员
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// 当前角色
    /// </summary>
    string? CurrentRole { get; }

    /// <summary>
    /// 所有角色
    /// </summary>
    IReadOnlyList<string>? Roles { get; }

    public HttpContext? HttpContext { get; set; }

    /// <summary>
    /// 判断当前用户是否属于指定角色
    /// </summary>
    bool IsRole(string roleName);
}
