import { useState } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { PasswordInput } from "./PasswordInput";
import { authService } from "@/services/auth-service";
import { Key, Lock, ArrowRight } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/utils";
import { validateResetPasswordForm } from "@/shared/validation/auth-validation";

interface ResetPasswordCardProps {
  email: string;
  onResetSuccess: () => void;
  onNavigateToLogin: () => void;
}

export function ResetPasswordCard({
  email,
  onResetSuccess,
  onNavigateToLogin,
}: ResetPasswordCardProps) {
  const { t } = useTranslation();
  const [otp, setOtp] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [formErrors, setFormErrors] = useState<{
    otp?: string;
    password?: string;
    confirmPassword?: string;
  }>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setFormErrors({});

    const { errors, isValid } = validateResetPasswordForm({
      otp,
      password: newPassword,
      confirmPassword,
    });

    if (!isValid) {
      setFormErrors(errors);
      return;
    }

    setIsLoading(true);

    const res = await authService.resetPassword(email, otp, newPassword);
    setIsLoading(false);

    if (res.error) {
      if (res.error.includes("Auth.InvalidOtp") || res.error.includes("InvalidOtp")) {
        setError(t("auth.errors.invalidOtp"));
      } else if (res.error.includes("Auth.AccountBlocked") || res.error.includes("AccountBlocked")) {
        setError(t("auth.errors.accountBlocked"));
      } else {
        setError(res.error);
      }
    } else {
      onResetSuccess();
    }
  };

  return (
    <Card className="auth-card">
      <CardContent className="p-0 w-full flex flex-col">
        {/* Header Icon */}
        <div className="auth-icon">
          <Lock className="size-6 text-indigo-600 dark:text-indigo-400" />
        </div>

        {/* Title */}
        <h2 className="text-[22px] font-bold text-center tracking-tight text-foreground mt-6">
          {t("auth.setNewPasswordTitle")}
        </h2>

        {/* Subtitle */}
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          {t("auth.setNewPasswordSubtitle")}
        </p>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* OTP Input */}
          <div className="space-y-1 w-full">
            <div className="auth-input-wrapper">
              <Key className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
              <Input
                type="text"
                placeholder={t("auth.otpCode")}
                required
                className={cn("auth-input", formErrors.otp && "border-red-500 focus-visible:ring-red-500/20")}
                value={otp}
                onChange={(e) => {
                  setOtp(e.target.value);
                  if (formErrors.otp) setFormErrors(prev => ({ ...prev, otp: undefined }));
                }}
                disabled={isLoading}
              />
            </div>
            {formErrors.otp && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.otp)}</p>
            )}
          </div>

          {/* New Password Input */}
          <div className="space-y-1 w-full">
            <PasswordInput
              placeholder={t("auth.newPassword")}
              required
              className={formErrors.password ? "border-red-500 focus-visible:ring-red-500/20" : ""}
              value={newPassword}
              onChange={(e) => {
                setNewPassword(e.target.value);
                if (formErrors.password) setFormErrors(prev => ({ ...prev, password: undefined }));
              }}
              disabled={isLoading}
            />
            {formErrors.password && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.password)}</p>
            )}
          </div>

          {/* Confirm Password Input */}
          <div className="space-y-1 w-full">
            <PasswordInput
              placeholder={t("auth.confirmNewPassword")}
              required
              className={formErrors.confirmPassword ? "border-red-500 focus-visible:ring-red-500/20" : ""}
              value={confirmPassword}
              onChange={(e) => {
                setConfirmPassword(e.target.value);
                if (formErrors.confirmPassword) setFormErrors(prev => ({ ...prev, confirmPassword: undefined }));
              }}
              disabled={isLoading}
            />
            {formErrors.confirmPassword && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.confirmPassword)}</p>
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
            aria-label="Reset password button"
          >
            {isLoading ? (
              <span className="size-4.5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              <>
                {t("auth.resetPasswordButton")}
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
