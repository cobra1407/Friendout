import { create } from "zustand";
import { type User } from "@/features/user/types/user.type";
import { authApi } from "@/features/auth/api/auth.api";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  isRateLimited: boolean;

  fetchMe: () => Promise<void>;
  logout: () => Promise<void>;
  updateUser: (patch: Partial<User>) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  loading: true,
  isRateLimited: false,

  fetchMe: async () => {
    try {
      const user = await authApi.me();
      set({
        user,
        isAuthenticated: !!user,
      });
    } catch (error) {
      if (error instanceof Error && error.message === "rate_limited") {
        set({ isRateLimited: true });
        return;
      }
      set({ user: null, isAuthenticated: false });
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

  updateUser: (patch) => {
    set((state) => ({
      user: state.user ? { ...state.user, ...patch } : state.user,
    }));
  },
}));
