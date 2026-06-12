import { useState, useRef } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Separator } from "@/shared/components/ui/separator";
import { PasswordInput } from "./PasswordInput";
import { SocialLoginButtons } from "./SocialLoginButtons";
import { Mail, User, UserPlus, ArrowRight, Calendar } from "lucide-react";
import { authService } from "@/services/auth-service";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/utils";
import { validateRegisterForm } from "@/shared/validation/auth-validation";

interface RegisterCardProps {
  onRegisterSuccess: (email: string) => void;
  onNavigateToLogin: () => void;
}

export function RegisterCard({
  onRegisterSuccess,
  onNavigateToLogin,
}: RegisterCardProps) {
  const { t } = useTranslation();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [dayOfBirth, setDayOfBirth] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [formErrors, setFormErrors] = useState<{
    firstName?: string;
    lastName?: string;
    email?: string;
    dayOfBirth?: string;
    password?: string;
    confirmPassword?: string;
  }>({});

  const birthdateRef = useRef<HTMLInputElement>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setFormErrors({});

    const { errors, isValid } = validateRegisterForm({
      firstName,
      lastName,
      email,
      dayOfBirth,
      password,
      confirmPassword,
    });

    if (!isValid) {
      setFormErrors(errors);
      return;
    }

    setIsLoading(true);

    // Convert date to ISO string
    let isoBirthdate = new Date().toISOString();
    try {
      isoBirthdate = new Date(dayOfBirth).toISOString();
    } catch {
      // Keep fallback
    }

    const res = await authService.register(email, password, firstName, lastName, isoBirthdate);
    setIsLoading(false);

    if (res.error) {
      if (res.error.includes("Auth.EmailAlreadyExists") || res.error.includes("EmailAlreadyExists")) {
        setError(t("auth.errors.emailAlreadyExists"));
      } else {
        setError(res.error);
      }
    } else {
      onRegisterSuccess(email);
    }
  };

  return (
    <Card className="auth-card">
      <CardContent className="p-0 w-full flex flex-col">
        {/* Header Icon Section */}
        <div className="auth-icon">
          <UserPlus className="size-6 text-indigo-600 dark:text-indigo-400" />
        </div>

        {/* Title & Subtitle */}
        <h2 className="text-[22px] font-bold text-center tracking-tight text-foreground mt-6">
          {t("auth.registerTitle")}
        </h2>
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          {t("auth.registerSubtitle")}
        </p>

        {/* Form Section */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* First Name & Last Name Input Grid */}
          <div className="grid grid-cols-2 gap-3 w-full">
            {/* First Name Input */}
            <div className="space-y-1">
              <div className="auth-input-wrapper">
                <User className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
                <Input
                  type="text"
                  placeholder={t("auth.firstName")}
                  required
                  aria-label="First Name"
                  className={cn("auth-input", formErrors.firstName && "border-red-500 focus-visible:ring-red-500/20")}
                  value={firstName}
                  onChange={(e) => {
                    setFirstName(e.target.value);
                    if (formErrors.firstName) setFormErrors(prev => ({ ...prev, firstName: undefined }));
                  }}
                  disabled={isLoading}
                />
              </div>
              {formErrors.firstName && (
                <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.firstName)}</p>
              )}
            </div>

            {/* Last Name Input */}
            <div className="space-y-1">
              <div className="auth-input-wrapper">
                <User className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
                <Input
                  type="text"
                  placeholder={t("auth.lastName")}
                  required
                  aria-label="Last Name"
                  className={cn("auth-input", formErrors.lastName && "border-red-500 focus-visible:ring-red-500/20")}
                  value={lastName}
                  onChange={(e) => {
                    setLastName(e.target.value);
                    if (formErrors.lastName) setFormErrors(prev => ({ ...prev, lastName: undefined }));
                  }}
                  disabled={isLoading}
                />
              </div>
              {formErrors.lastName && (
                <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.lastName)}</p>
              )}
            </div>
          </div>

          {/* Email Input */}
          <div className="space-y-1 w-full">
            <div className="auth-input-wrapper">
              <Mail className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
              <Input
                type="email"
                placeholder={t("auth.email")}
                required
                aria-label="Email Address"
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

          {/* Date of Birth Input */}
          <div className="space-y-1 w-full">
            <div 
              className={cn("auth-input-wrapper cursor-pointer rounded-xl border border-transparent", formErrors.dayOfBirth && "border-red-500 focus-within:ring-red-500/20")} 
              onClick={() => {
                if (!isLoading) {
                  try {
                    birthdateRef.current?.showPicker();
                  } catch (err) {
                    console.warn("showPicker not supported:", err);
                  }
                }
              }}
            >
              <Calendar className="absolute left-3 size-4 text-muted-foreground z-10" />
              <Input
                ref={birthdateRef}
                type="date"
                placeholder={t("auth.dateOfBirth")}
                required
                aria-label="Date of Birth"
                className={cn("auth-input cursor-pointer", formErrors.dayOfBirth && "border-red-500 focus-visible:ring-red-500/20")}
                value={dayOfBirth}
                onChange={(e) => {
                  setDayOfBirth(e.target.value);
                  if (formErrors.dayOfBirth) setFormErrors(prev => ({ ...prev, dayOfBirth: undefined }));
                }}
                disabled={isLoading}
                onClick={(e) => {
                  e.stopPropagation(); // Avoid double picker call if clicked directly on input
                  if (!isLoading) {
                    try {
                      birthdateRef.current?.showPicker();
                    } catch (err) {
                      console.warn("showPicker not supported:", err);
                    }
                  }
                }}
              />
            </div>
            {formErrors.dayOfBirth && (
              <p className="text-[11px] text-red-500 px-1 animate-in fade-in slide-in-from-top-1 duration-150">{t(formErrors.dayOfBirth)}</p>
            )}
          </div>

          {/* Password Input */}
          <div className="space-y-1 w-full">
            <PasswordInput
              placeholder={t("auth.password")}
              required
              aria-label="Password"
              className={formErrors.password ? "border-red-500 focus-visible:ring-red-500/20" : ""}
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
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
              placeholder={t("auth.confirmPassword")}
              required
              aria-label="Confirm Password"
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
            aria-label="Create account button"
          >
            {isLoading ? (
              <span className="size-4.5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              <>
                {t("auth.createAccount")}
                <ArrowRight className="size-4" />
              </>
            )}
          </Button>
        </form>

        {/* Switch to Login */}
        <div className="text-center mt-4">
          <span className="text-xs text-muted-foreground">
            {t("auth.alreadyHaveAccount")}{" "}
            <button
              type="button"
              onClick={onNavigateToLogin}
              className="text-xs text-indigo-600 dark:text-indigo-400 font-semibold hover:underline cursor-pointer"
            >
              {t("auth.signIn")}
            </button>
          </span>
        </div>

        {/* Divider Section */}
        <div className="auth-divider">
          <Separator className="flex-1" />
          <span className="text-[12px] text-muted-foreground uppercase tracking-widest font-semibold whitespace-nowrap">
            {t("auth.orSignUpWith")}
          </span>
          <Separator className="flex-1" />
        </div>

        {/* Social Register Options */}
        <SocialLoginButtons />
      </CardContent>
    </Card>
  );
}
