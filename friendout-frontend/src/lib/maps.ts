import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";

export const generateGoogleMapsUrl = (address: string): string => {
  const encodedAddress = encodeURIComponent(address);
  return `https://www.google.com/maps/search/?api=1&query=${encodedAddress}`;
};

export const generateGoogleMapsDirectionsUrl = (destination: string, origin?: string): string => {
  const encodedDestination = encodeURIComponent(destination);
  let url = `https://www.google.com/maps/dir/?api=1&destination=${encodedDestination}`;

  if (origin) {
    const encodedOrigin = encodeURIComponent(origin);
    url += `&origin=${encodedOrigin}`;
  }

  return url;
};

export const isValidAddress = (address: string): boolean => {
  return address.trim().length > 3;
};

export const isGoogleMapsLink = (url: string): boolean => {
  const mapsPatterns = [
    /^https?:\/\/(www\.)?google\.[a-z]+\/maps/i,
    /^https?:\/\/maps\.google\.[a-z]+/i,
    /^https?:\/\/goo\.gl\/maps/i,
    /^https?:\/\/maps\.app\.goo\.gl/i,
  ];

  return mapsPatterns.some((pattern) => pattern.test(url.trim()));
};

/**
 * True when a string extracted from a Maps link's /place/ segment is actually raw
 * coordinates (DMS like 50°22'51.2"N, or a decimal "lat,lng" pair) rather than a
 * real place name — happens when someone shares a dropped-pin location instead of
 * a searched address/business. In that case the backend will resolve a real city
 * name via reverse geocoding on save; this is just the client-side live preview,
 * so it falls back to the generic label instead of showing raw coordinates.
 */
const looksLikeCoordinates = (text: string): boolean => {
  if (/^-?\d+°/.test(text)) return true;
  if (/^-?\d+(\.\d+)?,\s*-?\d+(\.\d+)?$/.test(text)) return true;
  return false;
};

export const extractLocationNameFromMapsUrl = (url: string): string => {
  try {
    const urlObj = new URL(url);
    const searchParams = urlObj.searchParams;

    const pathMatch = urlObj.pathname.match(/\/maps\/place\/([^/]+)/);
    if (pathMatch) {
      const extracted = decodeURIComponent(pathMatch[1].replace(/\+/g, " "));
      if (!looksLikeCoordinates(extracted)) return extracted;
    }

    const qParam = searchParams.get("q");
    if (qParam) { 
      return decodeURIComponent(qParam.replace(/\+/g, " "));
    }

    const queryParam = searchParams.get("query");
    if (queryParam) {
      return decodeURIComponent(queryParam.replace(/\+/g, " "));
    }

    const destParam = searchParams.get("destination");
    if (destParam) {
      return decodeURIComponent(destParam.replace(/\+/g, " "));
    }

    return "Lieu depuis Google Maps";
  } catch {
    return "Lieu depuis Google Maps";
  }
};

export const validateGoogleMapsUrl = (url: string): { isValid: boolean; error?: string } => {
  if (!url.trim()) {
    return { isValid: false, error: "Le lien ne peut pas etre vide" };
  }

  if (!isGoogleMapsLink(url)) {
    return { isValid: false, error: "Ce n'est pas un lien Google Maps valide" };
  }

  try {
    new URL(url);
    return { isValid: true };
  } catch {
    return { isValid: false, error: "Le lien n'est pas une URL valide" };
  }
};

export const getLocalisationDisplayText = (localisation: Localisation | null | undefined): string => {
  if (!localisation) {
    return "Lieu non specifie";
  }

  if (localisation.type === LocalisationType.MapLink) {
    if (localisation.displayName?.trim()) return localisation.displayName;
    if (localisation.address?.trim()) return localisation.address;
    if (localisation.mapLink?.trim()) return extractLocationNameFromMapsUrl(localisation.mapLink);
    return "Lieu non specifie";
  }

  if (localisation.type === LocalisationType.Virtual) {
    return localisation.displayName || localisation.address || "Lieu virtuel";
  }

  return localisation.address || localisation.displayName || "Lieu non specifie";
};

export const getLocalisationUrl = (localisation: string, localisationData: Localisation | null | undefined): string => {
  if (!localisationData) {
    return generateGoogleMapsUrl(localisation);
  }

  if (localisationData.type === LocalisationType.MapLink) {
    return localisationData.mapLink || generateGoogleMapsUrl(localisationData.address || localisation);
  }

  if (localisationData.type === LocalisationType.Virtual) {
    return localisationData.virtualUrl || "#";
  }

  return generateGoogleMapsUrl(localisationData.address || localisation);
};

export const getLocationDisplayText = getLocalisationDisplayText;
export const getLocationUrl = getLocalisationUrl;
