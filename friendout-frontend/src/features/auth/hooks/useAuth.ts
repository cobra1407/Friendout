import { useAuthStore } from "@/features/auth/store/auth.store";

export const useAuth = () => {
  const {
    user,
    isAuthenticated,
    loading,
    fetchMe,
    logout,
  } = useAuthStore();

  return {
    user,
    isAuthenticated,
    loading,
    fetchMe,
    logout,
  };
};
