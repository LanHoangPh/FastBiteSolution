# Workflow: Review And Verification

Use this before final handoff.

## 1. Diff Review

- Review only files in scope.
- Do not revert unrelated changes.
- Confirm no backend/root files changed unless requested.
- Confirm no package version changes unless explicitly requested.

## 2. Architecture Review

- Domain has no dependency on other desktop layers.
- Application depends on Domain only.
- Infrastructure depends on Application + Domain.
- UI depends on Application + Infrastructure.
- Views/components do not call API clients directly.
- Services are registered in the owning layer.

## 3. UI Review

If UI changed:

- No raw hex colors outside Light/Dark theme dictionaries.
- No fixed `Foreground="White"` or `Background="#..."`.
- Theme-aware resources use `DynamicResource`.
- Light and Dark mode are considered.
- Popups/menus/dropdowns have explicit styling if touched.
- Reusable components expose dependency properties.

## 4. Build

Run:

```powershell
dotnet build FastBiteDesktop.slnx
```

If build fails because the app is running and locking DLLs, close `FastBiteGroup.Desktop.UI` and build again.

## 5. Final Response

Report:

- What changed.
- Why it was done this way.
- Verification performed.
- Any skipped verification and why.

