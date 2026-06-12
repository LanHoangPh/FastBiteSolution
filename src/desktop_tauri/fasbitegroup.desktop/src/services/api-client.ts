import { Log } from "@/shared/logger";

const BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

export interface ApiResponse<T> {
  data?: T;
  error?: string;
  status: number;
}

// Kiểm tra xem ứng dụng có đang chạy trong môi trường Tauri (Desktop) hay không
const isTauri = typeof window !== "undefined" && Boolean((window as any).__TAURI_INTERNALS__);

let tauriFetchCache: any = null;

async function getFetchFn() {
  if (!isTauri) {
    return window.fetch.bind(window);
  }
  if (!tauriFetchCache) {
    try {
      // Dynamic import to avoid loading Tauri HTTP plugin in browser mode
      const module = await import("@tauri-apps/plugin-http");
      tauriFetchCache = module.fetch;
    } catch (err) {
      console.error("Failed to load Tauri HTTP plugin, falling back to window.fetch:", err);
      tauriFetchCache = window.fetch.bind(window);
    }
  }
  return tauriFetchCache;
}

/**
 * Perform a generic HTTP request using Tauri's native HTTP plugin to bypass CORS.
 */
async function request<T>(
  path: string,
  options: RequestInit = {},
): Promise<ApiResponse<T>> {
  const url = `${BASE_URL.replace(/\/$/, "")}/${path.replace(/^\//, "")}`;

  try {
    const headers = {
      "Content-Type": "application/json",
      ...options.headers,
    };

    // Nếu chạy trên Web Browser thì dùng fetch của trình duyệt, nếu chạy trên Tauri thì dùng tauriFetch để bypass CORS
    const fetchFn = await getFetchFn();

    const response = await fetchFn(url, {
      ...options,
      headers,
    });

    if (!response.ok) {
      let errorMessage = `HTTP error! status: ${response.status}`;
      try {
        const errorData = await response.json();
        if (
          errorData &&
          typeof errorData === "object" &&
          "detail" in errorData
        ) {
          errorMessage = errorData.detail as string;
        } else if (
          errorData &&
          typeof errorData === "object" &&
          "message" in errorData
        ) {
          errorMessage = errorData.message as string;
        }
      } catch {
        // Fallback if not JSON or empty
      }

      return {
        status: response.status,
        error: errorMessage,
      };
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return {
        status: response.status,
        data: {} as T,
      };
    }

    const data = (await response.json()) as T;
    return {
      status: response.status,
      data,
    };
  } catch (error) {
    Log.error("API Client request failed:", error);
    return {
      status: 500,
      error: error instanceof Error ? error.message : "Network request failed",
    };
  }
}

export const apiClient = {
  get: <T>(path: string, options?: RequestInit) =>
    request<T>(path, { ...options, method: "GET" }),

  post: <T>(path: string, body?: unknown, options?: RequestInit) =>
    request<T>(path, {
      ...options,
      method: "POST",
      body: body ? JSON.stringify(body) : undefined,
    }),

  put: <T>(path: string, body?: unknown, options?: RequestInit) =>
    request<T>(path, {
      ...options,
      method: "PUT",
      body: body ? JSON.stringify(body) : undefined,
    }),

  patch: <T>(path: string, body?: unknown, options?: RequestInit) =>
    request<T>(path, {
      ...options,
      method: "PATCH",
      body: body ? JSON.stringify(body) : undefined,
    }),

  delete: <T>(path: string, options?: RequestInit) =>
    request<T>(path, { ...options, method: "DELETE" }),
};
