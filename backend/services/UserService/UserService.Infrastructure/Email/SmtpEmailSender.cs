using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var host = _config["Smtp:Host"];
        if (string.IsNullOrEmpty(host))
        {
            _logger.LogInformation("SMTP not configured — writing reset email to log. To={To} Subject={Subject} Body={Body}", to, subject, htmlBody);
            return Task.CompletedTask;
        }

        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 25;
        var user = _config["Smtp:Username"];
        var pass = _config["Smtp:Password"];
        var from = _config["Smtp:From"] ?? user ?? "no-reply@example.com";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true
        };
        if (!string.IsNullOrEmpty(user)) client.Credentials = new NetworkCredential(user, pass);

        var mail = new MailMessage(from, to, subject, htmlBody) { IsBodyHtml = true };
        return client.SendMailAsync(mail);
    }
}
