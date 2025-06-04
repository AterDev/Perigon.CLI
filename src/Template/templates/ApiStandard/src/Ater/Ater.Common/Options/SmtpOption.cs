namespace Ater.Common.Options;

/// <summary>
/// SMTP服务选项
/// </summary>
public class SmtpOption
{
    public const string ConfigPath = "Smtp";

    public required string Host { get; set; }
    public int Port { get; set; } = 25;
    public required string DisplayName { get; set; }
    public required string From { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool EnableSsl { get; set; } = true;
}
