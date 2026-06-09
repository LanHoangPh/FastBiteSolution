# Workflow: Desktop Change

Use this for any normal FastBite Desktop code change.

## 1. Read Context

Read in order:

1. `AGENTS.md`
2. `docs/ai-context/DESKTOP_CONTEXT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/UI_GUIDELINES.md`
5. `docs/ai-context/CURRENT_STATUS.md`
6. `.agents/skills/wpf-desktop/SKILL.md`

Do not read backend/root context unless the user explicitly asks for backend or full-solution work.

## 2. Frame the Work

Think from four perspectives:

- Business Analyst: what user problem is being solved?
- Product Manager: does this move the desktop product forward now?
- Product Owner: is the scope clear enough to implement?
- Solution Architect: which layer owns the change?

If the request is ambiguous but still implementable, choose the smallest useful scope and state the assumption.

## 3. Inspect Before Editing

- Check relevant files with `rg --files`, `rg`, and `Get-Content`.
- Identify existing patterns before adding new abstractions.
- Check current worktree state for files in scope.
- Do not revert unrelated user changes.

## 4. Implement

- Keep changes scoped to `src/desktop`.
- Preserve .NET 8 and `net8.0-windows`.
- Keep ViewModels in UI unless architecture is explicitly changed.
- Register services in the owning layer.
- Use `apply_patch` for manual file edits.

## 5. Verify

- Run `dotnet build FastBiteDesktop.slnx`.
- If UI changed, apply `ui-theme-change.md`.
- Finish with `review-verification.md`.

