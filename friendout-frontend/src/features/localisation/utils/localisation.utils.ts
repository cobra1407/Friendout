import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";

/**
 * Normalizes a localisation object from the API.
 *
 * The backend uses JsonStringEnumConverter, so enum values arrive as PascalCase
 * strings ("Address", "MapLink", "Virtual") instead of the numeric values (0, 1, 2)
 * expected by the frontend. This function converts them accordingly.
 */
export const normalizeLocalisation = (
  localisation?: Localisation | null,
): Localisation | null => {
  if (!localisation) return null;

  if (
    localisation.type === LocalisationType.Address ||
    localisation.type === LocalisationType.MapLink ||
    localisation.type === LocalisationType.Virtual
  ) {
    return localisation;
  }

  // Handle string values: PascalCase from backend ("Address", "MapLink", "Virtual")
  // and legacy snake_case ("address", "maps_link", "virtual")
  const rawType = (localisation.type as unknown) as string;
  if (typeof rawType === "string") {
    const lower = rawType.toLowerCase();
    if (lower === "address")
      return { ...localisation, type: LocalisationType.Address };
    if (lower === "maplink" || lower === "maps_link")
      return { ...localisation, type: LocalisationType.MapLink };
    if (lower === "virtual")
      return { ...localisation, type: LocalisationType.Virtual };
  }

  return localisation;
};

/**
 * Picks the localisation from a source object, checking both `localisation`
 * and `location` fields (both names are used across the codebase).
 */
export const pickLocalisation = <
  T extends { localisation?: Localisation | null; location?: Localisation | null },
>(
  source?: T | null,
): Localisation | null => normalizeLocalisation(source?.localisation ?? source?.location ?? null);

/**
 * Returns a valid Google Maps URL from a localisation object.
 * - If `mapLink` is a full URL, returns it directly.
 * - Otherwise builds a search URL from `address`.
 * - Falls back to the Google Maps homepage if no data is available.
 */
export const getGoogleMapsUrl = (localisation?: Localisation | null): string => {
  if (!localisation) return "https://www.google.com/maps";

  const raw = localisation.mapLink?.trim();
  if (raw && /^https?:\/\//i.test(raw)) return raw;

  const address = localisation.address?.trim();
  if (!address) return "https://www.google.com/maps";

  return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`;
};
