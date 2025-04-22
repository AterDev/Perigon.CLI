using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Definition.Entity;

/// <summary>
/// 账号
/// </summary>
public class Account : EntityBase
{
    [MaxLength(100)]
    public string UserName { get; set; }
    [MaxLength(100)]
    public string HashPassword { get; set; }
    [MaxLength(100)]
    public string HashSalt { get; set; }
}
