using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ater.Common.Options;
public class AWSS3Option
{
    public const string ConfigPath = "AWSS3";
    public required string Endpoint { get; set; }
    public required string AccessKeyId { get; set; }
    public required string AccessKeySecret { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? Prefix { get; set; }
}
