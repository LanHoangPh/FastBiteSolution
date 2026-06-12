import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { ThemeProvider } from "@/app/theme-provider";
import "@/shared/i18n/i18n";
import "./styles/globals.css";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <ThemeProvider defaultTheme="system" storageKey="tauri-calc-theme">
      <App />
    </ThemeProvider>
  </React.StrictMode>,
);
