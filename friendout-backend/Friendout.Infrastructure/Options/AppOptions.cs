using System.ComponentModel.DataAnnotations;

namespace Friendout.Infrastructure.Options;

/// <summary>
/// General application settings.
/// Bound from the "App" section in appsettings.json / environment variables.
/// </summary>
public class AppOptions
{
    public const string Section = "App";

    /// <summary>The public-facing URL of the application (e.g. https://friendout.example.com).</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "App:Url is required. Set it in your .env file or appsettings.json.")]
    public string Url { get; set; } = string.Empty;
}
