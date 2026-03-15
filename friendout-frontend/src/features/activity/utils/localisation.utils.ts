import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";

export const normalizeLocalisation = (localisation?: Localisation | null): Localisation | null => {
  if (!localisation) return null;
  if (localisation.type === LocalisationType.Address || localisation.type === LocalisationType.MapLink || localisation.type === LocalisationType.Virtual) {
    return localisation;
  }

  if ((localisation.type as unknown) === "address") {
    return { ...localisation, type: LocalisationType.Address };
  }

  if ((localisation.type as unknown) === "maps_link") {
    return { ...localisation, type: LocalisationType.MapLink };
  }

  if ((localisation.type as unknown) === "virtual") {
    return { ...localisation, type: LocalisationType.Virtual };
  }

  return localisation;
};

export const pickLocalisation = <T extends { localisation?: Localisation | null; location?: Localisation | null }>(
  source?: T | null,
): Localisation | null => normalizeLocalisation(source?.localisation ?? source?.location ?? null);
