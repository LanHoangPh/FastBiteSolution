# Workflow: MVVM Feature

Use this when adding a screen, command, navigation target, form, dashboard section, or real feature behavior.

## 1. Define Ownership

- UI owns ViewModels, Views, resources, theme, and WPF services.
- Application owns use case abstractions and orchestration contracts.
- Infrastructure owns API/storage implementations.
- Domain owns shared models and domain exceptions only.

Do not move WPF-specific types into Application.

## 2. ViewModel Pattern

- Use `ObservableObject` or `ObservableValidator`.
- Use `[ObservableProperty]` for bindable state.
- Use `[RelayCommand]` for user intents.
- Async commands return `Task`.
- Expose `IsLoading`, `ErrorMessage`, and `IsEmpty` when relevant.
- Avoid direct WPF control references in ViewModels.

## 3. View Pattern

- Bind commands instead of using `Click` handlers.
- Use existing components and theme tokens.
- Keep code-behind to `InitializeComponent`, DataContext/window wiring, and unavoidable WPF interop.
- Use `ContentControl` + DataTemplates for view composition where navigation is involved.

## 4. Application Boundary

When behavior becomes real instead of placeholder UI:

- Add an Application abstraction/use case if feature orchestration is not UI-only.
- Return clear result models or typed responses.
- Keep backend contracts out of XAML and code-behind.

## 5. Verify

- Build `FastBiteDesktop.slnx`.
- Confirm no new dependency violates layer rules.
- Confirm placeholder data is labeled or removed when real data is wired.

