// ─────────────────────────────────────────────────────────────────────────────
//  localisation.utils.ts
// ─────────────────────────────────────────────────────────────────────────────
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";

/**
 * Normalise les différents types pouvant arriver depuis le backend.
 */
export const normalizeLocalisation = (
  localisation?: Localisation | null,
): Localisation | null => {
  if (!localisation) return null;

  // Cas standard déjà conforme
  if (
    localisation.type === LocalisationType.Address ||
    localisation.type === LocalisationType.MapLink ||
    localisation.type === LocalisationType.Virtual
  ) {
    return localisation;
  }

  // Compatibilité avec d’éventuels anciens noms de champs
  const legacyType = (localisation.type as unknown) as string;
  if (legacyType === "address")
    return { ...localisation, type: LocalisationType.Address };
  if (legacyType === "maps_link")
    return { ...localisation, type: LocalisationType.MapLink };
  if (legacyType === "virtual")
    return { ...localisation, type: LocalisationType.Virtual };

  return localisation;
};

/**
 * Helper générique utilisé dans les composants : récupère la localisation
 * depuis `localisation` ou `location` (les deux noms sont parfois utilisés).
 */
export const pickLocalisation = <
  T extends { localisation?: Localisation | null; location?: Localisation | null },
>(
  source?: T | null,
): Localisation | null => normalizeLocalisation(source?.localisation ?? source?.location ?? null);

/**
 * -------------------------------------------------------------------------
 *  Fonction centrale : retour d’une URL Google Maps **toujours** valide.
 * -------------------------------------------------------------------------
 *
 * - Si `localisation.mapLink` est déjà une URL complète → on la retourne.
 * - Sinon on construit une URL à partir de l’adresse `localisation.address`.
 * - Si aucune donnée n’est disponible, on renvoie la page d’accueil Google Maps.
 *
 * @param localisation
 * @returns URL encodée prête à être ouverte dans un nouvel onglet.
 */
export const getGoogleMapsUrl = (localisation?: Localisation | null): string => {
  if (!localisation) return "https://www.google.com/maps";

  const raw = localisation.mapLink?.trim();
  if (raw && /^https?:\/\//i.test(raw)) {
    return raw;
  }


  const address = localisation.address?.trim();
  if (!address) return "https://www.google.com/maps";


  const encoded = encodeURIComponent(address);
  return `https://www.google.com/maps/search/?api=1&query=${encoded}`;
};
