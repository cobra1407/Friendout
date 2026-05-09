export const ParticipationStatus = {
feat/user-menu-and-admin-scaffold
    Participating: "Participating",
    Maybe: "Maybe",
    NotParticipating: "NotParticipating",
} as const;

export type ParticipationStatus = typeof ParticipationStatus[keyof typeof ParticipationStatus];

