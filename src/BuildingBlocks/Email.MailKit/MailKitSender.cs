﻿using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Shared.Email;

namespace Email.MailKit;

public class MailKitSender : IEmailSender
{
    private readonly ILogger<MailKitSender> _logger;
    private readonly MailKitOptions _mailkitOptions;
    private readonly SmtpClient _smtpClient;

    public MailKitSender(SmtpClient smtpClient, IOptionsMonitor<MailKitOptions> mailkitOptions, ILogger<MailKitSender> logger)
    {
        _mailkitOptions = mailkitOptions.CurrentValue;
        _smtpClient = smtpClient;
        _logger = logger;
    }

    public async Task SendAsync(EmailData email, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("MailKitSender - Connecting to Server={Host}:{Port}", _mailkitOptions.SmtpHost, _mailkitOptions.Port);

            await _smtpClient.ConnectAsync(_mailkitOptions.SmtpHost, _mailkitOptions.Port, false, cancellationToken);

            if (!string.IsNullOrEmpty(_mailkitOptions.Username))
            {
                var credential = new NetworkCredential(_mailkitOptions.Username, _mailkitOptions.Password);

                await _smtpClient.AuthenticateAsync(new SaslMechanismPlain(credential), cancellationToken);
            }

            await _smtpClient.SendAsync(CreateMailMessage(email), cancellationToken);
        }
        finally
        {
            _logger.LogInformation("MailKitSender - Disconnect to Server={Host}:{Port}", _mailkitOptions.SmtpHost, _mailkitOptions.Port);

            await _smtpClient.DisconnectAsync(true, cancellationToken);
        }
    }

    private MimeMessage CreateMailMessage(EmailData email)
    {
        var message = new MimeMessage
        {
            Subject = email.Subject
        };

        message.From.Add(new MailboxAddress(_mailkitOptions.MailBoxName, _mailkitOptions.MailBoxAddress));

        email.ToAddress.ForEach(x => { message.To.Add(new MailboxAddress(x, x)); });
        email.CcAddress.ForEach(x => { message.Cc.Add(new MailboxAddress(x, x)); });
        email.BccAddress.ForEach(x => { message.Bcc.Add(new MailboxAddress(x, x)); });

        message.Body = new TextPart(email.IsHtml ? TextFormat.Html : TextFormat.Plain) { Text = email.Body };

        message.Priority = email.Priority switch
        {
            Priority.High => MessagePriority.Urgent,
            Priority.Low => MessagePriority.NonUrgent,
            _ => MessagePriority.Normal
        };

        return message;
    }
}