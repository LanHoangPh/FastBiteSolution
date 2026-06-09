---
name: wpf-navigation-shell
description: Architecture and design patterns for main desktop application shells, sidebar routing, dialog management, and view composition.
---

# Skill: WPF Navigation Shell

## Purpose
To create a robust, maintainable application shell that coordinates navigation, top bars, sidebars, and dialogs. Desktop navigation differs from web URLs; it relies on View composition, ViewModel resolution, and state preservation.

## When to Use This Skill
- Designing the `MainWindow.xaml` shell.
- Implementing sidebar or tab-based navigation.
- Creating a Navigation Service.
- Managing modal and modeless dialog overlays within the main window.

## Role and Perspective
You are a **Desktop Systems Architect**. You view the application shell as a host for independent modules. You decouple the navigation logic from the Views so that ViewModels can request navigation without knowing about WPF specific frames or windows.

## Core Principles
1. **ViewModel-First Navigation**: Navigation is driven by changing a `CurrentViewModel` property on the `MainViewModel`, not by explicitly instantiating `Page` or `UserControl` objects in code-behind.
2. **DataTemplates for Routing**: WPF resolves the View for a ViewModel using implicit `DataTemplate` declarations in the App resources or MainWindow resources.
3. **Region Composition**: The shell is divided into distinct regions: Sidebar (Navigation), TopBar (Search, User Profile), ContentArea (The active view), and Overlay (Dialogs/Notifications).

## Technical Rules
* **ContentControl Hosting**: Use `<ContentControl Content="{Binding CurrentViewModel}" />` to host the active view.
* **Navigation Service**: Create an `INavigationService` injected into ViewModels. Example interface: `NavigateTo<TViewModel>()`.
* **Sidebar Binding**: Bind sidebar `ListBox` or custom navigation items to a collection of available routes. Bind the `SelectedItem` to the navigation command.
* **Dialog Hosting**: For custom dialogs, overlay a `Grid` with a semi-transparent background and a `ContentControl` for the dialog content, bound to a `DialogViewModel`.

## Design Rules
* **Sidebar Aesthetics**: The sidebar should be distinctly styled (often darker or slightly offset in color) to anchor the application. Include a brand header/logo.
* **Active State**: Navigation items must clearly show their "Active" or "Selected" state using high-contrast indicators (e.g., a left accent border or distinct background).
* **Breadcrumbs / Top Bar**: Contextualize the user. The Top Bar should display the title of the current view or breadcrumbs if deep-linked.

## Output Requirements
* Output the `MainWindow.xaml` demonstrating the structural regions.
* Output the `MainViewModel` handling the current state.
* Output the implicit `DataTemplate` mapping to connect ViewModels to Views.
* Provide the basic interface for `INavigationService`.

## Anti-patterns
* Using the old WPF `Frame` and `Page` navigation (`NavigationService.Navigate(new Uri(...))`) which tightly couples UI and breaks MVVM.
* Popping open new physical OS Windows (`new Window().Show()`) for every screen. Use a single-window application shell with view swapping.
* Hardcoding menu items that cannot be dynamically filtered by user roles/permissions.

## Example Prompts
> "Implement a main shell using `wpf-navigation-shell`. It should have a collapsible sidebar, a top status bar, and a main content area driven by ViewModel-first navigation using ContentControl."

> "Refactor this multi-window WPF app into a single-window shell using `wpf-navigation-shell` and an INavigationService."
