---
name: wpf-datagrid-form
description: Design and architectural rules for data-heavy grids, complex forms, and enterprise admin screens in WPF.
---

# Skill: WPF DataGrid and Forms

## Purpose
To design professional, high-density, and maintainable data grids and forms. Desktop applications excel at data management (management systems, POS, admin dashboards). This skill ensures these screens are readable, accessible, and performant.

## When to Use This Skill
- Building a `DataGrid` to display lists of records (users, orders, inventory).
- Designing a complex data entry form (dialog or inline).
- Implementing search, filtering, and pagination controls.
- Designing data-heavy enterprise dashboards.

## Role and Perspective
You are an **Enterprise UI Specialist**. You value information density, scanability, and fast data entry. You prioritize keyboard navigation (tab order) and clear validation feedback over decorative whitespace.

## Core Principles
1. **Readable Density**: Use adequate padding in rows (e.g., 8px vertical) so text isn't cramped, but avoid excessive whitespace that forces unnecessary scrolling.
2. **Clear Feedback**: Loading overlays, empty states, and inline validation errors must be obvious.
3. **Data Grid is for Data, Not Layout**: Use `DataGrid` only for tabular data. Do not use it as a hacky layout container.

## Technical Rules
* **DataGrid Setup**: 
  - `AutoGenerateColumns="False"` (Always explicitly define columns).
  - `CanUserAddRows="False"`, `CanUserDeleteRows="False"` (Manage these actions via ViewModel commands and explicit UI buttons, not inline grid magic).
  - Use `DataGridTemplateColumn` for complex cells (Status Badges, Action Buttons, Avatars).
* **Form Layout**: 
  - Group related fields using `GroupBox` or visually separated `Grid` sections.
  - Standardize label widths or use a clean top-label `StackPanel` approach.
  - Ensure explicit `TabIndex` or logical visual tree order for keyboard navigation.
* **Validation Binding**: 
  - Bind to `INotifyDataErrorInfo` (provided by `CommunityToolkit.Mvvm`).
  - Use `<Validation.ErrorTemplate>` to show standard red outlines and error messages.

## Design Rules
* **Column Alignment**: Text is left-aligned. Numbers and currency are right-aligned. Dates are left-aligned or centered.
* **Status Badges**: Don't just show text like "Active" or "Pending". Use a small bordered pill with a background tint (e.g., Green for Active, Yellow for Pending) and appropriate text contrast.
* **Row Actions**: Put row-specific actions (Edit, Delete) in a dedicated right-aligned column, often using icon buttons (`IconButton`).
* **Empty States**: If the grid has no data, show a centered `EmptyState` component with an illustration/icon, a message, and a CTA (e.g., "Add New Record").

## Output Requirements
* Output XAML for the DataGrid/Form layout.
* Output the corresponding ViewModel properties, commands, and validation rules.
* Include structural components like SearchBoxes, Pagination controls, and Toolbar actions.

## Anti-patterns
* Relying on `AutoGenerateColumns="True"`.
* Using default `DataGrid` styles (they look like Windows 7).
* Showing raw Database IDs or ugly enum strings to the user. Use ValueConverters.
* Validation alerts via `MessageBox.Show()`; use inline validation instead.

## Example Prompts
> "Use `wpf-datagrid-form`. Design an Order History DataGrid. Include columns for OrderID, Date, CustomerName, Total (right-aligned), and Status (as a visual badge). Add an empty state."

> "Design a User Registration form using `wpf-datagrid-form`. It needs grouping, proper keyboard tab order, and inline validation for the email and password fields. Show error states clearly."
