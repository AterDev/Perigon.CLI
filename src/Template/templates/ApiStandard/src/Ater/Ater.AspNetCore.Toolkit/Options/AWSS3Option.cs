namespace Ater.AspNetCore.Toolkit.Options;

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
