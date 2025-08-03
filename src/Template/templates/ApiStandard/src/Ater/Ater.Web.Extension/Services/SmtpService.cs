using Ater.Common.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Ater.Web.Extension.Services;

public class SmtpService
{
    private readonly IOptions<SmtpOption> options;

    public SmtpService(IOptions<SmtpOption> options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        this.options = options;
    }

    public async Task SendAsync(string email, string subject, string html)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(this.options.Value.From));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = html };

        // send email
        var smtp = new SmtpClient();
        smtp.Connect(
            this.options.Value.Host,
            this.options.Value.Port,
            SecureSocketOptions.StartTls
        );
        smtp.Authenticate(this.options.Value.Username, this.options.Value.Password);
        await smtp.SendAsync(message);
        smtp.Disconnect(true);
    }
}
