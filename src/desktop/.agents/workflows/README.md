# FastBite Desktop Agent Workflows

These workflows are playbooks for Codex, Antigravity, and other AI coding agents working under `src/desktop`.

Always apply `AGENTS.md` and `.agents/skills/wpf-desktop/SKILL.md` first. Use the workflow that best matches the task.

## Available Workflows

- `desktop-change.md`: default workflow for any desktop code change.
- `ui-theme-change.md`: WPF UI, theme, resource, and component changes.
- `mvvm-feature.md`: new screen or feature with ViewModel/Application/Infrastructure boundaries.
- `api-integration.md`: backend API consumption through Application abstractions and Infrastructure implementations.
- `review-verification.md`: final review, build, and handoff checklist.

## Selection Guide

- Use `desktop-change.md` for general edits.
- Add `ui-theme-change.md` when XAML/resources/components change.
- Add `mvvm-feature.md` when adding real user workflow behavior.
- Add `api-integration.md` when HTTP/backend access is involved.
- Always finish with `review-verification.md`.

