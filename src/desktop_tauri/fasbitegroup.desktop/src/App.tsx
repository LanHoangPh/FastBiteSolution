import { useState } from "react";
import { Calculator } from "@/features/calculator/components/Calculator";
import { AuthBackground } from "@/features/auth/components/AuthBackground";
import { Logo } from "@/features/auth/components/Logo";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { RegisterCard } from "@/features/auth/components/RegisterCard";
import { Button } from "@/shared/components/ui/button";
import { Sun, Moon } from "lucide-react";
import { useTheme } from "@/app/theme-provider";
import { motion, AnimatePresence } from "framer-motion";
import "./App.css";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [authView, setAuthView] = useState<"login" | "register">("login");
  const { theme, setTheme } = useTheme();
  const isDark =
    theme === "dark" ||
    (theme === "system" &&
      window.matchMedia("(prefers-color-scheme: dark)").matches);

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
            <Logo brandName="Ebolt" />

            {/* Floating Theme Toggle (Top-Right) */}
            <div className="fixed top-6 right-6 sm:right-[60px] z-50">
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
                {authView === "login" ? (
                  <motion.div
                    key="login"
                    initial={{ opacity: 0, y: 15, scale: 0.98 }}
                    animate={{ opacity: 1, y: 0, scale: 1 }}
                    exit={{ opacity: 0, y: -15, scale: 0.98 }}
                    transition={{ duration: 0.22, ease: "easeInOut" }}
                  >
                    <AuthCard
                      onLoginSuccess={() => setIsLoggedIn(true)}
                      onNavigateToRegister={() => setAuthView("register")}
                    />
                  </motion.div>
                ) : (
                  <motion.div
                    key="register"
                    initial={{ opacity: 0, y: 15, scale: 0.98 }}
                    animate={{ opacity: 1, y: 0, scale: 1 }}
                    exit={{ opacity: 0, y: -15, scale: 0.98 }}
                    transition={{ duration: 0.22, ease: "easeInOut" }}
                  >
                    <RegisterCard
                      onRegisterSuccess={() => setIsLoggedIn(true)}
                      onNavigateToLogin={() => setAuthView("login")}
                    />
                  </motion.div>
                )}
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
            <Calculator onLogout={() => setIsLoggedIn(false)} />
          </motion.main>
        )}
      </AnimatePresence>
    </div>
  );
}

export default App;
