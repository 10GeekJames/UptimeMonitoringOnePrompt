using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace UptimeService;

public interface ISmtpAlertService
{
    Task SendFailureAlertAsync(string endpointUrl, int statusCode, string errorMessage);
}

public class SmtpAlertService : ISmtpAlertService
{
    private readonly SmtpConfiguration _config;
    private readonly ILogger<SmtpAlertService> _logger;

    public SmtpAlertService(Configuration configuration, ILogger<SmtpAlertService> logger)
    {
        _config = configuration.Smtp;
        _logger = logger;
    }

    public async Task SendFailureAlertAsync(string endpointUrl, int statusCode, string errorMessage)
    {
        if (!_config.Enabled)
        {
            _logger.LogDebug("SMTP alerts disabled, skipping alert for {Url}", endpointUrl);
            return;
        }

        if (!_config.RecipientList.Any())
        {
            _logger.LogWarning("No recipients configured for SMTP alerts");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config.FromName, _config.FromAddress));

            foreach (var recipient in _config.RecipientList)
            {
                message.To.Add(new MailboxAddress("", recipient));
            }

            message.Subject = $"⚠️ Uptime Alert: {endpointUrl} is DOWN";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"""
                UPTIME MONITOR ALERT
                
                The following endpoint has failed:
                
                URL: {endpointUrl}
                Status Code: {statusCode}
                Error: {errorMessage}
                Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                
                Please investigate the issue immediately.
                
                --
                Uptime Monitor Service
                """,
                
                HtmlBody = $"""
                <html>
                <body>
                    <h2 style="color: red;">⚠️ UPTIME MONITOR ALERT</h2>
                    <p>The following endpoint has failed:</p>
                    
                    <table border="1" style="border-collapse: collapse;">
                        <tr><td><strong>URL</strong></td><td>{endpointUrl}</td></tr>
                        <tr><td><strong>Status Code</strong></td><td>{statusCode}</td></tr>
                        <tr><td><strong>Error</strong></td><td>{errorMessage}</td></tr>
                        <tr><td><strong>Time</strong></td><td>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</td></tr>
                    </table>
                    
                    <p><strong>Please investigate the issue immediately.</strong></p>
                    
                    <hr>
                    <p><em>Uptime Monitor Service</em></p>
                </body>
                </html>
                """
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, 
                _config.UseSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(_config.SmtpUser) && !string.IsNullOrEmpty(_config.SmtpPass))
            {
                await client.AuthenticateAsync(_config.SmtpUser, _config.SmtpPass);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Failure alert sent successfully for {Url} to {Recipients}", 
                endpointUrl, string.Join(", ", _config.RecipientList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send failure alert for {Url}", endpointUrl);
            throw;
        }
    }
}