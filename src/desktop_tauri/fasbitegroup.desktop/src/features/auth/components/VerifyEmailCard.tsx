import { useState, useEffect } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { authService } from "@/services/auth-service";
import { Link, ShieldCheck, ArrowRight } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/utils";
import { validateVerifyEmailForm } from "@/shared/validation/auth-validation";

interface VerifyEmailCardProps {
  email: string;
  initialToken?: string;
  onVerificationSuccess: () => void;
  onNavigateToLogin: () => void;
}

export function VerifyEmailCard({
  email,
  initialToken,
  onVerificationSuccess,
  onNavigateToLogin,
}: VerifyEmailCardProps) {
  const { t } = useTranslation();
  const [token, setToken] = useState(initialToken || "");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [formErrors, setFormErrors] = useState<{ token?: string }>({});

  const parseTokenAndEmail = (input: string) => {
    let parsedToken = input.trim();
    let parsedEmail = email;

    if (input.includes("?")) {
      try {
        const urlParams = new URLSearchParams(input.substring(input.indexOf("?")));
        const t = urlParams.get("token");
        const e = urlParams.get("email");
        if (t) parsedToken = t;
        if (e) parsedEmail = e;
      } catch (err) {
        // Fallback
      }
    }
    return { token: parsedToken, email: parsedEmail };
  };

  useEffect(() => {
    if (initialToken && email) {
      const autoVerify = async () => {
        setIsLoading(true);
        const res = await authService.verifyEmail(email, initialToken);
        setIsLoading(false);
        if (res.error) {
          if (res.error.includes("Auth.InvalidToken") || res.error.includes("InvalidToken")) {
            setError(t("auth.errors.invalidToken"));
          } else {
            setError(res.error);
          }
        } else {
          onVerificationSuccess();
        }
      };
      autoVerify();
    }
  }, [initialToken, email, onVerificationSuccess, t]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setFormErrors({});

    const { token: parsedToken, email: parsedEmail } = parseTokenAndEmail(token);

    const { errors, isValid } = validateVerifyEmailForm(parsedToken);
    if (!isValid) {
      setFormErrors(errors);
      return;
    }

    setIsLoading(true);

    const res = await authService.verifyEmail(parsedEmail, parsedToken);
    setIsLoading(false);

    if (res.error) {
      if (res.error.includes("Auth.InvalidToken") || res.error.includes("InvalidToken")) {
        setError(t("auth.errors.invalidToken"));
      } else {
        setError(res.error);
      }
    } else {
      onVerificationSuccess();
    }
  };

  return (
    <Card className="auth-card">
      <CardContent className="p-0 w-full flex flex-col">
        {/* Header Icon */}
        <div className="auth-icon">
          <ShieldCheck className="size-6 text-indigo-600 dark:text-indigo-400" />
        </div>

        {/* Title */}
        <h2 className="text-[22px] font-bold text-center tracking-tight text-foreground mt-6">
          {t("auth.verifyEmailTitle")}
        </h2>

        {/* Subtitle */}
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          {t("auth.verifyEmailSubtitle", { email })}
        </p>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* Verification Token/Link Input */}
          <div className="space-y-1 w-full">
            <div className="auth-input-wrapper">
              <Link className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
              <Input
                type="text"
                placeholder={t("auth.verificationToken")}
                required
                maxLength={1000}
                className={cn("auth-input", formErrors.token && "border-red-500 focus-visible:ring-red-500/20")}
                value={token}
                onChange={(e) => {
                  setToken(e.target.value);
                  if (formErrors.token) setFormErrors(prev => ({ ...prev, token: undefined }));
                }}
                disabled={isLoading}
              />
            </div>
            {formErrors.token && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.token)}</p>
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
            aria-label="Verify email button"
          >
            {isLoading ? (
              <span className="size-4.5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              <>
                {t("auth.confirmSignIn")}
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
