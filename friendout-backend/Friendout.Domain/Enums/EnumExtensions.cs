using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

public static class EnumExtensions
{
    public static string GetEnumMemberValue(this Enum value)
    {
        var member = value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault();

        var attribute = member?
            .GetCustomAttribute<EnumMemberAttribute>();

        return attribute?.Value ?? value.ToString();
    }

    /// <summary>
    /// Reverse of GetEnumMemberValue: finds the enum member whose [EnumMember(Value = ...)]
    /// matches the given string. Falls back to the member's own name if no attribute matches.
    /// Used to map database-stored string values (e.g. "DISCORD") back to their enum
    /// (e.g. ProviderEnum.Discord) in EF Core value converters.
    /// </summary>
    public static TEnum ParseEnumMemberValue<TEnum>(this string value) where TEnum : struct, Enum
    {
        foreach (var candidate in Enum.GetValues<TEnum>())
        {
            if (((Enum)(object)candidate).GetEnumMemberValue() == value)
                return candidate;
        }

        throw new ArgumentException($"Unknown enum member value '{value}' for {typeof(TEnum)}");
    }
}