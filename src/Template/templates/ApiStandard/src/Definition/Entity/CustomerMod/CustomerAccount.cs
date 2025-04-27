using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entity.CustomerMod;
/// <summary>
/// 客户账号
/// </summary>
public class CustomerAccount : EntityBase
{
    public Guid CustomerInfoId { get; set; } = default!;
    public CustomerInfo CustomerInfo { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(40)]
    [Length(2, 40)]

    public required string UserName { get; set; }
    [JsonIgnore]
    [MaxLength(100)]
    public string PasswordHash { get; set; } = default!;
    [JsonIgnore]
    [MaxLength(60)]
    public string PasswordSalt { get; set; } = default!;
}
