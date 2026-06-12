# AGENTS.md - FastBite Desktop Project Memory

> AI Agent Quick Reference for the Tauri Desktop Frontend. Read this first.
> Detailed application rules and design guidelines live in `TAURI_REACT_SHADCN_RULES.md`.

---

## Project Summary

**fasbitegroup.desktop** is the desktop client application for the **FastBiteGroup** Unified Communication Platform (a hybrid Microsoft Teams/Slack + Facebook Workplace/Yammer enterprise solution). It is built using Tauri v2 as the native wrapper (Rust backend) and React + Tailwind CSS v4 + shadcn-ui as the user interface (Webview frontend).

- **Architecture**: Feature-Based Architecture (frontend) + Adapter/Command pattern (Rust backend)
- **Frontend Stack**: React 19, TypeScript, Vite, Tailwind CSS v4, shadcn-ui (Nova preset)
- **Backend Stack**: Rust, Tauri v2
- **State Management**: Zustand (UI state) / React State
- **Form/Validation**: React Hook Form + Zod
- **Routing/Navigation**: React Router (if expanded) / View switching state
- **Tauri Integration**: Service Layer wrappers over direct Tauri `invoke` calls

---

## Build & Run Commands

```bash
# Install package dependencies
pnpm install

# Run the Tauri application in Development Mode
pnpm tauri dev

# Build the frontend static assets
pnpm run build

# Compile and package the Tauri desktop application for release
pnpm tauri build
```

---

## Development Rules

### Folder Structure Constraints
All frontend code resides under the `src/` directory, adhering strictly to Feature-Based Architecture:
- `src/app/`: Providers (theme, state), entry components, global routing.
- `src/styles/`: Global stylesheets (like `globals.css` using Tailwind `@import "tailwindcss";`).
- `src/shared/`: Shared, reusable assets:
  - `src/shared/components/ui/`: shadcn-ui components.
  - `src/shared/components/common/`: Project-specific common UI patterns (DataGrid, ConfirmDialog, etc.).
  - `src/shared/utils.ts`: Shared utility functions (e.g., `cn` class merger).
- `src/features/`: Feature-isolated directories (e.g., `features/auth/`, `features/calculator/`). Each contains its own `components/`, `hooks/`, `services/`, and `types/` if applicable.
- `src/services/`: Service Layer wrapping external APIs and Tauri commands.
- `src/types/`: Global TypeScript definitions.

### Key Coding Rules
1. **Never use `invoke` directly in UI**: Wrap all Tauri commands in a service file under `src/services/` (e.g., `greet-service.ts`) and import the service function into the React component instead.
2. **Responsive Styling (Tailwind v4)**: Mobile-first mindset using Tailwind breakpoints (`sm:`, `md:`, `lg:`). Giao diện phải co giãn tốt và không được vỡ hay đè nút khi thu nhỏ cửa sổ ứng dụng về `900x600`.
3. **TypeScript Strictness**: `"strict": true` is enabled. Do not use `any` unless absolutely necessary; prefer `unknown` and validate.
4. **Form Validation**: Use React Hook Form + Zod for complex forms, or Centralized TS Validators under `src/shared/validation/` (returning i18n translation keys) for lightweight forms. Error fields must display red borders and translation labels.
5. **Theme Variables**: Color tokens, spacing, and border radius must utilize CSS Variables in `src/styles/globals.css`. Support both Dark and Light modes.
6. **No custom raw CSS/inline styles**: All custom UI styling must be declared centrally in the global CSS (`globals.css`) utilizing theme variables to support light/dark modes. Inline styles or component-specific CSS files are forbidden.
7. **Shared SVG Icons**: Never paste raw `<svg>` structures directly into pages/features. Wrap them in [SvgIcon.tsx](file:///d:/CodeVs/FastBiteSolution/src/desktop_tauri/fasbitegroup.desktop/src/shared/components/ui/svg-icon.tsx) and define custom icon components under `src/shared/components/icons/`.
8. **Logging Conventions**: Frontend logging must utilize the shared logger wrapping `@tauri-apps/plugin-log` which outputs to the `/logs/app.log` file in the project root directory during native execution.

---

## AI Agent Rules (Strict)

### 1. Scope Restriction
- **Work Directory Only**: Restrict your operations and scans to the current desktop project directory (`src/desktop_tauri/fasbitegroup.desktop/`).
- **Do NOT query the master backend/root directory** unless the user explicitly requests it.

### 2. Follow Architecture Guidelines
- Ensure any newly added UI components are stored in `src/shared/components/ui/` or `src/shared/components/common/`.
- Ensure new shadcn components match the updated paths in `components.json`.
- Adhere strictly to the structured response format for technical answers: Problem Analysis -> Proposed Architecture -> Detail Implementation -> Sample Code -> Best Practices.

### 3. Before Making Any Changes (Mandatory)
Before starting any task or making code modifications, AI agents MUST read the following context and rule files in order:
1. `docs/ai-context/README.md`
2. `docs/ai-context/PROJECT_CONTEXT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/CODING_GUIDELINES.md`
5. `TAURI_REACT_SHADCN_RULES.md`
