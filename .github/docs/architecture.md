---
status: Active
title: Architecture
purpose: Describes how the application is designed and how major systems work.
audience: Humans and AI.
update_frequency: Occasionally.
contains:
  - System architecture
  - Data flow
  - Major components
  - Design rationale
does_not_contain:
  - Coding standards
  - Current feature work
---

# Overview

## Application Overview

A small, single-purpose .NET MAUI app (Android-only) that performs live QR scanning, persists scan results locally, and provides simple actions (Open/Copy/Share). The app is MVVM-based, Shell-navigated (TabBar for Scanner and History), and targets a minimal, dark-themed UI for quick scanning workflows.

## Tech Stack

| Component | Package | Version |
|-----------|---------|---------|
| Framework | .NET MAUI | 10.0 |
| Scanning | ZXing.Net.Maui.Controls | 0.10.3 |
| Persistence | sqlite-net-pcl | 1.9.2 |
| SQLite Native Bundle | SQLitePCLRaw.bundle_green | 2.1.2 |
| MVVM | CommunityToolkit.Mvvm | 8.4.2 |
| Logging | Microsoft.Extensions.Logging.Debug | 10.0.0 |
| Target Platforms | net10.0-android, net10.0-windows10.0.19041.0 | |
| Minimum Android | Android | 21.0 |

## High-Level Architecture

- UI: .NET MAUI with Shell and TabBar (Scanner, History). ResultDetail is a routed page pushed on top of tabs.
- Pattern: MVVM using CommunityToolkit.Mvvm source generators for ViewModels and Commands.
- Scanning: ZXing.Net.MAUI camera barcode reader constrained to QR format with a 2s debounce.
- Persistence: SQLite via `sqlite-net-pcl` and `SQLitePCLRaw.bundle_green` (see Persistence section).
- Services: Concrete, small services (ScannerService, SqliteStorageService) registered in DI — no interface indirection for MVP.

## Project Structure

- UIApp\Views\ — Pages: ScannerPage, HistoryPage, ResultDetailPage
- UIApp\ViewModels\ — ScannerViewModel, HistoryViewModel, ResultDetailViewModel
- UIApp\Models\ — ScanResult.cs
- UIApp\Services\ — SqliteStorageService.cs, ScannerService.cs
- UIApp\Resources\ — Styles, Colors
- Platforms\Android\ — Manifest permissions, platform tweaks

# Core Systems

## MVVM

### Views

Pages are thin shells that bind to ViewModels via x:DataType and expose UI-only logic (animations, visual states). Keep code-behind minimal: only UI lifetime hooks and small view-only helpers.

### ViewModels

ViewModels hold state, expose Observable properties, and RelayCommands for actions. Use [QueryProperty] for Shell route parameter binding on ResultDetailViewModel.

### Models

The primary model is ScanResult (Id, RawValue, Type, ScannedAt, IsFavorite). Keep models POCO and attribute-decorated for SQLite mapping.

### Services

- SqliteStorageService: concrete async CRUD for ScanResult. Responsible for table creation and schema compatibility.
- ScannerService: wraps ZXing camera view events, applies SCAN_DEBOUNCE_MS, and exposes a simple event or callback for decoded payloads.
- Platform helpers: permission requester (CAMERA), Launcher/Clipboard/Share wrappers called directly from ViewModels when needed.

## Dependency Injection

Register services and ViewModels in MauiProgram.cs. Ensure `SQLitePCL.Batteries_V2.Init();` runs before any DB access. Use transient ViewModel lifetimes where appropriate and singleton for long-lived services (storage service, scanner service if it manages shared resources).

## Navigation / UI Alerts

Decision: All user-facing alerts use `Shell.Current.DisplayAlertAsync(...)`.
Never `Application.Current.MainPage.DisplayAlert(...)`.

Why

`Application.MainPage` was obsoleted in .NET MAUI 9 as part of the
framework's move to multi-window support (Window.Page replaces the old
single-page-per-app assumption — see Microsoft's MAUI 9 release notes).
Since the app is Shell-based (Shell > TabBar > ShellContent), `Shell.Current`
is always available once the app starts, requires no null-forgiving
operators, and needs no Window-index lookup.

No IAlertService/IDialogService abstraction for MVP — revisit only if
ViewModel unit testing requires mocking alerts, or call sites grow beyond
simple error messages.

## Navigation

Use Shell routes and `Shell.Current.GoToAsync(...)`. Register `resultdetail` as a route in AppShell and pass ids via query parameters. Rely on Shell's tab-aware back stack behavior.

## Persistence / SQLite

Data Persistence: SQLite

Library choice: `sqlite-net-pcl` (not EF Core)

Why

