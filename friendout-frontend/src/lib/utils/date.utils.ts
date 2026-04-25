import { getLocale, getTranslation } from '@/i18n';

/**
 * Formats a date as a human-readable string in the current locale (French or English).
 *
 * @param dateInput - The date to format, as a string or Date object.
 * @returns The formatted date, or translated "Unknown date"/"Invalid date" if the value is not valid.
 *
 * Example (fr-FR): "samedi 1 juin 2024"
 * Example (en-US): "Saturday, June 1, 2024"
 */
export const formatDate = (dateInput?: string | Date): string => {
    if (!dateInput) return getTranslation('date.unknown');

    const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;

    if (isNaN(date.getTime())) return getTranslation('date.invalid');

    return date.toLocaleDateString(getLocale(), {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

/**
 * Formats a time as "HH:mm" in French.
 *
 * @param {string | null} time - The time or datetime string.
 * @returns {string | null} - Formatted as "HH:mm", or null if invalid.
 */
export const formatTime = (time: string | Date): string => {
  // Normalize to UTC: backend may omit the 'Z' suffix on DateTime strings.
  // Without it, the browser parses as local time, shifting the displayed hour by the UTC offset.
  let date: Date
  if (typeof time === 'string') {
    const normalized = !time.endsWith('Z') && !time.includes('+') && !time.includes('-', 10)
      ? time + 'Z'
      : time
    date = new Date(normalized)
  } else {
    date = time
  }
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
};



export const CalculateDuration = (startTime: string, endTime: string): string => {
  const [startH, startM] = startTime.split(":").map(Number);
  const [endH, endM] = endTime.split(":").map(Number);

  if ([startH, startM, endH, endM].some(isNaN)) return getTranslation('date.invalid');

  const startTotal = startH * 60 + startM;
  const endTotal = endH * 60 + endM;

  if (endTotal < startTotal) return getTranslation('date.invalid');

  const diff = endTotal - startTotal;
  const hours = Math.floor(diff / 60);
  const minutes = diff % 60;

  return `${hours}h ${minutes}m`;
};



/**
 * Checks if an activity has started in the past.
 *
 * @param {Activity} activity - The activity to check.
 * @returns {boolean} - True if the activity has started in the past, false otherwise.
 */
  export function isPast(date: string | Date): boolean {
    return new Date(date) < new Date();
}
