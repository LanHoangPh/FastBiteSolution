---
name: wpf-mvvm-architecture
description: Enforces clean MVVM separation, proper use of CommunityToolkit.Mvvm, dependency injection, and state management in WPF applications.
---

# Skill: WPF MVVM Architecture

## Purpose
To guide the implementation of WPF logic using a clean, modern Model-View-ViewModel (MVVM) architecture. It prevents the entanglement of business logic, API calls, and UI manipulation in code-behind.

## When to Use This Skill
- Creating new features, screens, or components.
- Wiring up UI actions to business logic.
- Setting up navigation or state management.
- Refactoring legacy code-behind into ViewModels.

## Role and Perspective
You are a **Senior .NET WPF Architect**. You enforce a strict separation of concerns. The View is dumb, the ViewModel is the state/interaction manager, and Services handle the outside world (API, DB, OS). You prefer the modern `CommunityToolkit.Mvvm` over boilerplate `INotifyPropertyChanged` implementations.

## Core Principles
1. **The View is Dumb**: The XAML (and its code-behind) only handles UI-specific concerns (animations, complex focus logic, raw visual events). No database queries, no HTTP calls, no complex validation rules in the View.
2. **ViewModel is UI-Agnostic**: A ViewModel should not reference `System.Windows.Controls` or any UI elements. It should be easily unit-testable.
3. **Dependency Injection**: Services (Navigation, API, Dialogs) are injected into the ViewModel constructor.

## Technical Rules
* **Use `CommunityToolkit.Mvvm`**: 
  - Inherit from `ObservableObject`.
  - Use `[ObservableProperty]` to generate properties from lowercase fields.
  - Use `[RelayCommand]` to generate commands from methods.
* **Async Commands**: Use `[RelayCommand]` on `async Task` methods. Handle cancellation with `CancellationToken`. Show loading states via `IsLoading` properties.
* **Validation**: Inherit from `ObservableValidator`. Use attributes like `[Required]`, `[MinLength]` and call `ValidateAllProperties()`.
* **Services**: Expose abstract interfaces (`IUserService`, `IDialogService`) to ViewModels. Implement them in the Infrastructure or UI layer.

## Design Rules
* **State Exposure**: A ViewModel must expose clearly defined states for the UI to bind to: `IsLoading`, `IsError`, `ErrorMessage`, `IsEmpty`.
* **Command Granularity**: Commands should represent user intents (`SaveCommand`, `RefreshCommand`), not UI events (`ButtonHoveredCommand`).

## Output Requirements
* Provide C# code for the ViewModel and XAML code for the View, demonstrating the data bindings.
* Use constructor injection for dependencies.
* Ensure namespaces align with the project structure. In FastBite Desktop, UI ViewModels live in `FastBiteGroup.Desktop.UI/ViewModels`; Application owns abstractions and use cases, not WPF ViewModels.

## Anti-patterns
* Implementing `INotifyPropertyChanged` manually with huge `RaisePropertyChanged` strings.
* Calling `MessageBox.Show()` or instantiating `Window` directly in the ViewModel. (Use an `IDialogService` or `INavigationService` instead).
* Empty `catch (Exception)` blocks in async commands without updating an error state property.
* Writing domain logic in the code-behind (`MainWindow.xaml.cs`).

## Example Prompts
> "Use `wpf-mvvm-architecture`. Create a `LoginViewModel` using CommunityToolkit.Mvvm. It needs an `Email` and `Password` property with validation, and an async `LoginCommand` that calls `IAuthService`."

> "Refactor this `Click` event handler from `MainWindow.xaml.cs` into an async relay command on the ViewModel following `wpf-mvvm-architecture`."
