export const ParticipationStatus = {
    Participating: "Participating",
    Maybe: "Maybe",
    NotParticipating: "NotParticipating",
} as const;

export type ParticipationStatus = typeof ParticipationStatus[keyof typeof ParticipationStatus];
