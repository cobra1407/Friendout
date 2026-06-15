using System.ComponentModel.DataAnnotations;

namespace Friendout.Infrastructure.Options;

/// <summary>
/// SMTP email delivery settings.
/// Bound from the "Smtp" section in appsettings.json / environment variables.
/// </summary>
public class SmtpOptions
{
    public const string Section = "Smtp";

    [Required(AllowEmptyStrings = false, ErrorMessage = "Smtp:Server is required. Set it in your .env file or appsettings.json.")]
    public string Server { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Smtp:Port must be between 1 and 65535.")]
    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Smtp:UserName is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Smtp:Password is required.")]
    public string Password { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Smtp:FromAddress is required.")]
    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Friendout";
}
