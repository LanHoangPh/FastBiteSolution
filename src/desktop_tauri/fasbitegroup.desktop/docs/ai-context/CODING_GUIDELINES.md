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

---

## 4. Forms and Validation

- **No Manual Validation**: Never check empty strings or email formats using manual `if/else` inside components.
- **React Hook Form + Zod**: Bind forms using React Hook Form and enforce constraints with Zod schemas.

Example:
```typescript
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";

const loginSchema = z.object({
  email: z.string().email("Invalid email format"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});
```

---

## 5. Security & Safety

- **No Dangerous Injection**: Avoid `dangerouslySetInnerHTML` to prevent XSS issues.
- **Memory Token Storage**: Never save high-privilege JWT access tokens in `localStorage`. Keep them in memory, and use secure HTTP cookies or secure Tauri native storage adapters for refresh tokens.
- **No Sensitive Logging**: Never use `console.log()` to dump passwords, refresh tokens, or personal identifiers (PII).
