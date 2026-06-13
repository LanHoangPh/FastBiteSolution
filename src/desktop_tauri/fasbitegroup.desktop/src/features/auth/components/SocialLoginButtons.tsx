import { Button } from "@/shared/components/ui/button";
import { authService } from "@/services/auth-service";

interface SocialLoginButtonsProps {
  onLoginSuccess?: () => void;
  onLoginError?: (error: string) => void;
}

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID || "938749832749-abcdefgh.apps.googleusercontent.com";
const isTauri = typeof window !== "undefined" && Boolean((window as any).__TAURI_INTERNALS__);

export function SocialLoginButtons({
  onLoginSuccess,
  onLoginError,
}: SocialLoginButtonsProps) {
  const handleGoogleLogin = async () => {
    try {
      let idToken: string | null = null;

      if (isTauri) {
        // 1. In Tauri, calculate the authUrl and invoke the login flow on the backend
        const redirectUri = "http://localhost:14250/callback";
        const authUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${GOOGLE_CLIENT_ID}&redirect_uri=${redirectUri}&response_type=id_token&scope=openid%20email%20profile&nonce=random_nonce&response_mode=query`;

        // Call the Tauri command which starts TCP listener, opens popup window, and automatically closes it
        const { invoke } = await import("@tauri-apps/api/core");
        idToken = await invoke<string>("start_google_login_flow", { authUrl });
      } else {
        // 2. In browser mode, display a prompt to manually enter an ID token for development testing
        const pastedToken = prompt(
          "Google Sign-In is only fully automated in desktop mode. For browser testing, please paste a valid Google ID token:"
        );
        if (pastedToken) {
          idToken = pastedToken.trim();
        }
      }

      if (!idToken) return;

      const res = await authService.googleLogin(idToken);
      if (res.error) {
        onLoginError?.(res.error);
      } else {
        onLoginSuccess?.();
      }
    } catch (err) {
      console.error("Google sign in failed:", err);
      onLoginError?.(err instanceof Error ? err.message : "Google authentication failed");
    }
  };

  return (
    <div className="social-login-grid">
      {/* Google Button */}
      <Button
        variant="outline"
        type="button"
        className="social-login-button"
        aria-label="Sign in with Google"
        onClick={handleGoogleLogin}
      >
        <svg className="size-4.5" viewBox="0 0 24 24" fill="currentColor">
          <path
            d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
            fill="#4285F4"
          />
          <path
            d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            fill="#34A853"
          />
          <path
            d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"
            fill="#FBBC05"
          />
          <path
            d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"
            fill="#EA4335"
          />
        </svg>
      </Button>

      {/* Facebook Button */}
      <Button
        variant="outline"
        type="button"
        className="social-login-button text-[#1877F2]"
        aria-label="Sign in with Facebook"
        onClick={() => console.log("Facebook sign in clicked")}
      >
        <svg className="size-4.5 fill-current" viewBox="0 0 24 24">
          <path d="M22 12c0-5.52-4.48-10-10-10S2 6.48 2 12c0 4.84 3.44 8.87 8 9.8V15H8v-3h2V9.5C10 7.57 11.57 6 13.5 6H16v3h-2c-.55 0-1 .45-1 1v2h3v3h-3v6.95c4.56-.93 8-4.96 8-9.75z" />
        </svg>
      </Button>

      {/* Apple Button */}
      <Button
        variant="outline"
        type="button"
        className="social-login-button text-foreground"
        aria-label="Sign in with Apple"
        onClick={() => console.log("Apple sign in clicked")}
      >
        <svg className="size-4.5 fill-current" viewBox="0 0 24 24">
          <path d="M18.71 19.5c-.83 1.24-1.71 2.45-3.05 2.47-1.34.03-1.77-.79-3.29-.79-1.53 0-2 .77-3.27.82-1.31.05-2.3-1.32-3.14-2.53C4.25 17 2.94 12.45 4.7 9.39c.87-1.52 2.43-2.48 4.12-2.51 1.28-.02 2.5.87 3.29.87.78 0 2.26-1.07 3.81-.91.65.03 2.47.26 3.64 1.98-.09.06-2.17 1.28-2.15 3.81.03 3.02 2.65 4.03 2.68 4.04-.03.07-.42 1.44-1.38 2.83M15.97 4.17c.66-.81 1.11-1.93.99-3.06-1 .04-2.22.67-2.94 1.5-.63.73-1.18 1.87-1.03 2.97 1.12.09 2.27-.57 2.98-1.41z" />
        </svg>
      </Button>
    </div>
  );
}
