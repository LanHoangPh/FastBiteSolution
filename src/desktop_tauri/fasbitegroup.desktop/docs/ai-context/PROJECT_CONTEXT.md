# PROJECT_CONTEXT.md - FastBite Desktop App

## What This Project Does

**fasbitegroup.desktop** is the desktop client frontend for the **FastBiteGroup** Unified Communication Platform. Currently, it acts as an onboarding client containing:
- **Authentication Forms**: Modern Neumorphic Login and Registration screens.
- **Calculator Module**: An interactive Neumorphic Calculator dashboard featuring complete history logging, theme toggles, and memory functions.
- **Tauri Native Bridge**: Connects the React frontend with the Rust backend, utilizing wrapped Tauri command services.

---

## Repository Structure (Current Desktop Project)

All desktop-related directories are self-contained under `src/desktop_tauri/fasbitegroup.desktop/`:

```text
fasbitegroup.desktop/
  TAURI_REACT_SHADCN_RULES.md
  AGENTS.md
  components.json               # shadcn config
  package.json                  # npm scripts and package list
  tsconfig.json                 # TypeScript path aliases config
  vite.config.ts                # Vite config with Tailwind v4 & alias
  src/
    main.tsx                    # React App entry point
    App.tsx                     # Main layout & router/view switch
    App.css                     # Old local CSS (preserved)
    globals.css                 # Main global styles (Tailwind v4 directive)
    vite-env.d.ts
    app/
      theme-provider.tsx        # Dark/Light theme manager
    styles/
      globals.css               # Restructured global styles location
    shared/
      utils.ts                  # cn classnames utility
      components/
        ui/                     # shadcn-ui components (button, card, input, separator, scroll-area)
    features/
      auth/                     # Auth login & register feature
        components/             # AuthCard, RegisterCard, SocialLoginButtons, AuthBackground, Logo, PasswordInput
      calculator/               # Calculator dashboard feature
        components/             # Calculator component
    services/
      greet-service.ts          # Tauri greet command wrapper service
  src-tauri/                    # Rust backend project folder
    Cargo.toml                  # Rust cargo configuration
    Cargo.lock                  # Pinned Rust dependencies
    tauri.conf.json             # Tauri configuration
    src/
      main.rs                   # Rust entry point
      lib.rs                    # Rust commands and lib setup
```

---

## Development Workflow

1. **Adding UI Components**:
   Install shadcn components using:
   ```bash
   pnpm dlx shadcn@latest add <component-name>
   ```
   This will automatically place files in `src/shared/components/ui/` and link utility paths correctly as configured in `components.json`.

2. **Creating Features**:
   Create a new directory under `src/features/<feature-name>/`. Place components under `components/`, hooks under `hooks/`, and services under `services/`.

3. **Interacting with Tauri Native Commands**:
   If a new command is added in Rust (`src-tauri/src/lib.rs`):
   - Wrap it in a TypeScript service file under `src/services/` (e.g. `src/services/auth-service.ts`) using the `invoke` API.
   - Import and call the service function in the UI. Never use `invoke` directly in UI components.

4. **Biên dịch và Chạy thử**:
   Run `pnpm tauri dev` to test locally or `pnpm run build` to verify production assets compile cleanly.
