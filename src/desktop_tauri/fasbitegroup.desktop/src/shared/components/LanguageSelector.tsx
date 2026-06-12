import { useTranslation } from "react-i18next";
import { Button } from "@/shared/components/ui/button";
import { FluentTranslateIcon } from "./icons/FluentTranslateIcon";

export function LanguageSelector() {
  const { i18n } = useTranslation();
  const currentLanguage = i18n.language;

  const toggleLanguage = () => {
    const nextLang = currentLanguage === "vi" ? "en" : "vi";
    i18n.changeLanguage(nextLang);
    localStorage.setItem("ebolt_language", nextLang);
  };

  return (
    <Button
      variant="ghost"
      size="sm"
      onClick={toggleLanguage}
      className="rounded-full px-3 py-1 opacity-75 hover:opacity-100 transition-all duration-300 text-foreground cursor-pointer bg-white/25 dark:bg-slate-900/40 backdrop-blur-md border border-border/50 shadow-sm flex items-center gap-1.5 h-9"
      title={
        currentLanguage === "vi"
          ? "Switch to English"
          : "Chuyển sang tiếng Việt"
      }
      aria-label="Toggle language"
    >
      <FluentTranslateIcon className="size-4 text-indigo-600 dark:text-indigo-400" />
      <span className="text-xs font-semibold uppercase tracking-wider">
        {currentLanguage}
      </span>
    </Button>
  );
}
