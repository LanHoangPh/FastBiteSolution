# CODING_GUIDELINES.md - FastBite Desktop App

## 1. Naming Conventions

### File Names
- **React Components**: PascalCase (e.g. `LoginForm.tsx`, `Calculator.tsx`)
- **React Hooks**: camelCase starting with `use` (e.g. `useTheme.ts`, `useAuth.ts`)
- **Services & APIs**: kebab-case (e.g. `greet-service.ts`, `auth-api.ts`)
- **Types**: kebab-case (e.g. `user-types.ts`)

### Code Symbols
- **Interfaces & Types**: PascalCase (e.g. `interface UserDto`, `type ViewType`)
- **Variables & Functions**: camelCase (e.g. `const [email, setEmail]`, `function handleSubmit()`)

---

## 2. Component Design & State Rules

- **Pure Rendering**: Components should focus on rendering UI. Extracted event handlers, validation logic, and complex state mutation belong to custom Hooks or Service modules.
- **State Partitioning**:
  - **Local UI State**: Use React `useState` or `useReducer` for UI switches (e.g., open sidebar, toggle tabs).
  - **Global/Persistent UI State**: Use Zustand (in `src/stores/`) for cross-cutting configurations (e.g., theme settings).
  - **Server State**: Always fetch remote REST API data using TanStack Query. Avoid copying API fetch payloads into Zustand.

---

## 3. Styling & Tailwind CSS v4 Rules

- **Mobile-First Breakpoints**: Always write standard utility classes first, then scale using `md:`, `lg:`, etc.
- **No Class Duplication**: If a Tailwind pattern becomes too verbose (e.g. >100 characters repeated in multiple spots), extract it into a separate React component or use class merging utilities:
  ```typescript
  import { cn } from "@/shared/utils";
  
  // Good: Merging classes cleanly
  const className = cn("text-sm text-foreground", customClasses);
  ```
- **CSS Variables Only**: Do not hardcode specific hex colors (like `#0f172a`) directly in the codebase. Reference theme tokens defined in `globals.css` (e.g. `bg-card`, `text-primary`).
- **No Hardcoded CSS/Styles**: Do not write custom inline styles (`style={{...}}`) or local CSS files for React components. All custom UI styling must be declared centrally in the global CSS (`globals.css`) utilizing theme variables to natively support both Light and Dark modes.

---

## 4. Forms and Validation

- **Separation of Validation**: Never write manual validation logic (like check empty strings, length, or regex patterns) inside the submit handler of UI components.
- **Validation Options**:
  1. **React Hook Form + Zod**: For complex business forms.
  2. **Centralized TS Validators**: For lightweight, multi-language modules. Put validators under `src/shared/validation/` returning i18n translation keys.
- **Field-level Error Feedback**:
  - Show a red border on input fields with errors using `cn("auth-input", errors.field && "border-red-500")`.
  - Render a small red translation label directly below the field: `<p className="text-red-500">{t(errors.field)}</p>`.

---

## 4.1 Browser dev compatibility & Custom inputs

- **Lazy Import Tauri APIs**: To prevent crashes on standard web browsers (Web Dev mode), do not use top-level imports for Tauri plugins (e.g. `@tauri-apps/plugin-http` or `@tauri-apps/api/*`). Instead, check `isTauri` at runtime and dynamically import them:
  ```typescript
  const module = await import("@tauri-apps/plugin-http");
  ```
- **Custom Input Behaviors**:
  - Hiding browser overlays: Edge password reveal buttons (`::-ms-reveal`) and native input indicators must be hidden via `!important` CSS rules.
  - Date picker popup: Remove native indicators on `input[type="date"]` and trigger `.showPicker()` programmatically using a React `ref` when clicking either the input field or its starting icon wrapper.

---

## 4.2 Shared SVG Icon Wrapper Pattern

- **Avoid raw inline SVGs**: Never paste raw `<svg>` structures directly into feature screens or layout files.
- **Use SvgIcon Wrapper**: Use the shared [SvgIcon.tsx](file:///d:/CodeVs/FastBiteSolution/src/desktop_tauri/fasbitegroup.desktop/src/shared/components/ui/svg-icon.tsx) component to encapsulate custom SVGs.
- **Encapsulate Reusable Icons**: Create dedicated React components for each external SVG under `src/shared/components/icons/` (e.g., `FluentTranslateIcon.tsx`).
  - Override `viewBox` to match the source SVG coordinates.
  - Set `fill="currentColor"` and `stroke="none"` (with `strokeWidth={0}`) for fill-based (shape) paths.
  - Expose default standard SVG attributes by omitting the `children` prop.

---

## 4.3 Logging Architecture & Conventions

- **No console.log in Production**: Avoid leaving raw `console.log()` statements in component files.
- **Use Native Tauri Logger**: Utilize the Tauri Logger plugin (`@tauri-apps/plugin-log`) in desktop native environment which dynamically logs output to the project directory `/logs/app.log`.
- **Logger Wrapper**: Use the centralized frontend logger utility for logging error, info, and warn events, which auto-detects browser dev environment vs Tauri native runtime.

---

## 5. Security & Safety

- **No Dangerous Injection**: Avoid `dangerouslySetInnerHTML` to prevent XSS issues.
- **Memory Token Storage**: Never save high-privilege JWT access tokens in `localStorage`. Keep them in memory, and use secure HTTP cookies or secure Tauri native storage adapters for refresh tokens.
- **No Sensitive Logging**: Never use `console.log()` to dump passwords, refresh tokens, or personal identifiers (PII).
