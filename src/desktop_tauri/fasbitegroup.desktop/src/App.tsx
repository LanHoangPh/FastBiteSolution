import { useState, useEffect } from "react";
import { Calculator } from "@/features/calculator/components/Calculator";
import { AuthBackground } from "@/features/auth/components/AuthBackground";
import { Logo } from "@/features/auth/components/Logo";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { RegisterCard } from "@/features/auth/components/RegisterCard";
import { VerifyEmailCard } from "@/features/auth/components/VerifyEmailCard";
import { ForgotPasswordCard } from "@/features/auth/components/ForgotPasswordCard";
import { ResetPasswordCard } from "@/features/auth/components/ResetPasswordCard";
import { Button } from "@/shared/components/ui/button";
import { Sun, Moon } from "lucide-react";
import { useTheme } from "@/app/theme-provider";
import { motion, AnimatePresence } from "framer-motion";
import { authService } from "@/services/auth-service";
import { LanguageSelector } from "@/shared/components/LanguageSelector";
import "./App.css";

function App() {
  const [isInitializing, setIsInitializing] = useState(true);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [authView, setAuthView] = useState<
    "login" | "register" | "verify-email" | "forgot-password" | "reset-password"
  >("login");
  const [email, setEmail] = useState("");
  const [initialToken, setInitialToken] = useState(() => {
    const params = new URLSearchParams(window.location.search);
    return params.get("token") || "";
  });

  const [successMessage, setSuccessMessage] = useState("");
  const { theme, setTheme } = useTheme();

  useEffect(() => {
    const initSession = async () => {
      const params = new URLSearchParams(window.location.search);
      const hasToken = params.has("token");
      const hasEmail = params.has("email");
      const isVerifyPath = window.location.pathname === "/verify-email";

      if (isVerifyPath || (hasToken && hasEmail)) {
        setEmail(params.get("email") || "");
        setAuthView("verify-email");
        window.history.replaceState({}, document.title, "/");
      }

      await authService.initialize();
      setIsLoggedIn(authService.isAuthenticated());
      setIsInitializing(false);
    };
    initSession();
  }, []);

  const isDark =
    theme === "dark" ||
    (theme === "system" &&
      window.matchMedia("(prefers-color-scheme: dark)").matches);

  const handleLogout = async () => {
    await authService.logout();
    setIsLoggedIn(false);
  };

  const renderAuthCard = () => {
    switch (authView) {
      case "login":
        return (
          <AuthCard
            onLoginSuccess={() => setIsLoggedIn(true)}
            onNavigateToRegister={() => {
              setAuthView("register");
              setSuccessMessage("");
            }}
            onNavigateToForgotPassword={() => {
              setAuthView("forgot-password");
              setSuccessMessage("");
            }}
            onRequireVerification={(targetEmail) => {
              setEmail(targetEmail);
              setInitialToken("");
              setAuthView("verify-email");
            }}
            successMessage={successMessage}
          />
        );
      case "register":
        return (
          <RegisterCard
            onRegisterSuccess={(registeredEmail) => {
              setEmail(registeredEmail);
              setInitialToken("");
              setAuthView("verify-email");
            }}
            onNavigateToLogin={() => setAuthView("login")}
          />
        );
      case "verify-email":
        return (
          <VerifyEmailCard
            email={email}
            initialToken={initialToken}
            onVerificationSuccess={() => setIsLoggedIn(true)}
            onNavigateToLogin={() => {
              setInitialToken("");
              setAuthView("login");
            }}
          />
        );
      case "forgot-password":
        return (
          <ForgotPasswordCard
            initialEmail={email}
            onOtpRequested={(targetEmail) => {
              setEmail(targetEmail);
              setAuthView("reset-password");
            }}
            onNavigateToLogin={() => setAuthView("login")}
          />
        );
      case "reset-password":
        return (
          <ResetPasswordCard
            email={email}
            onResetSuccess={() => {
              setSuccessMessage("Password reset successfully. Please sign in.");
              setAuthView("login");
            }}
            onNavigateToLogin={() => setAuthView("login")}
          />
        );
    }
  };

  if (isInitializing) {
    return (
      <div className="min-h-screen w-full flex items-center justify-center bg-slate-50 dark:bg-slate-950 transition-colors duration-500">
        <span className="size-8 border-4 border-indigo-600/30 border-t-indigo-600 rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="relative min-h-screen w-full overflow-x-hidden transition-colors duration-500 bg-slate-50 dark:bg-slate-950">
      <AnimatePresence mode="wait">
        {!isLoggedIn ? (
          <motion.div
            key="auth"
            initial={{ opacity: 1 }}
            exit={{ opacity: 0, scale: 0.97 }}
            transition={{ duration: 0.25 }}
            className="auth-page"
          >
            {/* Background Layers */}
            <AuthBackground />

            {/* Brand Logo Header */}
            <Logo brandName="FastBite" />

            {/* Floating Actions (Top-Right) */}
            <div className="fixed top-6 right-6 sm:right-[60px] z-50 flex items-center gap-2">
              <LanguageSelector />
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setTheme(isDark ? "light" : "dark")}
                className="rounded-full opacity-75 hover:opacity-100 transition-all duration-300 text-foreground cursor-pointer bg-white/25 dark:bg-slate-900/40 backdrop-blur-md border border-border/50 shadow-sm size-9"
                title="Toggle theme"
              >
                {isDark ? (
                  <Sun className="size-4.5" />
                ) : (
                  <Moon className="size-4.5" />
                )}
              </Button>
            </div>

            {/* Login / Register Card Container with Transitions */}
            <div className="z-20">
              <AnimatePresence mode="wait">
                <motion.div
                  key={authView}
                  initial={{ opacity: 0, y: 15, scale: 0.98 }}
                  animate={{ opacity: 1, y: 0, scale: 1 }}
                  exit={{ opacity: 0, y: -15, scale: 0.98 }}
                  transition={{ duration: 0.22, ease: "easeInOut" }}
                >
                  {renderAuthCard()}
                </motion.div>
              </AnimatePresence>
            </div>
          </motion.div>
        ) : (
          <motion.main
            key="app"
            initial={{ opacity: 0, scale: 1.03 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.97 }}
            transition={{ duration: 0.28, ease: "easeOut" }}
            className="flex items-center justify-center min-h-screen w-full bg-gradient-to-tr from-slate-100 to-slate-200 dark:from-slate-900 dark:to-slate-950 p-4 transition-colors duration-300"
          >
            <Calculator onLogout={handleLogout} />
          </motion.main>
        )}
      </AnimatePresence>
    </div>
  );
}

export default App;
