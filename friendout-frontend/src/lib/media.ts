const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "";

const BACKEND_ORIGIN = (() => {
  try {
    return new URL(API_BASE_URL).origin;
  } catch {
    return "";
  }
})();

export function resolveMediaUrl(rawUrl?: string | null): string | undefined {
  if (!rawUrl) return undefined;

  const url = rawUrl.trim();
  if (!url) return undefined;

  if (url.startsWith("blob:") || url.startsWith("data:")) {
    return url;
  }

  if (/^https?:\/\//i.test(url)) {
    return url;
  }

  if (url.startsWith("/uploads")) {
    return BACKEND_ORIGIN ? `${BACKEND_ORIGIN}${url}` : url;
  }

  return url;
}
