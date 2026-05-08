export type UserRole = "Admin" | "User";

export interface User {
  userId: string;
  name: string;
  avatarUrl?: string;
  email?: string;
  role?: UserRole;
}
