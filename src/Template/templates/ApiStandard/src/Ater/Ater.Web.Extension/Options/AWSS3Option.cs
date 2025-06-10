namespace Ater.Web.Extension.Options;
public class AWSS3Option
{
    public const string SectionName = "AWSS3";
    public required string Endpoint { get; set; }
    public required string AccessKeyId { get; set; }
    public required string AccessKeySecret { get; set; }
    public string BucketName { get; set; } = string.Empty;
}
