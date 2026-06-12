import * as React from "react";
import { Lock, Eye, EyeOff } from "lucide-react";
import { Input } from "@/shared/components/ui/input";
import { cn } from "@/shared/utils";

export function PasswordInput({ className, ...props }: React.ComponentProps<"input">) {
  const [showPassword, setShowPassword] = React.useState(false);

  return (
    <div className="auth-input-wrapper">
      <Lock className="absolute left-3 size-4 text-muted-foreground pointer-events-none z-10" />
      <Input
        type={showPassword ? "text" : "password"}
        className={cn("auth-input", className)}
        {...props}
      />
      <button
        type="button"
        onClick={() => setShowPassword(!showPassword)}
        className="absolute right-3 p-1 rounded-md text-muted-foreground hover:text-foreground hover:bg-secondary/80 transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring z-10 cursor-pointer"
        aria-label={showPassword ? "Hide password" : "Show password"}
      >
        {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
      </button>
    </div>
  );
}
