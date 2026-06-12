# TECH_STACK.md - FastBite Desktop App

## Runtime Environments

| Component | Version | Role |
|---|---|---|
| Node.js | v20+ (recommended) | Package management & dev server runtime |
| Rust / Cargo | rustc 1.80+ / cargo 1.80+ | Tauri desktop app native compilation |

---

## Core Frontend Stack

| Library | Version | Purpose |
|---|---|---|
| React | ^19.1.0 | Component rendering |
| React-DOM | ^19.1.0 | DOM binding |
| TypeScript | ~5.8.3 | Static typing |
| Vite | ^7.0.4 | Bundler & Development Server |
| Tailwind CSS | ^4.3.1 | Utility CSS framework |
| @tailwindcss/vite | ^4.3.1 | Vite integration plugin for Tailwind v4 |
| framer-motion | ^12.4.0 | UI transitions and micro-animations |
| lucide-react | ^0.450.0 | Icon library |
| class-variance-authority | ^0.7.0 | Component variant utility (CVA) |
| clsx / tailwind-merge | latest | Class merging utilities |

---

## Core Native Stack (Tauri / Rust Backend)

| Package | Version | Purpose |
|---|---|---|
| tauri | ^2.0.0 | Tauri app container runtime |
| tauri-build | ^2.0.0 | Tauri build scripts |
| tauri-plugin-opener | ^2.0.0 | System utility bindings |
| serde / serde_json | ^1.0 | Serialization & Deserialization |
| time | 0.3.47 | Date and time handling (pinned to v0.3.47 to avoid E0119 trait conflict) |

---

## Tooling & CLI

| Tool | Purpose |
|---|---|
| pnpm | Primary package manager (v10+) |
| shadcn-ui CLI | Scaffold new radix-ui components |
| tauri-cli | Manage, compile, and run Tauri applications |
