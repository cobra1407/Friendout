import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api";

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
});

// On 401, try one /auth/refresh, then retry the original request.
// _retry avoids infinite loops; refreshPromise dedupes concurrent 401s so a
// rotated refresh token isn't invalidated by a second call before it's used.
let refreshPromise: Promise<unknown> | null = null;

api.interceptors.response.use(
  response => response,
  async (error) => {
    const originalRequest = error.config;

    const is401 = error.response?.status === 401;
    const isRefreshEndpoint = originalRequest?.url?.includes("/auth/refresh");
    const alreadyRetried = originalRequest?._retry === true;
    const isOnLoginPage = window.location.pathname === "/login";
    const isOnPublicSharePage = window.location.pathname.startsWith("/share/");

    // Don't attempt refresh if:
    // - Already on the login page (avoids redirect loop — /auth/me returns 401 on login page intentionally)
    // - On a public share page (anonymous visitors are expected to get 401s there)
    // - The failing request is /auth/refresh itself
    // - We already retried once
    if (is401 && !isRefreshEndpoint && !alreadyRetried && !isOnLoginPage && !isOnPublicSharePage) {
      originalRequest._retry = true;

      try {
        if (!refreshPromise) {
          refreshPromise = api.post("/auth/refresh").finally(() => {
            refreshPromise = null;
          });
        }
        await refreshPromise;
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
