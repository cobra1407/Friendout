using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Strategy pattern — concrete email delivery strategy using MailKit.
///
/// Fetches the user's email via IUserService, renders the appropriate HTML template,
/// and sends it via SMTP. Configured through SmtpSettings (appsettings.json → "Smtp" section).
///
/// MailKit is used instead of the deprecated System.Net.Mail.SmtpClient.
/// </summary>
public class EmailNotificationStrategy : INotificationStrategy
{
    private readonly SmtpSettings _smtp;
    private readonly IUserService _userService;
    private readonly INotificationTemplateProvider _templateProvider;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<EmailNotificationStrategy> _logger;

    public string StrategyName => "Email";

    public EmailNotificationStrategy(
        IOptions<SmtpSettings> smtpSettings,
        IUserService userService,
        INotificationTemplateProvider templateProvider,
        ITemplateEngine templateEngine,
        ILogger<EmailNotificationStrategy> logger)
    {
        _smtp             = smtpSettings.Value;
        _userService      = userService;
        _templateProvider = templateProvider;
        _templateEngine   = templateEngine;
        _logger           = logger;
    }

    public async Task SendAsync(string userId, NotificationType type, Dictionary<string, string> data)
    {
        // Resolve the user's email address.
        var emailResult = await _userService.GetUserEmailAsync(userId);
        if (!emailResult.IsSuccess || string.IsNullOrEmpty(emailResult.Data))
        {
            _logger.LogWarning("Cannot send email notification: no email found for user {UserId}", userId);
            return;
        }

        var recipientEmail = emailResult.Data;

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
        using var client = new SmtpClient();
        await client.ConnectAsync(_smtp.Server, _smtp.Port,
            _smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

        if (!string.IsNullOrEmpty(_smtp.UserName))
            await client.AuthenticateAsync(_smtp.UserName, _smtp.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email ({Type}) sent to {Email}", type, recipientEmail);
    }

    /// <summary>
    /// Returns a localized email subject based on the notification type and user's locale.
    /// </summary>
    /// <param name="type">The notification type.</param>
    /// <param name="locale">The user's locale.</param>
    /// <returns>The localized email subject.</returns>
    private static string GetSubject(NotificationType type, string? locale = "en")
{
    var currentLocale = locale?.ToLower() ?? "en";

    if (currentLocale == "fr")
    {
        return type switch
        {
            NotificationType.AccessRequestApproved => "Votre accès à Friendout a été approuvé",
            NotificationType.AccessRequestDenied   => "Mise à jour de votre demande d'accès à Friendout",
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
        NotificationType.ActivityModified      => "An activity you joined has been updated",
        NotificationType.ActivityCanceled      => "An activity has been canceled",
        NotificationType.ActivityReminder      => "Reminder: upcoming activity",
        NotificationType.InvitationReceived    => "You've been invited to an activity",
        NotificationType.AccountDeleted        => "Your Friendout account has been deleted",
        _                                      => "Notification from Friendout"
    };
}
}
