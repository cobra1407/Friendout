import api from "@/lib/api/api";
import type { User } from "@/features/user/types/user.type";
import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api";
const DISCORD_AUTH_URL =
  import.meta.env.VITE_DISCORD_AUTH_URL ??
  `${API_BASE_URL.replace(/\/$/, "")}/auth/discord`;

const GOOGLE_AUTH_URL =
  import.meta.env.VITE_GOOGLE_AUTH_URL ??
  `${API_BASE_URL.replace(/\/$/, "")}/auth/google`;

export interface LoginMethodsDto {
  discordAvailable: boolean;
  googleAvailable: boolean;
}

export const authApi = {
  me: async (): Promise<User | null> => {
    try {
      const res = await api.get<User>("/auth/me");
      return res.data;
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 401) {
          return null; // user not authenticated
        }
        if (error.response?.status === 429) {
          throw new Error("rate_limited", { cause: error }); // preserve auth state
        }
      }

      console.error("Failed to fetch user:", error);
      throw error;
    }
  },

  loginMethods: async (): Promise<LoginMethodsDto> => {
    const res = await api.get<LoginMethodsDto>("/auth/login-methods");
    return res.data;
  },

  logout: async () => {
    await api.post("/auth/logout");
  },

  discordLogin: async () => {
    window.location.href = DISCORD_AUTH_URL;
  },

  googleLogin: async () => {
    window.location.href = GOOGLE_AUTH_URL;
  },
};
