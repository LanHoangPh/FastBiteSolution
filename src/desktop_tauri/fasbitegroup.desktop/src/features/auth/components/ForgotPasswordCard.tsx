import { useState } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { authService } from "@/services/auth-service";
import { Mail, KeyRound, ArrowRight } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/utils";
import { validateForgotPasswordForm } from "@/shared/validation/auth-validation";

interface ForgotPasswordCardProps {
  initialEmail: string;
  onOtpRequested: (email: string) => void;
  onNavigateToLogin: () => void;
}

export function ForgotPasswordCard({
  initialEmail,
  onOtpRequested,
  onNavigateToLogin,
}: ForgotPasswordCardProps) {
  const { t } = useTranslation();
  const [email, setEmail] = useState(initialEmail);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [formErrors, setFormErrors] = useState<{ email?: string }>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setFormErrors({});

    const { errors, isValid } = validateForgotPasswordForm(email);
    if (!isValid) {
      setFormErrors(errors);
      return;
    }

    setIsLoading(true);

    const res = await authService.forgotPassword(email);
    setIsLoading(false);

    if (res.error) {
      if (
        res.error.includes("Auth.UserNotFound") ||
        res.error.includes("UserNotFound")
      ) {
        setError(t("auth.errors.userNotFound"));
      } else if (
        res.error.includes("Auth.TooManyRequests") ||
        res.error.includes("TooManyRequests")
      ) {
        setError(t("auth.errors.tooManyRequests"));
      } else {
        setError(res.error);
      }
    } else {
      onOtpRequested(email);
    }
  };

  return (
    <Card className="auth-card">
      <CardContent className="p-0 w-full flex flex-col">
        {/* Header Icon */}
        <div className="auth-icon">
          <KeyRound className="size-6 text-indigo-600 dark:text-indigo-400" />
        </div>

        {/* Title */}
        <h2 className="text-[22px] font-bold text-center tracking-tight text-foreground mt-6">
          {t("auth.resetPasswordTitle")}
        </h2>

        {/* Subtitle */}
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          {t("auth.forgotPasswordSubtitle")}
        </p>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* Email Input */}
          <div className="space-y-1 w-full">
            <div className="auth-input-wrapper">
              <Mail className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
              <Input
                type="email"
                placeholder={t("auth.email")}
                required
                className={cn("auth-input", formErrors.email && "border-red-500 focus-visible:ring-red-500/20")}
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  if (formErrors.email) setFormErrors(prev => ({ ...prev, email: undefined }));
                }}
                disabled={isLoading}
              />
            </div>
            {formErrors.email && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.email)}</p>
            )}
          </div>

          {/* Error Message */}
          {error && (
            <div className="text-xs text-red-500 dark:text-red-400 text-center font-medium animate-in fade-in slide-in-from-top-1 duration-200">
              ⚠️ {error}
            </div>
          )}

          {/* Submit Button */}
          <Button
            type="submit"
            disabled={isLoading}
            className="auth-primary-button"
            aria-label="Send OTP button"
          >
            {isLoading ? (
              <span className="size-4.5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              <>
                {t("auth.sendCode")}
                <ArrowRight className="size-4" />
              </>
            )}
          </Button>
        </form>

        {/* Back to Login */}
        <div className="text-center mt-4">
          <button
            type="button"
            onClick={onNavigateToLogin}
            className="text-xs text-indigo-600 dark:text-indigo-400 font-semibold hover:underline cursor-pointer"
          >
            {t("auth.backToSignIn")}
          </button>
        </div>
      </CardContent>
    </Card>
  );
}
