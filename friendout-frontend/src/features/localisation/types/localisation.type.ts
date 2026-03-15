export const LocalisationType = {
  Address: 0,
  MapLink: 1,
  Virtual: 2,
} as const;

export type LocalisationType = typeof LocalisationType[keyof typeof LocalisationType];

export interface Localisation {
    type: LocalisationType;
    address?: string;
    mapLink?: string;
    virtualUrl?: string;
    displayName?: string;
    platform?: string;
    serverInfo?: string;
}