The app's persistence needs are a single flat table — scan history records with tap-to-reopen and swipe-to-delete — with no relational complexity, migrations, or LINQ-heavy querying required. `sqlite-net-pcl` provides an attribute-based, async-first ORM (`SQLiteAsyncConnection`) that maps directly onto MAUI's async patterns with minimal boilerplate: define a POCO with `[PrimaryKey]/[AutoIncrement]/[Indexed]` attributes, call `CreateTableAsync<T>()` once, then use `InsertAsync`, `GetAllAsync`, `DeleteAsync` for CRUD. It's also the de facto standard for local storage in MAUI/Xamarin.Forms apps, which matters for agent handoff — the implementation pattern is heavily represented in documentation, reducing ambiguity during implementation.

Rejected alternative: `Microsoft.EntityFrameworkCore.Sqlite` was considered and rejected for MVP scope: EF Core's `DbContext`, change tracking, and migration tooling solve problems this app doesn't have. A single-entity data model doesn't justify the added dependency weight and ceremony.

Required packages

- `sqlite-net-pcl`
- `SQLitePCLRaw.bundle_green` — the native SQLite binary bundle. `sqlite-net-pcl` is a wrapper around SQLitePCLRaw's interop layer; without a bundle providing the native `libsqlite3` binaries for the target platform, the app throws at runtime when opening a connection. `bundle_green` is the standard "just works" bundle covering Android/iOS/Windows without needing platform-specific SQLCipher or custom builds.

Required startup step

In `MauiProgram.cs`, before any database access:

```csharp
// Initialize the native SQLite provider
SQLitePCL.Batteries_V2.Init();
```

Notes

- Omitting the `Batteries_V2.Init()` call is a common pitfall on Android — the app will often crash on first DB access with a `DllNotFoundException` or similar native binding error because Android won't implicitly resolve the native library the way some other platforms do.
- Keep the persistence wiring in `MauiProgram.cs` and register the storage service in DI so it's initialized consistently across app startup.

See also: `.backup/about-splite-package.md` (archived)

## Save Pipeline

### Save Flow

The SqliteStorageService exposes a single `SaveAsync(ScanResult)` method. ViewModels call SaveAsync after a validated scan; the service is responsible for scheduling/coalescing writes if needed in the future, but MVP performs direct async inserts.

### Save Responsibilities

- ViewModels: validate decoded payload (non-empty, classify URL vs plain text) and call storage service.
- Storage service: own table creation, insert/delete, and read-all operations.

### Save Coalescing

MVP: no coalescing required beyond simple async writes. If duplicate rapid scans become an issue, introduce a short de-duplication window in the storage service.

### Save Lifetime

Writes are short-lived async operations; keep them cancellable if the ViewModel is disposed before completion.

## Threading Model

UI interactions and navigation occur on the main thread. Long-running operations (DB I/O, decoding, file access) run on background threads via async/await. Use MainThread.InvokeOnMainThreadAsync for UI callbacks when necessary.

## Messaging / Events

ScannerService raises decoded events which ViewModels subscribe to. Prefer direct event handlers or IObservable patterns for simplicity; avoid an event-bus abstraction for MVP.

## Settings

Minimal: Application.UserAppTheme is set to Dark at startup. Preferences for Phase 2 (sound toggle) will use `Preferences`.

## Caching

No in-memory caching required for MVP beyond an in-memory list of history items loaded at startup and observed by HistoryViewModel.

## Validation

See the spec for validation rules (URL classification with Uri.TryCreate and scheme check, non-empty payloads, etc.).

# Data Flow

## Startup

- MauiProgram builds DI container and registers services/ViewModels.
- Call `SQLitePCL.Batteries_V2.Init();` early in startup.
- AppShell is loaded with TabBar (Scanner, History). HistoryViewModel loads persisted items on activation.

## User Interaction

- Scanner: ZXing raises decode event; ScannerService applies debounce and emits a validated payload to the ScannerViewModel, which saves and navigates to ResultDetail.
- History: taps navigate to ResultDetail with query parameter id.

## Save Flow

See Save Pipeline above.

## Synchronization

MVP: local-only; no background sync or cloud. Concurrency is limited to UI and DB async calls.

# Design Decisions

## Why This Architecture

- MVVM + Shell is the natural fit for MAUI small apps; it keeps UI and logic separated and integrates with Shell routing and deep-link friendly query parameters.
- Concrete services (no interfaces) reduce ceremony for an app this small and speed up implementation.
- SQLite via `sqlite-net-pcl` is lightweight and appropriate for a single-table history.

## Alternatives Considered

- EF Core (rejected): unnecessary weight for single-entity local persistence.
- Full dialog service abstraction (IAlertService): deferred until testability or multiple dialog styles are required.

## Trade-offs

Simplicity and speed of implementation are prioritized over long-term extensibility. If Phase 2 requires more complex features, introduce interfaces and abstractions then.

# Future Improvements

## Possible Refactoring

- Introduce IScannerService and IStorageService interfaces for testability and swap-in implementations.
- Add a lightweight repository layer if models grow beyond a single table.

## Known Limitations

- Android-only targeting (no iOS/Windows support planned in this repo for MVP).
- Minimal validation on relative timestamp formatting and toasts; Phase 2 can refine UX.
