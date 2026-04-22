using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using QIM.Application.Common.Settings;
using QIM.Application.Interfaces.Services;

namespace QIM.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly MailSettings _mailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
    {
        _mailSettings = mailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage, CancellationToken ct = default)
    {
        if (!IsValidEmail(email))
        {
            _logger.LogWarning("Skipping email send: invalid recipient {Email}", email);
            return;
        }

        var message = BuildMessage(subject, htmlMessage);
        message.To.Add(MailboxAddress.Parse(email));
        await SendAsync(message, ct);
    }

    public async Task SendBulkEmailAsync(string[] emails, string subject, string htmlMessage, CancellationToken ct = default)
    {
        var message = BuildMessage(subject, htmlMessage);
        foreach (var e in emails)
        {
            if (IsValidEmail(e))
                message.To.Add(MailboxAddress.Parse(e));
        }

        if (message.To.Count == 0)
            return;

        await SendAsync(message, ct);
    }

    public async Task SendEmailWithAttachmentsAsync(string email, string subject, string htmlMessage, IEnumerable<string> attachmentPaths, CancellationToken ct = default)
    {
        if (!IsValidEmail(email))
            return;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
        foreach (var path in attachmentPaths)
        {
            if (File.Exists(path))
                await bodyBuilder.Attachments.AddAsync(path, ct);
        }

        message.Body = bodyBuilder.ToMessageBody();
        await SendAsync(message, ct);
    }

    private MimeMessage BuildMessage(string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();
        return message;
    }

    private async Task SendAsync(MimeMessage message, CancellationToken ct)
    {
        try
        {
            using var client = new SmtpClient();

            var secureOption = _mailSettings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, secureOption, ct);
            await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent to {Recipients} with subject {Subject}",
                string.Join(",", message.To.Mailboxes.Select(m => m.Address)), message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with subject {Subject}", message.Subject);
            throw;
        }
    }

    private static bool IsValidEmail(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;
        try
        {
            var addr = MailboxAddress.Parse(address);
            return string.Equals(addr.Address, address, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
