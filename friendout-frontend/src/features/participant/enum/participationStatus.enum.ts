export const ParticipationStatus = {
  Participating: 0,
  Maybe: 1,
  NotParticipating: 2,
} as const;


export type ParticipationStatus =
  typeof ParticipationStatus[keyof typeof ParticipationStatus];
