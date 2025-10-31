namespace Ater.AspNetCore.Toolkit.Options;

public class SMSOption
{
    public const string ConfigPath = "SMS";
    public required string AccessKeyId { get; set; }
    public required string AccessKeySecret { get; set; }

    public required string Sign { get; set; }
}
