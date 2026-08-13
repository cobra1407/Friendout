using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Strategy pattern — concrete email delivery strategy using MailKit.
///
/// Smtp configuration is validated at startup via SmtpOptions + ValidateOnStart,
/// so no runtime config checks are needed here.
///
/// Uses IServiceScopeFactory to resolve scoped services safely in fire-and-forget contexts.
/// </summary>
public class EmailNotificationStrategy : INotificationStrategy
{
    private readonly SmtpOptions _smtp;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INotificationTemplateProvider _templateProvider;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<EmailNotificationStrategy> _logger;

    public string StrategyName => "Email";

    public EmailNotificationStrategy(
        IOptions<SmtpOptions> smtpOptions,
        IServiceScopeFactory scopeFactory,
        INotificationTemplateProvider templateProvider,
        ITemplateEngine templateEngine,
        ILogger<EmailNotificationStrategy> logger)
    {
        _smtp             = smtpOptions.Value;
        _scopeFactory     = scopeFactory;
        _templateProvider = templateProvider;
        _templateEngine   = templateEngine;
        _logger           = logger;
    }

    public async Task SendAsync(string userId, NotificationType type, Dictionary<string, string> data)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var appLog            = scope.ServiceProvider.GetRequiredService<IAppLogService>();

        // Resolve recipient email.
        // Allow overriding recipient directly in data (e.g. access request emails
        // where the user has no account yet).
        string recipientEmail;
        if (data.TryGetValue("RecipientEmail", out var overrideEmail) && !string.IsNullOrEmpty(overrideEmail))
        {
            recipientEmail = overrideEmail;
        }
        else
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var emailResult = await userService.GetUserEmailAsync(userId);
            if (!emailResult.IsSuccess || string.IsNullOrEmpty(emailResult.Data))
            {
                _logger.LogWarning("Cannot send email notification: no email found for user {UserId}", userId);
                return;
            }
            recipientEmail = emailResult.Data;
        }

        // Render the template in the user's locale (defaults to "en").
        data.TryGetValue("Locale", out var locale);
        var rawTemplate = await _templateProvider.GetTemplateAsync(type, locale ?? "en");
        var body        = _templateEngine.Render(rawTemplate, data);

        // Build the MIME message.
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = GetSubject(type, locale);
        message.Body    = new TextPart("html") { Text = body };

        // Send via MailKit SMTP client.
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtp.Server, _smtp.Port,
                _smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(_smtp.UserName))
                await client.AuthenticateAsync(_smtp.UserName, _smtp.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email ({Type}) sent to {Email}", type, recipientEmail);
        }
        catch (Exception ex)
        {
            var errorMessage = $"SMTP delivery failed for {type} to {recipientEmail}: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            await appLog.LogErrorAsync("Notifications", errorMessage, ex);
            // Do not re-throw — SMTP errors must never surface to the caller.
        }
    }

    private static string GetSubject(NotificationType type, string? locale = "en")
    {
        var isFr = locale?.ToLower() == "fr";

        if (isFr)
        {
            return type switch
            {
                NotificationType.AccessRequestApproved => "Votre accès à Friendout a été approuvé",
                NotificationType.AccessRequestDenied   => "Mise à jour de votre demande d'accès à Friendout",
                NotificationType.AccessRequestReceived => "Nouvelle demande d'accès à Friendout",
                NotificationType.ActivityModified      => "Une activité à laquelle vous avez participé a été mise à jour",
                NotificationType.ActivityCanceled      => "Une activité a été annulée",
                NotificationType.ActivityReminder      => "Rappel : activité à venir",
                NotificationType.InvitationReceived    => "Vous avez été invité à une activité",
                NotificationType.AccountDeleted        => "Votre compte Friendout a été supprimé",
                _                                      => "Notification de Friendout"
            };
        }

        return type switch
        {
            NotificationType.AccessRequestApproved => "Your Friendout access has been approved",
            NotificationType.AccessRequestDenied   => "Update on your Friendout access request",
            NotificationType.AccessRequestReceived => "New Friendout access request",
            NotificationType.ActivityModified      => "An activity you joined has been updated",
            NotificationType.ActivityCanceled      => "An activity has been canceled",
            NotificationType.ActivityReminder      => "Reminder: upcoming activity",
            NotificationType.InvitationReceived    => "You've been invited to an activity",
            NotificationType.AccountDeleted        => "Your Friendout account has been deleted",
            _                                      => "Notification from Friendout"
        };
    }
}
