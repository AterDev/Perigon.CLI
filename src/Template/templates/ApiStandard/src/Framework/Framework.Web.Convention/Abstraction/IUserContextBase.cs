namespace Framework.Web.Convention.Abstraction;
public interface IUserContextBase
{
    public Guid UserId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; set; }
    public bool IsAdmin { get; init; }
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }
    public Guid? GroupId { get; init; }
    public bool IsRole(string roleName);
}
