---
name: gpt-taste
description: FastBite WPF Desktop taste guard. Use for modernizing or building WPF desktop UI in this repository. Enforces MVVM, theme tokens, reusable components, and productivity-app UX. Do not generate React, Tailwind, HTML, CSS, or GSAP for this project.
---

# FastBite WPF Desktop Taste Guard

This compatibility skill exists to catch requests that mention "taste" or UI polish. For implementation details, use `.agents/skills/wpf-desktop/SKILL.md` as the source of truth.

This repository is a **WPF desktop application**, not a web landing page. Any agent that loads this skill must produce native WPF/.NET output aligned with `AGENTS.md` and the desktop context files.

## Required Flow

Before proposing or editing code, read these files in order:

1. `.agents/skills/wpf-desktop/SKILL.md`
2. `docs/ai-context/DESKTOP_CONTEXT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/UI_GUIDELINES.md`
5. `docs/ai-context/CURRENT_STATUS.md`

Only read backend or repository-root context when the user explicitly asks for backend or full-solution work.

## Product Direction

FastBite Desktop should feel like a focused communication/workspace tool:

- Quiet, utilitarian, and fast to scan.
- Dense enough for repeated daily work.
- Clear hierarchy without marketing-page composition.
- Native desktop behavior over web-style spectacle.
- Professional Light, Dark, and System theme support.

Do not create hero sections, bento landing pages, scroll-driven storytelling, GSAP animations, Tailwind classes, React components, or web routing.

## UX Rules

- Desktop is not web. Use sidebars, top bars, panes, tabs, toolbars, context menus, keyboard focus, and clear active states.
- Favor dense but readable screens over decorative whitespace.
- Use `Grid` and `DockPanel` for structural layout; avoid deep nested `StackPanel` trees.
- Avoid fixed widths/heights unless the element is truly fixed-format.
- Every interactive state needs a clear hover, pressed, disabled, and focus state.
- Use reusable components already present before inventing new ones: `ModernButton`, `IconButton`, `Avatar`, `StatusBadge`, `SearchBox`, `EmptyState`, and `SectionHeader`.

