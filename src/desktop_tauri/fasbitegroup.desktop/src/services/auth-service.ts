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

export const authService = {
  /**
   * Save session info to localStorage
   */
  setSession(authData: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, authData.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, authData.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(authData.user));
  },

  /**
   * Clear session info from localStorage
   */
  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },

  /**
   * Get active token
   */
  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  },

  /**
   * Get active refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  },

  /**
   * Get active user info
   */
  getUserInfo(): UserInfo | null {
    const userStr = localStorage.getItem(USER_KEY);
    if (!userStr) return null;
    try {
      return JSON.parse(userStr) as UserInfo;
    } catch {
      return null;
    }
  },

  /**
   * Check if user is currently authenticated
   */
  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  },

  /**
   * Login user
   */
  async login(
    email: string,
    password: string,
  ): Promise<ApiResponse<AuthResponse>> {
    const res = await apiClient.post<AuthResponse>("/api/v1/auth/login", {
      email,
      password,
    });
    if (res.data) {
      this.setSession(res.data);
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
      this.setSession(res.data);
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

    this.clearSession();

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
};
