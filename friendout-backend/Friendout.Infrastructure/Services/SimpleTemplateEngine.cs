using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Friendout.Infrastructure.Interfaces;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Renders HTML templates by replacing {{ Key }} placeholders with values
/// from a Dictionary&lt;string, string&gt;.
///
/// Placeholder format: {{ UserEmail }}, {{ ActivityName }}, etc.
/// Keys are matched case-insensitively.
/// Unknown placeholders are left as-is so missing variables are visible in output.
///
/// Example:
///   template : "Hello {{ UserEmail }}, your request has been approved."
///   data     : { "UserEmail": "thomas@gmail.com" }
///   result   : "Hello thomas@gmail.com, your request has been approved."
/// </summary>
public class SimpleTemplateEngine : ITemplateEngine
{
    private static readonly Regex PlaceholderRegex = new(@"\{\{\s*(.+?)\s*\}\}", RegexOptions.Compiled);

    public string Render(string template, Dictionary<string, string> data)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value.Trim();

            // Case-insensitive lookup
            var pair = data.FirstOrDefault(kv =>
                string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));

            // Return the value if found, or leave the placeholder intact
            return pair.Key is not null ? pair.Value : match.Value;
        });
    }
}
