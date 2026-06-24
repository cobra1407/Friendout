import en from './en.json';
import fr from './fr.json';
import { useLocaleStore } from './locale.store';

const translations: Record<string, any> = { en, fr };

/**
 * Language code used by getTranslation (e.g. 'fr', 'en').
 * Reads the reactive locale store — defaults to the browser language until
 * the user's saved preference is loaded, see locale.store.ts.
 */
export const getLang = () => useLocaleStore.getState().locale;

/** BCP 47 locale for date/number formatting (e.g. 'fr-FR', 'en-US'). */
export const getLocale = () => (getLang() === 'fr' ? 'fr-FR' : 'en-US');

export const getTranslation = (key: string, params?: Record<string, string | number>) => {
  const lang = getLang();
  const parts = key.split('.');
  let result: any = translations[lang];

  for (const part of parts) {
    result = result?.[part];
    if (result === undefined) break;
  }

  let str = typeof result === 'string' ? result : key;
  if (params) {
    Object.entries(params).forEach(([k, v]) => {
      str = str.replace(new RegExp(`\\{\\{${k}\\}\\}`, 'g'), String(v));
    });
  }
  return str;
};
