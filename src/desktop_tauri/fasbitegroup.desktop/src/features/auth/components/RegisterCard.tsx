import { useState } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Separator } from "@/shared/components/ui/separator";
import { PasswordInput } from "./PasswordInput";
import { SocialLoginButtons } from "./SocialLoginButtons";
import { Mail, User, UserPlus, ArrowRight } from "lucide-react";

interface RegisterCardProps {
  onRegisterSuccess: () => void;
  onNavigateToLogin: () => void;
}

export function RegisterCard({
  onRegisterSuccess,
  onNavigateToLogin,
}: RegisterCardProps) {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!name || !email || !password || !confirmPassword) return;

    if (password !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    if (password.length < 6) {
      setError("Password must be at least 6 characters");
      return;
    }

    setIsLoading(true);

    // Simulate registration delay
    setTimeout(() => {
      setIsLoading(false);
      onRegisterSuccess();
    }, 1200);
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
          Create an account
        </h2>
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          Sign up now to start tracking your calculations and sync your devices.
        </p>

        {/* Form Section */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* Full Name Input */}
          <div className="auth-input-wrapper">
            <User className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
            <Input
              type="text"
              placeholder="Full Name"
              required
              aria-label="Full Name"
              className="auth-input"
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={isLoading}
            />
          </div>

          {/* Email Input */}
          <div className="auth-input-wrapper">
            <Mail className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
            <Input
              type="email"
              placeholder="Email"
              required
              aria-label="Email Address"
              className="auth-input"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isLoading}
            />
          </div>

          {/* Password Input */}
          <PasswordInput
            placeholder="Password"
            required
            aria-label="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={isLoading}
          />

          {/* Confirm Password Input */}
          <PasswordInput
            placeholder="Confirm Password"
            required
            aria-label="Confirm Password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            disabled={isLoading}
          />

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
                Create Account
                <ArrowRight className="size-4" />
              </>
            )}
          </Button>
        </form>

        {/* Switch to Login */}
        <div className="text-center mt-4">
          <span className="text-xs text-muted-foreground">
            Already have an account?{" "}
            <button
              type="button"
              onClick={onNavigateToLogin}
              className="text-xs text-indigo-600 dark:text-indigo-400 font-semibold hover:underline cursor-pointer"
            >
              Sign in
            </button>
          </span>
        </div>

        {/* Divider Section */}
        <div className="auth-divider">
          <Separator className="flex-1" />
          <span className="text-[12px] text-muted-foreground uppercase tracking-widest font-semibold whitespace-nowrap">
            Or sign up with
          </span>
          <Separator className="flex-1" />
        </div>

        {/* Social Register Options */}
        <SocialLoginButtons />
      </CardContent>
    </Card>
  );
}
