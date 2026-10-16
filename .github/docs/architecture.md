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

A small .NET MAUI QR scanner app. Android is the primary target; Windows is retained as a development/build target. The app scans QR codes from the camera or a selected image, stores scan history locally, and provides History, Generator, and Settings screens. It uses MVVM, Shell tabs, and a dark-only UI.

## Tech Stack

| Component | Package | Version |
|-----------|---------|---------|
| Framework | .NET MAUI | 10.0 |
| Scanning | ZXing.Net.Maui.Controls | 0.10.3 |
| Persistence | sqlite-net-pcl | 1.9.172 |
| SQLite native engine | SQLitePCLRaw.lib.e_sqlite3 | 3.53.3 |
| SQLite Android native engine | SQLitePCLRaw.lib.e_sqlite3.android | 2.1.13 |
| SQLite bridge | SourceGear.sqlite3 | 3.53.4 |
| MVVM | CommunityToolkit.Mvvm | 8.4.2 |
| Logging | Microsoft.Extensions.Logging.Debug | 10.0.0 |
| Target Frameworks | net10.0-android, net10.0-windows10.0.19041.0 | |
| Minimum Android | Android | 21.0 |

## High-Level Architecture

- UI: .NET MAUI Shell with a TabBar for Scanner, History, Generator, and Settings. ResultDetail is a registered route pushed over the active tab.
- Pattern: MVVM using CommunityToolkit.Mvvm source generators for ViewModels and Commands.
- Scanning: ZXing.Net.MAUI camera reader constrained to QR format, plus image decoding from MediaPicker; live scans use a 2s debounce. Torch and pinch zoom are controlled from the Scanner page.
- Persistence: SQLite via `sqlite-net-pcl` and `SQLitePCLRaw.bundle_green` (see Persistence section).
- Services: Small concrete services (`ScannerService`, `SqliteStorageService`) registered in DI. `ISoundService` is the platform-specific seam for scan feedback.

## Project Structure

- UIApp\Views\ — Pages: ScannerPage, HistoryPage, ResultDetailPage, GeneratorPage, SettingsPage
- UIApp\ViewModels\ — matching page ViewModels and BaseViewModel
- UIApp\Models\ — ScanResult.cs
- UIApp\Services\ — SqliteStorageService.cs, ScannerService.cs, PayloadParsers.cs, sound-service contract/fallback
- UIApp\Resources\ — Styles, Colors
- UIApp\Platforms\Android\ — Manifest permissions and AndroidSoundService

# Core Systems

## MVVM

### Views

Pages are thin shells that bind to ViewModels via x:DataType and expose UI-only logic (animations, visual states). Keep code-behind minimal: only UI lifetime hooks and small view-only helpers.

### ViewModels

ViewModels hold state, expose observable properties, and provide relay commands for actions. ResultDetailViewModel implements `IQueryAttributable` to receive the scan id from Shell navigation.

### Models

The primary model is ScanResult (Id, RawValue, Type, ScannedAt, IsFavorite). Keep models POCO and attribute-decorated for SQLite mapping.

### Services

- SqliteStorageService: concrete async CRUD for ScanResult. Responsible for creating the table and performing scan-history operations; no migration layer is currently used.
- ScannerService: processes live and image-decoded values, rejects empty values, applies the live-scan debounce, classifies payloads, and persists accepted results.
- PayloadParsers: pure parsers for WiFi, vCard, and calendar payloads. They return data objects; parsed fields are not currently stored in SQLite.
- Platform-specific sound: `ISoundService` is implemented by AndroidSoundService on Android and NoOpSoundService on other targets. Camera permission, vibration, MediaPicker, and Launcher/Clipboard/Share APIs are called through MAUI APIs.

## Dependency Injection

Register pages and ViewModels as transient, and `SqliteStorageService` and `ScannerService` as singletons in MauiProgram.cs. Register the platform sound implementation there as well. Ensure `SQLitePCL.Batteries_V2.Init();` runs before any DB access; the database itself is initialized lazily by the storage service.

## Navigation

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

### Routes

Use Shell routes and `Shell.Current.GoToAsync(...)`. Scanner, History, Generator, and Settings are Shell tabs. Register ResultDetail in AppShell using `AppShell.ResultDetailRoute` and pass scan ids via query parameters. Rely on Shell's tab-aware back stack behavior.

## Persistence

Data persistence: SQLite

Library choice: `sqlite-net-pcl` (not EF Core)

The `ScanResults` table stores each scan's id, raw value, classified type, UTC scan time, and favorite flag. The database file is `scanresults.db3` in `FileSystem.AppDataDirectory`. The favorite flag was included in the model before favorites were added, so no schema migration was needed for that feature. History reads are ordered newest first; favorites can be queried separately.

Why

