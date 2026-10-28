/**
 * Returns a normalized URL only if it is an absolute http(s) URL, otherwise undefined.
 *
 * Every user-supplied link (virtual invitation link, Maps link...) MUST go through this
 * before reaching an `href` or `window.open()`: it blocks `javascript:`, `data:`, `vbscript:`
 * and any other scheme, including values stored in the database before server-side
 * validation existed.
 */
export const toSafeHttpUrl = (url?: string | null): string | undefined => {
  if (!url) return undefined;

  try {
    const parsed = new URL(url.trim());
    return parsed.protocol === "http:" || parsed.protocol === "https:" ? parsed.href : undefined;
  } catch {
    return undefined;
  }
};

/** Opens a user-supplied link in a new tab, only if it is a safe http(s) URL. */
export const openExternalUrl = (url?: string | null): void => {
  const safeUrl = toSafeHttpUrl(url);
  if (!safeUrl) return;
  window.open(safeUrl, "_blank", "noopener,noreferrer");
};
