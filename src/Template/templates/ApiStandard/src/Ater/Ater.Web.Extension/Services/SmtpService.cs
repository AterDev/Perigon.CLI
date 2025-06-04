using Ater.Common.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Ater.Web.Extension.Services;
public class SmtpService(IOptions<SmtpOption> options)
{
    public async Task SendAsync(string email, string subject, string html)
    {
        SmtpOption option = options.Value ?? throw new ArgumentNullException($"cant find Smtp option: {SmtpOption.ConfigPath}");

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

}
