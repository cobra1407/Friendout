import { Outlet, Navigate } from "react-router-dom";
import { Spinner } from "@/components/ui/spinner";
import { useAuth } from "../hooks/useAuth";
import { RateLimitScreen } from "@/features/error/components/RateLimitScreen";

export const ProtectedRoutes = () => {
  const { loading, isAuthenticated, isRateLimited } = useAuth();

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Spinner className="size-10" />
      </div>
    );
  }

  if (isRateLimited) return <RateLimitScreen />;

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  return <Outlet />;
};

export default ProtectedRoutes;
