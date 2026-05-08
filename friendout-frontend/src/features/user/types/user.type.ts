import { UserRole } from "@/features/user/enum/userRole.enum";

export { UserRole };
export interface User {
  userId: string;
  name: string;
  avatarUrl?: string;
  email?: string;
  role?: UserRole;
}
