import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
});

// ─────────────────────────────────────────────────────────────────────────────
// Refresh token interceptor
//
// When an API call returns 401 (access token expired), this interceptor:
//   1. Calls POST /auth/refresh — the browser sends the refresh_token cookie automatically.
//   2. If the refresh succeeds, the backend sets new auth_token + refresh_token cookies.
//   3. The original request is retried with the new access token.
//   4. If the refresh fails (refresh token expired or revoked), the user is redirected
//      to the login page.
//
// _retry flag: prevents infinite loops (if /auth/refresh itself returns 401).
// ─────────────────────────────────────────────────────────────────────────────
api.interceptors.response.use(
  response => response,
  async (error) => {
    const originalRequest = error.config;

    const is401 = error.response?.status === 401;
    const isRefreshEndpoint = originalRequest?.url?.includes("/auth/refresh");
    const alreadyRetried = originalRequest?._retry === true;
    const isOnLoginPage = window.location.pathname === "/login";

    // Don't attempt refresh if:
    // - Already on the login page (avoids redirect loop — /auth/me returns 401 on login page intentionally)
    // - The failing request is /auth/refresh itself
    // - We already retried once
    if (is401 && !isRefreshEndpoint && !alreadyRetried && !isOnLoginPage) {
      originalRequest._retry = true;

      try {
        await api.post("/auth/refresh");
        return api(originalRequest);
      } catch {
        // Refresh failed: session is over, redirect to login.
        window.location.href = "/login";
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
