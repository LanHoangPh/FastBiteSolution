# AI Context README - FastBite Desktop App

> Start here when an AI agent or developer needs context on the Tauri Desktop Application. This file maps each document to its purpose and recommends what to read for each type of task.

---

## 1. Context Map

| Document | Primary Question | Use When |
|---|---|---|
| `PROJECT_CONTEXT.md` | What exists in the desktop project? | Understanding local modules, folders, UI screens, and local dev setup |
| `ARCHITECTURE.md` | How is the frontend & native bridge designed? | Checking layer boundaries, Service Layer constraints, and Tauri command bindings |
| `CODING_GUIDELINES.md` | How should desktop UI/code be written? | Creating new features, UI layouts, responsive components, validation, and states |
| `TECH_STACK.md` | Which packages and versions are used? | Checking frontend npm dependencies, Tailwind v4 integration, and Rust dependencies |
| `CURRENT_STATUS.md` | What is done and pending? | Checking current compilation state, implemented screens (Auth, Calculator), and next steps |

---

## 2. Required Baseline For Any Code Change

Before making code changes, read these in order:
1. `PROJECT_CONTEXT.md`
2. `ARCHITECTURE.md`
3. `CODING_GUIDELINES.md`
4. `CURRENT_STATUS.md`

---

## 3. Ownership Rules For Updating Docs

When a change affects project behavior, update the owning document:
- Repository structure or local setup: `PROJECT_CONTEXT.md`
- Architecture boundaries or Tauri bridge: `ARCHITECTURE.md`
- Style guides, styling rules, or formatting: `CODING_GUIDELINES.md`
- Package updates (npm or cargo): `TECH_STACK.md`
- Work completed or build fixes: `CURRENT_STATUS.md`
