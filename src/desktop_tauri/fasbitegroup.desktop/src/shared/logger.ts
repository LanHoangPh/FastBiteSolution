/**
 * Cross-platform Logger Service.
 * Outputs to browser console in Web mode, and writes to `<project-root>/logs/app.log` via tauri-plugin-log in Tauri Desktop mode.
 */

const isTauri = typeof window !== "undefined" && Boolean((window as any).__TAURI_INTERNALS__);

let tauriLogModule: any = null;

async function getTauriLog() {
  if (!isTauri) return null;
  if (!tauriLogModule) {
    try {
      tauriLogModule = await import("@tauri-apps/plugin-log");
    } catch (err) {
      console.warn("Failed to load tauri-plugin-log:", err);
    }
  }
  return tauriLogModule;
}

const formatMessage = (message: string, args: any[]): string => {
  const argStr = args.length 
    ? ` ${args.map(a => typeof a === 'object' ? JSON.stringify(a) : String(a)).join(' ')}` 
    : '';
  return `[FE] ${message}${argStr}`;
};

export const Log = {
  debug: async (message: string, ...args: any[]) => {
    console.debug(message, ...args);
    const tauriLog = await getTauriLog();
    if (tauriLog) {
      tauriLog.debug(formatMessage(message, args));
    }
  },

  info: async (message: string, ...args: any[]) => {
    console.info(message, ...args);
    const tauriLog = await getTauriLog();
    if (tauriLog) {
      tauriLog.info(formatMessage(message, args));
    }
  },

  warn: async (message: string, ...args: any[]) => {
    console.warn(message, ...args);
    const tauriLog = await getTauriLog();
    if (tauriLog) {
      tauriLog.warn(formatMessage(message, args));
    }
  },

  error: async (message: string, error?: any, ...args: any[]) => {
    console.error(message, error, ...args);
    const tauriLog = await getTauriLog();
    if (tauriLog) {
      const combinedArgs = error ? [error, ...args] : args;
      tauriLog.error(formatMessage(message, combinedArgs));
    }
  }
};
