# MVP Implementation Plan — QR Scanner App (Phase: MVP)

## Problem statement
Implement the MVP portion of the QR Scanner app spec (see .github/docs/plans/qr-scanner-app-spec-mvp-phase2.md). Deliver an Android-only .NET MAUI app with live QR scanning, persistent history, result actions (Open/Copy/Share), Shell navigation, and dark theme UI.

## Approach
- Build incrementally to satisfy each MVP acceptance criterion.
- Start with app shell, DI registration, and dark theme enforcement.
- Implement Scanner page with ZXing live camera view and 2s debounce.
- Implement result handling and ResultDetail page (URL detection, Open/Copy/Share).
- Implement SQLite persistence for ScanResult model and History page (CollectionView + SwipeView delete).
- Add CAMERA permission flow and graceful handling when denied.
- Verify acceptance criteria with manual device runs on Android emulator/device and validate persistence across restarts.

## Key files/components to change or add
- UIApp\AppShell.xaml / AppShell.xaml.cs — adopt TabBar with Scanner and History tabs and register ResultDetail route
- UIApp\Views\ScannerPage.xaml(.cs) — live camera preview, overlay, debounce logic
- UIApp\Views\ResultDetailPage.xaml(.cs) — show decoded content + actions
- UIApp\Views\HistoryPage.xaml(.cs) — CollectionView, swipe-to-delete, tap-to-open
- UIApp\ViewModels\ScannerViewModel.cs, ResultDetailViewModel.cs, HistoryViewModel.cs — MVVM wiring, commands
- UIApp\Models\ScanResult.cs — SQLite model (PrimaryKey, RawValue, Type, ScannedAt, IsFavorite)
- UIApp\Services\SqliteStorageService.cs — persist ScanResult objects (no IStorageService interface; implement concrete service directly)
- UIApp\Services\ScannerService.cs — ZXing integration and debounce (no IScannerService interface; implement concrete service directly)
- UIApp\Resources\Styles\Colors.xaml & Styles.xaml — ensure dark-only palette and set Application.UserAppTheme
- Platforms\Android\AndroidManifest.xml — CAMERA permission entry

## Implementation Todos
(Tracked as actionable items in session DB; IDs use kebab-case.)

1. Setup app shell and dark-theme enforcement
   - Add TabBar with routes: scan (ScannerPage), history (HistoryPage)
   - Register route for resultdetail
   - Set Application.Current.UserAppTheme = AppTheme.Dark at startup

2. Implement ScanResult model and SQLite storage service
   - Add ScanResult.cs per spec
   - Implement SqliteStorageService with CRUD (Create, ReadAll, Delete)

3. Implement Scanner page and scanner service
   - Integrate ZXing.Net.MAUI camera view
   - Limit to QR format, implement SCAN_DEBOUNCE_MS = 2000
   - On valid decode, classify URL vs PlainText, ignore empty/whitespace
   - Save scan to SQLite and navigate to ResultDetail

4. Implement ResultDetail page and actions
   - Show type icon, decoded text, primary Open button (URL-only)
   - Copy and Share actions
   - Wrap Launcher.OpenAsync and Clipboard operations in try/catch and show brief toast on failures

5. Implement History page
   - CollectionView with reverse-chronological items, relative timestamp
   - Tap to navigate to ResultDetail via GoToAsync("resultdetail?id={id}")
   - SwipeView with Delete action to remove from storage
   - Show empty state text when no items

6. Implement Camera permission flow
   - Request CAMERA permission when entering Scanner tab
   - On denial, show explanation and button to open system app settings

7. Wiring, DI, and routing
   - Register services and viewmodels in MauiProgram.cs
   - Ensure Routing.RegisterRoute("resultdetail", typeof(ResultDetailPage)) in AppShell

8. Validation & manual verification
   - Build and run on Android emulator/device
   - Validate all MVP acceptance criteria (list from spec)

## Notes & Decisions
- Keep UI minimal and single-purpose per screen as spec instructs.
- Include IsFavorite column now with default false to avoid Phase 2 migration.
- No torch/gallery/generator/settings in MVP; reserve them for Phase 2.
- No interfaces; implement concrete services directly.

## MVP Acceptance Checklist
see section "MVP Acceptance Criteria" in `.github\docs\plans\qr-scanner-app-spec-mvp-phase2.md`

## Required NuGet packages
- ZXing.Net.MAUI
- sqlite-net-pcl
- SQLitePCLRaw.bundle_green — native SQLite binary bundle required by `sqlite-net-pcl`; remember to call `SQLitePCL.Batteries_V2.Init()` in `MauiProgram.cs` before any DB access.
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Logging (already referenced)
- Microsoft.Maui.Essentials (for Launcher, Clipboard, Share — verify runtime APIs)

Note: Verify exact package IDs and choose stable versions compatible with .NET 10 during implementation. Pin versions in the csproj to avoid unexpected upgrades.

---

Saved plan to .github/docs/plans/qr-scanner-mvp-plan.md. Next step: tell me if you want to start implementing or update the plan.