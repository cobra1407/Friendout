namespace Friendout.Domain.Constants;

/// <summary>
/// Whitelist of icon keys that can be assigned to an <see cref="Models.EquipmentList"/>.
/// Each key maps to a specific Lucide icon component on the frontend (see
/// equipmentListIcons.ts). Kept as a closed set rather than free text so the
/// stored value can never point to a non-existent icon.
/// </summary>
public static class EquipmentListIcons
{
    public const string Default = "backpack";

    public static readonly HashSet<string> AllowedKeys = new()
    {
        "backpack",
        "tent",
        "mountain",
        "bike",
        "footprints",
        "waves",
        "snowflake",
        "utensils",
        "music",
        "camera",
        "dumbbell",
        "gamepad-2",
        "palette",
        "book-open",
        "package"
    };

    public static bool IsValid(string? icon) => icon is not null && AllowedKeys.Contains(icon);
}
