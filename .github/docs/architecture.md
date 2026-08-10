# Architecture

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