The app's persistence needs are a single flat table for scan history, favorites, and basic history management; no relational complexity or LINQ-heavy querying is required. `sqlite-net-pcl` provides an attribute-based, async-first ORM (`SQLiteAsyncConnection`) that maps directly onto MAUI's async patterns with minimal boilerplate: define a POCO with `[PrimaryKey]` and `[AutoIncrement]`, call `CreateTableAsync<T>()`, then use async table operations for CRUD. It's also widely used in MAUI/Xamarin.Forms apps, which reduces ambiguity during implementation and handoff.

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

`ScannerService` calls `SqliteStorageService.AddAsync` after validating and classifying a live or image scan. The storage service owns table initialization and direct asynchronous SQLite inserts.

### Save Responsibilities

- ScannerService: reject empty payloads, classify URL/plain text/WiFi/vCard/calendar, then persist the scan.
- SqliteStorageService: own table initialization and insert, read, update, delete, favorites, and clear-all operations.

### Save Coalescing

Writes are not coalesced. Each accepted scan is inserted directly; live camera duplicates are limited by ScannerService's 2s debounce.

### Save Lifetime

Database operations use `SQLiteAsyncConnection` asynchronous methods. No write queue or cancellation layer is currently used.

## Threading Model

UI interactions and navigation occur on the main thread. SQLite and image-decoding APIs are called asynchronously; scanner navigation is marshaled to the main thread. Pinch zoom updates the camera view on the main thread.

## Messaging / Events

ScannerPage handles ZXing's `BarcodesDetected` event and forwards decoded text to ScannerViewModel, which delegates validation and persistence to ScannerService. No application-wide event bus is used.

## Settings

`Application.UserAppTheme` is set to Dark at startup. SettingsViewModel persists sound and haptic toggles with MAUI `Preferences`; scan feedback uses MAUI vibration and `ISoundService`. History's favorites-only filter is also persisted with `Preferences`.

## Caching

HistoryViewModel keeps the currently loaded rows in an `ObservableCollection`; it reloads them when the History page appears or the favorites filter changes. There is no separate data cache.

## Validation

ScannerService rejects empty live-scan values and classifies payloads using the pure parsers before falling back to URL or plain text. Structured parser results are not persisted; ResultDetail currently operates on the raw value and stored type.

GeneratorViewModel accepts non-empty text up to 512 characters, creates a 1024×1024 QR image, saves it in the app data directory, and shares a temporary image from the cache directory.

# Data Flow

## Startup

- MauiProgram builds DI container and registers services/ViewModels.
- `MauiProgram` calls `SQLitePCL.Batteries_V2.Init();` before building the app.
- AppShell provides Scanner, History, Generator, and Settings tabs. HistoryViewModel loads persisted items when its page appears.

## User Interaction

- Scanner: ZXing raises a camera decode event, or the user selects an image through MediaPicker. ScannerViewModel delegates processing to ScannerService; accepted results are saved and opened in ResultDetail. Live camera scans use debounce; image scans use the same classification and persistence path without the live camera debounce.
- History: taps navigate to ResultDetail with query parameter id.
- Generator: user text is rendered as a QR image, then saved locally or shared.
- Settings: sound and haptic choices are read from and written to Preferences.

## Save Flow

See Save Pipeline above.

## Synchronization

Local-only; no background sync or cloud. SQLite operations use the asynchronous connection API; the application does not currently implement a separate write-coalescing or synchronization layer.

# Design Decisions

## Why This Architecture

- MVVM + Shell is the natural fit for MAUI small apps; it keeps UI and logic separated and integrates with Shell routing and deep-link friendly query parameters.
- Concrete services reduce ceremony for an app this small. `ISoundService` is the narrow exception, allowing Android sound playback and a no-op implementation on non-Android targets.
- SQLite via `sqlite-net-pcl` is lightweight and appropriate for a single-table history.

## Alternatives Considered

- EF Core (rejected): unnecessary weight for single-entity local persistence.
- Full dialog service abstraction (IAlertService): deferred until testability or multiple dialog styles are required.

## Trade-offs

Simplicity and speed of implementation are prioritized over long-term extensibility. Add interfaces or abstractions only for demonstrated needs, such as platform-specific behavior.

# Future Improvements

## Possible Refactoring

- Introduce IScannerService and IStorageService interfaces for testability and swap-in implementations.
- Add a lightweight repository layer if models grow beyond a single table.

## Known Limitations

- Android is the intended release target; Windows remains a build/development target, not a supported release platform.
- Torch availability is checked with MAUI's device-level flashlight API, while torch control uses ZXing's active camera. If camera selection is added, support should be verified for the selected camera.
- Structured QR parsers currently classify and extract WiFi, vCard, and calendar data, but those parsed fields are not persisted or exposed through type-specific actions.
