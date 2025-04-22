using EntityFramework.DBProvider;
using Framework.Common.Models;
using Framework.Web.Convention;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Application.Implement;

public class UserContext : IUserContext
{
    /// <summary>
    /// 用户id
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// 组织id
    /// </summary>
    public Guid? GroupId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; set; }
    /// <summary>
    /// 是否为管理员
    /// </summary>
    public bool IsAdmin { get; init; }
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CommandDbContext _context;
    public UserContext(IHttpContextAccessor httpContextAccessor, CommandDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        if (Guid.TryParse(FindClaim(ClaimTypes.NameIdentifier)?.Value, out Guid userId) && userId != Guid.Empty)
        {
            UserId = userId;
        }
        if (Guid.TryParse(FindClaim(ClaimTypes.GroupSid)?.Value, out Guid groupSid) && groupSid != Guid.Empty)
        {
            GroupId = groupSid;
        }
        Username = FindClaim(ClaimTypes.Name)?.Value;
        Email = FindClaim(ClaimTypes.Email)?.Value;

        CurrentRole = FindClaim(ClaimTypes.Role)?.Value;

        Roles = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value).ToList();
        if (Roles != null)
        {
            IsAdmin = Roles.Any(r => r.Equals(WebConst.AdminUser) || r.Equals(WebConst.SuperAdmin));
        }
    }

    protected Claim? FindClaim(string claimType)
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst(claimType);
    }

    /// <summary>
    /// 判断当前角色
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public bool IsRole(string roleName)
    {
        return Roles != null && Roles.Any(r => r == roleName);
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExistAsync<TUser>() where TUser : class, IEntityBase
    {
        return IsAdmin ?
            await _context.Set<TUser>().AnyAsync(u => u.Id == UserId) :
            await _context.Set<TUser>().AnyAsync(u => u.Id == UserId);
    }

    /// <summary>
    /// 获取ip地址
    /// </summary>
    /// <returns></returns>
    public string? GetIpAddress()
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        return request == null
            ? string.Empty
            : request.Headers.TryGetValue("X-Forwarded-For", out StringValues value) ? value.Where(x => x != null).FirstOrDefault()
            : _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString();
    }

    public async Task<TUser?> GetUserAsync<TUser>() where TUser : class, IEntityBase
    {
        return await _context.Set<TUser>().FindAsync(UserId);
    }

    public HttpContext? GetHttpContext()
    {
        return _httpContextAccessor.HttpContext;
    }
}
