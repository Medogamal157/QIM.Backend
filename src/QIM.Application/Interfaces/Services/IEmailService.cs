namespace QIM.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string htmlMessage, CancellationToken ct = default);
    Task SendBulkEmailAsync(string[] emails, string subject, string htmlMessage, CancellationToken ct = default);
    Task SendEmailWithAttachmentsAsync(string email, string subject, string htmlMessage, IEnumerable<string> attachmentPaths, CancellationToken ct = default);
}
