import { invoke } from "@tauri-apps/api/core";

/**
 * Greet command wrapper
 * @param name User name to greet
 * @returns Greet message string from Rust backend
 */
export async function greet(name: string): Promise<string> {
  return invoke<string>("greet", { name });
}
