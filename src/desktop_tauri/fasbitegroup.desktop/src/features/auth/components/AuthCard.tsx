import { useState } from "react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Separator } from "@/shared/components/ui/separator";
import { PasswordInput } from "./PasswordInput";
import { SocialLoginButtons } from "./SocialLoginButtons";
import { Mail, LogIn, ArrowRight } from "lucide-react";

interface AuthCardProps {
  onLoginSuccess: () => void;
  onNavigateToRegister: () => void;
}

export function AuthCard({
  onLoginSuccess,
  onNavigateToRegister,
}: AuthCardProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) return;

    setIsLoading(true);

    // Simulate login delay
    setTimeout(() => {
      setIsLoading(false);
      onLoginSuccess();
    }, 1200);
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
          Sign in with email
        </h2>

        {/* 3. Subtitle */}
        <p className="text-[13.5px] text-muted-foreground text-center mt-2 max-w-[270px] mx-auto leading-relaxed">
          Make a new doc to bring your words, data, and teams together. For free
        </p>

        {/* Form Section */}
        <form onSubmit={handleSubmit} className="space-y-4 mt-6 w-full">
          {/* 4. Email Input */}
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

          {/* 5. Password Input */}
          <PasswordInput
            placeholder="Password"
            required
            aria-label="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={isLoading}
          />

          {/* 6. Forgot Password */}
          <div className="flex justify-end">
            <button
              type="button"
              onClick={(e) => e.preventDefault()}
              className="auth-forgot-button"
              aria-label="Forgot password"
            >
              Forgot password?
            </button>
          </div>

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
                Get Started
                <ArrowRight className="size-4" />
              </>
            )}
          </Button>
        </form>

        {/* Switch to Register */}
        <div className="text-center mt-4">
          <span className="text-xs text-muted-foreground">
            Don't have an account?{" "}
            <button
              type="button"
              onClick={onNavigateToRegister}
              className="text-xs text-indigo-600 dark:text-indigo-400 font-semibold hover:underline cursor-pointer"
            >
              Sign up
            </button>
          </span>
        </div>

        {/* 8. Divider */}
        <div className="auth-divider">
          <Separator className="flex-1" />
          <span className="text-[12px] text-muted-foreground uppercase tracking-widest font-semibold whitespace-nowrap">
            Or sign in with
          </span>
          <Separator className="flex-1" />
        </div>

        {/* 9. Social Login Buttons */}
        <SocialLoginButtons />
      </CardContent>
    </Card>
  );
}
