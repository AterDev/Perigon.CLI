using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity;
/// <summary>
/// tenant 
/// </summary>
public class Tenant : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? QueryDbString { get; set; }

    [MaxLength(500)]
    public string? CommandDbString { get; set; }
}

