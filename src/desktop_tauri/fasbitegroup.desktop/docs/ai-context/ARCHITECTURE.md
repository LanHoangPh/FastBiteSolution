# ARCHITECTURE.md - FastBite Desktop App

## Architecture Style

The desktop client utilizes a **Three-Tier Architecture** for the frontend Webview and links to the native Rust platform via a **Service Layer (Tauri Bridge)**.

```text
UI (React Components)
  |-- (Prop events & Local UI states)
  v
Feature Components / Logic
  |-- (Zustand Stores / React Hook Form / Zod)
  v
Service Layer (TypeScript Wrappers)
  |-- (Tauri invoke API / HTTP client)
  v
Tauri IPC Bridge
  |-- (Serialized payload)
  v
Rust Native Commands (lib.rs)
  |-- (Rust backend logic)
```

---

## Folder Isolation Constraints

To enforce ease of maintenance and extensibility, the project strictly isolates dependencies:
- **Shared/UI**: `src/shared/components/ui/` has zero dependencies on features or business services. They are pure presentation slots.
- **Features**: Features under `src/features/` must remain independent from other features. If `featureA` needs logic from `featureB`, extract it into `src/shared/` or pass it down via props/services.
- **Service Layer**: The folder `src/services/` holds all Tauri command wrappers and external API callers. No UI styling or components belong here.

---

## Tauri IPC Bridge Pattern

To ensure code readability and type safety, all interactions with Tauri native commands are wrapped inside custom TypeScript functions.

### Bad Practice:
```typescript
// Do NOT invoke directly inside components
const message = await invoke("greet", { name });
```

### Good Practice:
```typescript
// Call from a dedicated service
import { greet } from "@/services/greet-service";
const message = await greet(name);
```
This abstracts native details away from the rendering code, allowing the team to swap mock data or test UI components in pure web browsers without Tauri environment errors.

---

## Responsive Layout Design

The styling system enforces **Mobile-First Responsive Styling** utilizing Tailwind CSS v4 breakpoints:
- Base styles (default) targeted at small screens (`sm`).
- Grid structures, padding, and layout shifts optimized dynamically as the window expands to `md`, `lg`, and `xl`.
- Layouts must be elastic (`max-w-7xl`, `min-h-screen`) and not rely on hardcoded static coordinates (`w-[1200px]`, `h-[800px]`), allowing smooth window resizing.
