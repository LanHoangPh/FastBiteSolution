
interface LogoProps {
  brandName?: string;
}

export function Logo({ brandName = "Ebolt" }: LogoProps) {
  const firstLetter = brandName.charAt(0).toUpperCase();

  return (
    <div className="auth-logo">
      {/* 28x28 dark icon with a bright letter inside */}
      <div className="flex items-center justify-center size-7 rounded-lg bg-slate-900 dark:bg-slate-100 text-slate-50 dark:text-slate-950 font-bold text-[13px] select-none shadow-sm">
        {firstLetter}
      </div>
      <span className="font-semibold text-base tracking-tight text-foreground select-none">
        {brandName}
      </span>
    </div>
  );
}
