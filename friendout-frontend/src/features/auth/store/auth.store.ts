import { create } from "zustand";
import { type User } from "@/features/user/types/user.type";
import { authApi } from "@/features/auth/api/auth.api";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;

  fetchMe: () => Promise<void>;
  logout: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  loading: true,

  fetchMe: async () => {
    try {
      const user = await authApi.me();
      set({
        user,
        isAuthenticated: !!user,
      });
    } catch {
      set({
        user: null,
        isAuthenticated: false,
      });
    } finally {
      set({ loading: false });
    }
  },

  logout: async () => {
    await authApi.logout();
    set({
      user: null,
      isAuthenticated: false,
    });
  },
}));
