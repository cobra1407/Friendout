using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace friendout_backend.Converters;

/// <summary>
/// Ensures all DateTime values are serialized with a trailing 'Z' (UTC indicator)
/// so JavaScript clients parse them correctly as UTC rather than local time.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
