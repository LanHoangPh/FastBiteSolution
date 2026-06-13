import { useState } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Separator } from "@/shared/components/ui/separator";
import { PasswordInput } from "./PasswordInput";
import { SocialLoginButtons } from "./SocialLoginButtons";
import { Mail, LogIn, ArrowRight } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/utils";
import { validateLoginForm } from "@/shared/validation/auth-validation";

import { authService } from "@/services/auth-service";

interface AuthCardProps {
  onLoginSuccess: () => void;
  onNavigateToRegister: () => void;
  onNavigateToForgotPassword: () => void;
  onRequireVerification: (email: string) => void;
  successMessage?: string;
}

export function AuthCard({
  onLoginSuccess,
  onNavigateToRegister,
  onNavigateToForgotPassword,
  onRequireVerification,
  successMessage,
}: AuthCardProps) {
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(true);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [formErrors, setFormErrors] = useState<{
    email?: string;
    password?: string;
  }>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setFormErrors({});

    const { errors, isValid } = validateLoginForm(email, password);
    if (!isValid) {
      setFormErrors(errors);
      return;
    }

    setIsLoading(true);

    const res = await authService.login(email, password, rememberMe);
    setIsLoading(false);

    if (res.error) {
      if (
        res.error.includes("Auth.AccountInactive") ||
        res.error.includes("AccountInactive")
      ) {
        // Redirect to email verification
        onRequireVerification(email);
      } else if (
        res.error.includes("Auth.InvalidCredentials") ||
        res.error.includes("InvalidCredentials")
      ) {
        setError(t("auth.errors.invalidCredentials"));
      } else {
        setError(res.error);
      }
    } else {
      onLoginSuccess();
    }
  };

  return (
    <Card className="auth-card">
      <CardContent className="p-0 w-full flex flex-col">
        {/* 1. Login Icon */}
        <div className="auth-icon">
          <LogIn className="size-6 text-indigo-600 dark:text-indigo-400" />
        </div>

        {/* 2. Title */}
        <h2 className="text-[22px] font-bold text-center tracking-tight text-foreground mt-6">
          {t("auth.loginTitle")}
        </h2>

        {/* 3. Subtitle */}
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          {t("auth.loginSubtitle")}
        </p>

        {/* Form Section */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* 4. Email Input */}
          <div className="space-y-1 w-full">
            <div className="auth-input-wrapper">
              <Mail className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
              <Input
                type="email"
                placeholder={t("auth.email")}
                required
                aria-label="Email Address"
                className={cn(
                  "auth-input",
                  formErrors.email &&
                    "border-red-500 focus-visible:ring-red-500/20",
                )}
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  if (formErrors.email)
                    setFormErrors((prev) => ({ ...prev, email: undefined }));
                }}
                disabled={isLoading}
              />
            </div>
            {formErrors.email && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">
                {t(formErrors.email)}
              </p>
            )}
          </div>

          {/* 5. Password Input */}
          <div className="space-y-1 w-full">
            <PasswordInput
              placeholder={t("auth.password")}
              required
              aria-label="Password"
              className={
                formErrors.password
                  ? "border-red-500 focus-visible:ring-red-500/20"
                  : ""
              }
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                if (formErrors.password)
                  setFormErrors((prev) => ({ ...prev, password: undefined }));
              }}
              disabled={isLoading}
            />
            {formErrors.password && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">
                {t(formErrors.password)}
              </p>
            )}
          </div>

          {/* 6. Remember Me & Forgot Password */}
          <div className="flex items-center justify-between select-none">
            <label className="flex items-center gap-2 cursor-pointer text-xs text-muted-foreground hover:text-foreground transition-colors">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                className="rounded border-border/80 text-indigo-600 focus:ring-indigo-500/30 bg-slate-100/50 dark:bg-slate-800/30 size-3.5 cursor-pointer accent-indigo-600"
              />
              {t("auth.rememberMe")}
            </label>
            <button
              type="button"
              onClick={() => onNavigateToForgotPassword()}
              className="auth-forgot-button"
              aria-label="Forgot password"
            >
              {t("auth.forgotPassword")}
            </button>
          </div>

          {/* Success Message */}
          {successMessage && (
            <div className="text-xs text-green-600 dark:text-green-400 text-center font-medium animate-in fade-in slide-in-from-top-1 duration-200">
              ✓ {successMessage}
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="text-xs text-red-500 dark:text-red-400 text-center font-medium animate-in fade-in slide-in-from-top-1 duration-200">
              ⚠️ {error}
            </div>
          )}

          {/* 7. Primary Button */}
          <Button
            type="submit"
            disabled={isLoading}
            className="auth-primary-button"
            aria-label="Sign in button"
          >
            {isLoading ? (
              <span className="size-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              <>
                {t("auth.getStarted")}
                <ArrowRight className="size-4" />
              </>
            )}
          </Button>
        </form>

        {/* Switch to Register */}
        <div className="text-center mt-4">
          <span className="text-xs text-muted-foreground">
            {t("auth.dontHaveAccount")}{" "}
            <button
              type="button"
              onClick={onNavigateToRegister}
              className="text-xs text-indigo-600 dark:text-indigo-400 font-semibold hover:underline cursor-pointer"
            >
              {t("auth.signUp")}
            </button>
          </span>
        </div>

        {/* 8. Divider */}
        <div className="auth-divider">
          <Separator className="flex-1" />
          <span className="text-[12px] text-muted-foreground uppercase tracking-widest font-semibold whitespace-nowrap">
            {t("auth.orSignInWith")}
          </span>
          <Separator className="flex-1" />
        </div>

        {/* 9. Social Login Buttons */}
        <SocialLoginButtons 
          onLoginSuccess={onLoginSuccess}
          onLoginError={(err) => setError(err)}
        />
      </CardContent>
    </Card>
  );
}
