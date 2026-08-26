# G-CAT - Avalonia Frontend, by Nads, Eron, and Klode

Frontend-only implementation of the G-CAT budgeting app UI, built with Avalonia UI (C# / .NET 8).
No backend logic is wired up - all data on the dashboard is placeholder data set directly in
`ViewModels/DashboardViewModel.cs`, ready to be swapped for real data once you wire up your
backend/API/data layer.

## URGENT, BUG FIX POOO

- Hello, yung dashboard is nagsshow din for the budget and settings menu HAHAHAHAHAHAA
paayos nalang pls!!! thx po - nads

## What's implemented

1. **Splash screen** (`Views/SplashWindow.axaml`) - matches image 1. Fades in, holds, fades out,
   then opens the main window. Animation is a simple `Opacity` `DoubleTransition` driven from
   code-behind (`SplashWindow.axaml.cs`) - no external animation library needed.
2. **Dashboard** (`Views/DashboardView.axaml`) - matches image 2. Sidebar nav, summary cards,
   a hand-drawn donut chart for "expenses overview" (`Views/Controls/DonutChart.cs`, a custom
   `Control` - no charting package dependency), a bar chart for "income vs expenses", recent
   transactions list, and budget overview with progress bars.
3. **Month picker popup** (`Views/Controls/MonthPickerPopup.axaml`) - matches image 3. Triggered
   by either the header "August 2026" button or the "this month" dropdown on the expenses panel.
   Darkens the background behind it and closes on the X button or by clicking outside the card.

## Project structure

```
GCAT.csproj
Program.cs                     - Avalonia entry point
App.axaml / App.axaml.cs       - app bootstrap, opens SplashWindow first

Models/                        - plain display models (all placeholder data lives here)
ViewModels/                    - INotifyPropertyChanged view models + a tiny RelayCommand
Converters/                    - PathToBitmapConverter (icon loading), RatioToHeightConverter (bar chart)
Views/
  SplashWindow.axaml           - image 1
  MainWindow.axaml             - sidebar shell, hosts DashboardView
  DashboardView.axaml          - image 2 + the popup overlay
  Controls/
    DonutChart.cs              - custom-drawn donut chart control
    MonthPickerPopup.axaml     - image 3

Styles/Theme.axaml             - all colors, gradients, and control styles in one place
Assets/Icons/                  - icons already dropped
```

## Placeholder data & numbers

Every number in the UI (₱77,777.77 balances, ₱10,000 "Food" line items, the May-August bar
chart, etc.) is a placeholder set in `DashboardViewModel`'s constructor. Swap those collections
for real data (e.g. populated from your backend) once it's wired up - the bindings will just
pick up whatever you put in the `ObservableCollection`s.

## Icons

Referenced icon paths (sidebar nav, category icons, the app logo) are just placeholder
filenames pointing at `Assets/Icons/`. All icons are present na ^^

## Running it

You'll need the .NET 8 SDK. From the project folder:

```bash
dotnet restore
dotnet run
```

This will fetch the Avalonia NuGet packages (Avalonia, Avalonia.Desktop, Avalonia.Themes.Fluent,
Avalonia.Fonts.Inter) - an internet connection is required for the first `dotnet restore`.

## Notes / things you may want to adjust

- The other sidebar items (transactions, budget, settings) are present in the nav
  but not wired to separate views yet - only "dashboard" has content. Add more `UserControl`s
  and swap `MainWindow`'s content area based on `SelectedNavItem` when you're ready to build
  those out.
- Colors/gradients all live in `Styles/Theme.axaml` as named resources (`CardGradientBrush`,
  `AccentGradientBrush`, etc.) - tweak them there to fine-tune the exact shades from your mocks.
- The rounded, slightly playful font in the mockups isn't bundled (no font files were provided).
  `Theme.axaml` currently falls back to "Segoe UI, Inter, sans-serif" - drop a font file into
  `Assets/Fonts/` and update the `FontFamily` setters if you want an exact match.
