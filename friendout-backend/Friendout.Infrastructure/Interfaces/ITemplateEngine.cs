using System.Collections.Generic;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Renders a raw HTML template by replacing {{ Key }} placeholders
/// with values from the provided dictionary.
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Returns the rendered HTML string.
    /// Unknown placeholders are left intact so missing variables are visible.
    /// </summary>
    string Render(string template, Dictionary<string, string> data);
}
