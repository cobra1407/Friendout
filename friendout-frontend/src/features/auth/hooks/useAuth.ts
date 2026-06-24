import { useAuthStore } from "@/features/auth/store/auth.store";

export const useAuth = () => {
  const {
    user,
    isAuthenticated,
    loading,
    isRateLimited,
    fetchMe,
    logout,
    updateUser,
  } = useAuthStore();

  return {
    user,
    isAuthenticated,
    loading,
    isRateLimited,
    fetchMe,
    logout,
    updateUser,
  };
};
