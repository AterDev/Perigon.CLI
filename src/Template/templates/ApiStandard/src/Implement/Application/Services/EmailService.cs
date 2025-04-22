using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Configuration;

using MimeKit;
using MimeKit.Text;

using Share.Options;

namespace Application.Services;

public interface IEmailService
{
    Task SendAsync(string email, string subject, string html);

    /// <summary>
    /// 发送注册验证码
    /// </summary>
    /// <param name="email"></param>
    /// <param name="verifyCode"></param>
    /// <returns></returns>
    Task SendRegisterVerifyAsync(string email, string verifyCode);
    /// <summary>
    /// 发送注册结果
    /// </summary>
    /// <param name="email"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task SendRegResultAsync(string email, string content);

    Task SendLoginVerifyAsync(string email, string verifyCode);
}

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _config = configuration;

    public async Task SendAsync(string email, string subject, string html)
    {
        SmtpOption option = _config.GetSection("Smtp").Get<SmtpOption>()
                            ?? throw new ArgumentNullException("未找到Smtp选项!");
        // create message 
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(option.From));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = html };

        // send email
        var smtp = new SmtpClient();
        smtp.Connect(option.Host, option.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(option.Username, option.Password);
        await smtp.SendAsync(message);
        smtp.Disconnect(true);
    }

    /// <summary>
    /// 发送注册验证码
    /// </summary>
    /// <param name="email"></param>
    /// <param name="verifyCode"></param>
    public async Task SendRegisterVerifyAsync(string email, string verifyCode)
    {
        var html = @$"<p>欢迎您注册成为MyProjectName网站的会员！</p>
<p>您的验证码为：</p>
<h3>
    <span style='padding:8px;color:white;background-color: rgb(0, 90, 226); border-radius: 5px;'>
        {verifyCode}
    </span>
</h3>
<p>验证码有效期为5分钟。</p>";
        await SendAsync(email, "【MyProjectName】注册验证码", html);
    }

    /// <summary>
    /// 发送注册结果
    /// </summary>
    /// <param name="email"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task SendRegResultAsync(string email, string content)
    {
        var html = @$"<p>感谢您注册成为MyProjectName网站的会员！</p>
<p>您的注册结果为：</p>
<h3>
    <span style='padding:8px;color:white;background-color: rgb(0, 90, 226); border-radius: 5px;'>
        {content}
    </span>
</h3>";
        await SendAsync(email, "【MyProjectName】注册结果", html);
    }

    /// <summary>
    /// 发送登录验证码
    /// </summary>
    /// <param name="email"></param>
    /// <param name="verifyCode"></param>
    /// <returns></returns>
    public async Task SendLoginVerifyAsync(string email, string verifyCode)
    {
        var html = @$"<p>您正在登录 MyProjectName网站!</p>
<p>您的验证码为：</p>
<h3>
    <span style='padding:8px;color:white;background-color: rgb(0, 90, 226); border-radius: 5px;'>
        {verifyCode}
    </span>
</h3>
<p>验证码有效期为5分钟。</p>";
        await SendAsync(email, "【MyProjectName】登录验证码", html);
    }
}