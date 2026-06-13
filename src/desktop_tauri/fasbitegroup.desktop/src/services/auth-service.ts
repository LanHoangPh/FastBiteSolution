import { apiClient, ApiResponse } from "./api-client";

export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string | null;
  avatarUrl: string | null;
  bio: string | null;
  isActive: boolean;
  roles: string[];
}

export interface AuthResponse {
  tokenType: string;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  user: UserInfo;
}

export interface RegisterResponse {
  message: string;
}

const TOKEN_KEY = "ebolt_access_token";
const REFRESH_TOKEN_KEY = "ebolt_refresh_token";
const USER_KEY = "ebolt_user_info";

// Local frontend session cache to support synchronous calls
let sessionCache: AuthResponse | null = null;
let isInitialized = false;

// Check if running in Tauri environment
const isTauri = typeof window !== "undefined" && Boolean((window as any).__TAURI_INTERNALS__);

export const authService = {
  /**
   * Initialize session by loading it from Rust (or fallback storage)
   */
  async initialize(): Promise<void> {
    if (isInitialized) return;

    if (isTauri) {
      try {
        const { invoke } = await import("@tauri-apps/api/core");
        const session = await invoke<AuthResponse | null>("load_session");
        if (session) {
          sessionCache = session;
        }
      } catch (err) {
        console.error("Failed to load session from Rust:", err);
      }
    } else {
      const token = localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY) || sessionStorage.getItem(REFRESH_TOKEN_KEY);
      const userStr = localStorage.getItem(USER_KEY) || sessionStorage.getItem(USER_KEY);
      if (token && refreshToken && userStr) {
        try {
          sessionCache = {
            tokenType: "Bearer",
            accessToken: token,
            refreshToken,
            accessTokenExpiresAt: "",
            refreshTokenExpiresAt: "",
            user: JSON.parse(userStr),
          };
        } catch {
          // Ignore
        }
      }
    }
    isInitialized = true;
  },

  /**
   * Save session info to Rust state/file or web fallback
   */
  async setSession(authData: AuthResponse, rememberMe: boolean = true): Promise<void> {
    sessionCache = authData;

    if (isTauri) {
      try {
        const { invoke } = await import("@tauri-apps/api/core");
        await invoke("save_session", { session: authData, rememberMe });
      } catch (err) {
        console.error("Failed to save session to Rust:", err);
      }
    } else {
      const storage = rememberMe ? localStorage : sessionStorage;
      const otherStorage = rememberMe ? sessionStorage : localStorage;

      otherStorage.removeItem(TOKEN_KEY);
      otherStorage.removeItem(REFRESH_TOKEN_KEY);
      otherStorage.removeItem(USER_KEY);

      storage.setItem(TOKEN_KEY, authData.accessToken);
      storage.setItem(REFRESH_TOKEN_KEY, authData.refreshToken);
      storage.setItem(USER_KEY, JSON.stringify(authData.user));
    }
  },

  /**
   * Clear session info
   */
  async clearSession(): Promise<void> {
    sessionCache = null;

    if (isTauri) {
      try {
        const { invoke } = await import("@tauri-apps/api/core");
        await invoke("clear_session");
      } catch (err) {
        console.error("Failed to clear session in Rust:", err);
      }
    } else {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      sessionStorage.removeItem(TOKEN_KEY);
      sessionStorage.removeItem(REFRESH_TOKEN_KEY);
      sessionStorage.removeItem(USER_KEY);
    }
  },

  /**
   * Get active token
   */
  getAccessToken(): string | null {
    return sessionCache?.accessToken || null;
  },

  /**
   * Get active refresh token
   */
  getRefreshToken(): string | null {
    return sessionCache?.refreshToken || null;
  },

  /**
   * Get active user info
   */
  getUserInfo(): UserInfo | null {
    return sessionCache?.user || null;
  },

  /**
   * Check if user is currently authenticated
   */
  isAuthenticated(): boolean {
    if (!isInitialized && !isTauri) {
      const token = localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
      return !!token;
    }
    return !!sessionCache?.accessToken;
  },

  /**
   * Login user
   */
  async login(
    email: string,
    password: string,
    rememberMe: boolean = true,
  ): Promise<ApiResponse<AuthResponse>> {
    const res = await apiClient.post<AuthResponse>("/api/v1/auth/login", {
      email,
      password,
    });
    if (res.data) {
      await this.setSession(res.data, rememberMe);
    }
    return res;
  },

  /**
   * Register a new user
   */
  async register(
    email: string,
    password: string,
    firstName: string,
    lastName: string,
    dayOfBirth: string,
  ): Promise<ApiResponse<RegisterResponse>> {
    return apiClient.post<RegisterResponse>("/api/v1/auth/register", {
      email,
      password,
      firstName,
      lastName,
      dayOfBirth,
    });
  },

  /**
   * Verify email confirmation token
   */
  async verifyEmail(
    email: string,
    token: string,
  ): Promise<ApiResponse<AuthResponse>> {
    const res = await apiClient.post<AuthResponse>(
      "/api/v1/auth/verify-email",
      { email, token },
    );
    if (res.data) {
      // Default to rememberMe = true for verification login
      await this.setSession(res.data, true);
    }
    return res;
  },

  /**
   * Request password reset OTP
   */
  async forgotPassword(email: string): Promise<ApiResponse<unknown>> {
    return apiClient.post<unknown>("/api/v1/auth/forgot-password", { email });
  },

  /**
   * Reset password using OTP
   */
  async resetPassword(
    email: string,
    otp: string,
    newPassword: string,
  ): Promise<ApiResponse<unknown>> {
    return apiClient.post<unknown>("/api/v1/auth/reset-password", {
      email,
      otp,
      newPassword,
    });
  },

  /**
   * Revoke current session
   */
  async logout(): Promise<ApiResponse<unknown>> {
    const refreshToken = this.getRefreshToken();
    const accessToken = this.getAccessToken();

    await this.clearSession();

    if (!refreshToken) {
      return { status: 204 };
    }

    return apiClient.post<unknown>(
      "/api/v1/auth/logout",
      { refreshToken },
      {
        headers: accessToken
          ? { Authorization: `Bearer ${accessToken}` }
          : undefined,
      },
    );
  },

  /**
   * Revoke all active sessions for the current user
   */
  async revokeAll(): Promise<ApiResponse<unknown>> {
    const accessToken = this.getAccessToken();

    await this.clearSession();

    return apiClient.post<unknown>(
      "/api/v1/auth/revoke-all",
      null,
      {
        headers: accessToken
          ? { Authorization: `Bearer ${accessToken}` }
          : undefined,
      },
    );
  },

  /**
   * Login or auto-register using Google ID Token
   */
  async googleLogin(idToken: string): Promise<ApiResponse<AuthResponse>> {
    const res = await apiClient.post<AuthResponse>("/api/v1/auth/google-login", {
      idToken,
    });
    if (res.data) {
      await this.setSession(res.data, true);
    }
    return res;
  },
};
