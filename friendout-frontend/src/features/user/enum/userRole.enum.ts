export const UserRole = {
    Admin: "Admin",
    User: "User",
} as const;

export type UserRole = typeof UserRole[keyof typeof UserRole];
