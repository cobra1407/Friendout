namespace friendout_backend.Helpers;

/// <summary>
/// Loads .env and .env.local files into environment variables at startup.
/// </summary>
public static class EnvLoader
{
    /// <summary>
    /// Loads .env then .env.local (which takes precedence) from the current
    /// directory or the base directory of the running assembly.
    /// </summary>
    public static void Load()
    {
        LoadFile(".env");
        LoadFile(".env.local");
    }

    /// <summary>
    /// Parses a comma-separated configuration value into a string array.
    /// Useful for values that can be expressed as either a JSON array or a
    /// comma-separated string (e.g. CORS allowed origins).
    /// </summary>
    public static string[]? GetCommaSeparated(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    // -------------------------------------------------------

    private static void LoadFile(string filename)
    {
        var dirs = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var dir in dirs)
        {
            var path = Path.Combine(dir, filename);
            if (File.Exists(path))
            {
                DotNetEnv.Env.Load(path);
                break;
            }
        }
    }
}
