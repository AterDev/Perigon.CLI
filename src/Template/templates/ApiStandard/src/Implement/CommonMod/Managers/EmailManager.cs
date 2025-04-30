using Framework.Web.Extension.Services;

namespace CommonMod.Managers;
public class EmailManager(ILogger<EmailManager> logger, SmtpService smtp) : ManagerBase(logger)
{
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
        await smtp.SendAsync(email, "【MyProjectName】注册验证码", html);
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
        await smtp.SendAsync(email, "【MyProjectName】注册结果", html);
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
        await smtp.SendAsync(email, "【MyProjectName】登录验证码", html);
    }
}
