# QR Code Scanner — Technical Specification

## 1. Overview

A .NET 10 MAUI application for **Android only**, providing QR code scanning via live camera, with scan history and result actions. UI is **dark mode only** — no light theme, no theme switching logic required.

This spec is split into **MVP** (build first, ship first) and **Phase 2** (defer until MVP is working and validated). Do not start Phase 2 work until every MVP acceptance criterion passes.

## 2. Platform & Tech Stack

| Concern | Choice |
|---|---|
| Framework | .NET 10, .NET MAUI |
| Target platform | Android only (remove/ignore iOS, Windows, macOS heads) |
| Scanning library | ZXing.Net.MAUI (`ZXing.Net.Maui.Controls`) |
| Local storage | SQLite via `sqlite-net-pcl` (requires `SQLitePCLRaw.bundle_green` native bundle) |
| Architecture pattern | MVVM (`CommunityToolkit.Mvvm` recommended for `[ObservableProperty]` / `[RelayCommand]` source generators) |
| Navigation | **.NET MAUI Shell** (`AppShell.xaml`), using a **`TabBar`** for the top-level pages (chosen over a `FlyoutItem`/hamburger-menu layout). All pages are registered as Shell routes; navigation goes through `Shell.Current.GoToAsync(...)`, not manual `Navigation.PushAsync`. |
| Min Android API | Target the current minimum MAUI-supported Android API level; confirm against ZXing.Net.MAUI's supported range at implementation time |

### 2.1 Shell navigation structure

Shell is the navigation system (routing, back stack, `GoToAsync`). `TabBar` is the top-level visual container used within Shell to present the app's main pages as bottom tabs, chosen instead of the `FlyoutItem` (hamburger menu) alternative.

```
AppShell
├── TabBar
│   ├── ShellContent → ScannerPage   (route: "scan")
│   └── ShellContent → HistoryPage   (route: "history")
└── Registered detail route (not in the TabBar, pushed via GoToAsync)
    └── ResultDetailPage (route: "resultdetail")
```

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       x:Class="QrScannerApp.AppShell">

    <TabBar>
        <ShellContent Title="Scan"
                      Route="scan"
                      ContentTemplate="{DataTemplate views:ScannerPage}" />
        <ShellContent Title="History"
                      Route="history"
                      ContentTemplate="{DataTemplate views:HistoryPage}" />
    </TabBar>

</Shell>
```

- `ResultDetailPage` is not part of the `TabBar` — it's registered separately with `Routing.RegisterRoute("resultdetail", typeof(ResultDetailPage));` in `AppShell.xaml.cs`, and reached via `GoToAsync("resultdetail?id=...")`, which pushes onto the nav stack on top of whichever tab is active.
- Pass scan results between pages via query parameters (`GoToAsync($"resultdetail?id={scanId}")` with `[QueryProperty]` on the destination ViewModel) rather than passing objects through constructors — this is the Shell-idiomatic pattern and keeps pages independently navigable/deep-linkable.
- Phase 2 screens (Generator, Settings) get added as additional `ShellContent` entries inside the same `TabBar` once built — the structure above should anticipate this so MVP doesn't need restructuring later.

### 2.2 Data persistance choice

Local storage: SQLite via `sqlite-net-pcl`.

Required packages:
- `sqlite-net-pcl`
- `SQLitePCLRaw.bundle_green` — native SQLite binary bundle required by `sqlite-net-pcl` on target platforms.

Startup step (MauiProgram.cs, before any DB access):

```csharp
// Initialize the native SQLite provider
SQLitePCL.Batteries_V2.Init();
```

For full rationale, pitfalls, and implementation details see `.github/docs/architecture.md`.

## 3. UI Design (simple, MVP-scope)

Two screens plus one modal/detail view is enough for MVP. Keep every screen to a single primary action.

### 3.1 Scanner Page (default/landing tab)
- Full-screen camera preview.
- Centered square/rectangular viewfinder overlay with a border, rest of the frame dimmed (semi-transparent black overlay outside the viewfinder box).
- Bottom of screen: single status text area (e.g. "Point camera at a QR code") — replaced momentarily by "Scanned!" feedback on detection, then auto-navigates to result.
- No buttons required for MVP (torch/zoom are Phase 2) — the screen's only job is scan-and-go.

### 3.2 Result Detail Page
- Large icon or label indicating detected type (URL vs plain text only, per MVP scope).
- Decoded content displayed in a read-only, wrapped text block (not an editable field).
- Primary action button: "Open in Browser" (only shown/enabled if content is a valid URL) or otherwise hidden.
- Secondary actions as a horizontal row of icon buttons: **Copy**, **Share**.
- Back navigation returns to Scanner tab (or History tab if opened from history — Shell's back stack handles this automatically).

### 3.3 History Page
- Simple vertical list (`CollectionView`), reverse-chronological.
- Each row: type icon, truncated content (single line, ellipsized), relative timestamp (e.g. "2m ago").
- Tap row → navigate to Result Detail Page for that item.
- Swipe-to-delete on each row (`SwipeView` with a delete action).
- Empty state: centered text ("No scans yet") when history is empty — don't leave a blank screen.
- No favorites, no clear-all, no filtering in MVP.

### 3.4 Visual style
- Single dark color palette defined once in `Resources/Styles/Colors.xaml` and `Styles.xaml`. No `AppThemeBinding` needed since there's only one theme.
- Set `Application.Current.UserAppTheme = AppTheme.Dark` explicitly at startup so system chrome (status bar, dialogs) matches even on a light-mode device.
- Minimum tap target size 44x44dp on all interactive elements (standard mobile accessibility baseline).
- Use system-standard Android back gesture/button behavior — don't override or intercept it in MVP.

## 4. MVP Scope

### 4.1 Core Scanning
- Live camera scanning using ZXing's `CameraBarcodeReaderView`, constrained to `BarcodeFormat.QrCode` only.
- Viewfinder overlay (visual only — see 3.1).
- **Scan debounce**: after a successful decode, suppress further detections for a fixed cooldown window of **2 seconds** before the scanner can trigger again. Hardcode this as a constant: `private const double SCAN_DEBOUNCE_MS = 2000;` with a comment explaining the tradeoff: longer windows prevent duplicate detections from a held code but feel less responsive; 2 seconds balances responsiveness against accidental re-scans. Required — without it, a held QR code fires duplicate detections continuously.
- No static image/gallery scanning in MVP (Phase 2).
- No torch, no pinch-to-zoom in MVP (Phase 2).

### 4.2 Result Handling
- Classify decoded payload as **URL** or **plain text** only. No WiFi/vCard/calendar parsing in MVP.
- Actions: Copy to clipboard, Share via Android share sheet, Open in Browser (URL only).
- Every successful scan is saved to history automatically — no user action required to persist it.

### 4.3 History & Persistence
- SQLite-backed list of past scans (see data model in 4.5).
- List view, tap to reopen detail, swipe-to-delete.
- No favorites, no bulk clear, no search/filter.

### 4.4 Permissions
- Request `CAMERA` permission on first entry to the Scanner tab, not at app launch.
- If denied: show an explanation screen with a button that deep-links to Android system settings (`AppInfo.ShowSettings()`).
- Manifest: `Manifest.permission.CAMERA`.

### 4.5 Data Model

```csharp
public class ScanResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string RawValue { get; set; }
    public ScanResultType Type { get; set; } // enum: Url, PlainText  (MVP: only these two)
    public DateTime ScannedAt { get; set; }
    public bool IsFavorite { get; set; } = false;  // Added now to avoid schema migration in Phase 2
}
```

`IsFavorite` is included now with a default value of `false`. This avoids a migration when Phase 2 rolls out. In MVP, this column is not used by the UI; Phase 2 will enable the favorites filter.

### 4.6 Input Validation & Error Handling

Even though this app has almost no user text input, validation and error feedback matter at these points:

| Input point | Validation rule | Failure feedback |
|---|---|---|
| Decoded QR payload → URL classification | Use `Uri.TryCreate(raw, UriKind.Absolute, out var uri)` **and** check `uri.Scheme is "http" or "https"` before treating as a URL. Don't classify `ftp://`, `mailto:`, or malformed strings as "open in browser" candidates — route them to plain text instead for MVP. | N/A (silent classification) |
| Decoded QR payload → length/empty check | If the decoded string is null, empty, or whitespace-only, treat as a failed scan — do not save an empty history entry or navigate to the result screen. | Toast: "Couldn't read QR code, try again." |
| "Open in Browser" action | Wrap `Launcher.OpenAsync(uri)` in a try/catch — a URL can pass the `Uri.TryCreate` check but still fail to open (no matching app, malformed edge cases). On failure, show a toast. | Toast: "Couldn't open link." |
| Clipboard copy | No validation needed beyond ensuring `RawValue` isn't null (guaranteed by the empty-check above). | N/A (success is silent) |
| SQLite writes | `RawValue` column should be non-nullable at the schema level as a backstop, even though app-level validation should prevent nulls from ever reaching it. | N/A (app-level validation prevents this) |

**Toast implementation**: Use `DisplayAlert()` with a brief timeout, or an equivalent lightweight notification (no blocking dialog — users should continue scanning). A simple `await MainThread.InvokeOnMainThreadAsync(() => Application.Current?.MainPage?.DisplayAlert("", "Couldn't read QR code, try again", "OK"))` is sufficient for MVP.

No free-text user input fields exist in MVP (no search box, no manual entry), so form-style validation (required fields, character limits, etc.) isn't needed yet — it becomes relevant in Phase 2 once the QR generator is added, since that takes arbitrary user-typed text.

### 4.7 History List — Relative Timestamps

Each history entry displays a **relative timestamp** calculated as the difference between the current time and `ScanResult.ScannedAt`. Simple math: `DateTime.Now - scanResult.ScannedAt`. Examples:
- If scanned 30 seconds ago: "30s ago"
- If scanned 5 minutes ago: "5m ago"
- If scanned 2 hours ago: "2h ago"
- If scanned yesterday: "1d ago"

No special library needed — build this logic in the ViewModel using `DateTime.Now - ScannedAt` and format the span into a human-readable string. For MVP, a simple implementation suffices (don't over-engineer relative time formatting); if timestamps feel stale or inaccurate, add periodic ViewModel refresh in Phase 2.

### 4.8 Shell Navigation & Back-Stack Behavior

Result Detail page (not part of the bottom tab bar) is navigated via `Shell.Current.GoToAsync("resultdetail?id={scanId}")`. This pushes the page onto the navigation stack **on top of the currently active tab**.

**Navigation flows:**

1. **Scanner tab → scan QR → Result Detail:**
   - User starts on Scanner tab.
   - Successful scan triggers `GoToAsync("resultdetail?id={scanId}")`.
   - Result Detail appears (stacked on top of Scanner tab).
   - User taps back → returns to Scanner tab (Shell pops the detail page).

2. **History tab → tap a result → Result Detail:**
   - User is on History tab, viewing the list.
   - Tap a row → `GoToAsync("resultdetail?id={scanId}")`.
   - Result Detail appears (stacked on top of History tab).
   - User taps back → returns to History tab (Shell pops the detail page).

**Key point:** The back stack respects the tab context. If you open a result from History, back goes to History. If you open a result from a live scan, back goes to Scanner. This is Shell's default behavior and requires no special handling — the `GoToAsync` navigation model preserves the tab stack automatically.

**Routing configuration in AppShell.xaml.cs:**
```csharp
Routing.RegisterRoute("resultdetail", typeof(ResultDetailPage));
```

No additional back-stack manipulation or custom navigation handlers needed in MVP.

## 5. Phase 2 Scope (build only after MVP acceptance criteria pass)

### 5.1 Additional scanning capability
- Static image scanning: pick an image from the gallery via `MediaPicker`/`FilePicker`, decode via ZXing's image-reader API.
- Torch/flashlight toggle.
- Pinch-to-zoom on the camera preview.

### 5.2 Expanded result classification
- WiFi config (`WIFI:S:...;`) → "Connect to network" action.
- vCard (`BEGIN:VCARD`) → "Add to contacts" action.
- Calendar event (`BEGIN:VEVENT`) → "Add to calendar" action.
- Each of these is a separate parser — treat as an independent unit of work per type, don't build one "smart" parser that tries to handle all of them at once.

### 5.3 History enhancements
- Favorites/pinning, with a filter toggle on the History page.
- "Clear all" with confirmation dialog.
- Possibly: search/filter by content.

### 5.4 QR Code Generation
- New Shell tab/route: Generator page.
- Free-text input field for content to encode.
- **Input validation needed here** (unlike MVP): enforce a max length appropriate to QR capacity (practically, keep well under the ~4,296 alphanumeric character ceiling — a few hundred characters is a sane UI-level cap, since longer inputs produce QR codes too dense to reliably scan on a phone camera). Show a character counter and disable the generate action when the field is empty or over the cap.
- Render via ZXing's writer/`BarcodeGeneratorView`.
- Save generated image to device storage and/or share directly.

### 5.5 Settings
- New Shell tab/route or a settings entry from History/Scanner.
- Sound toggle (on/off), persisted via `Preferences`.
- Haptic feedback on successful scan (can actually ship in MVP if trivial — it's cheap — but grouped here since it pairs naturally with the sound toggle).

## 6. Out of Scope (both MVP and Phase 2)

- iOS, Windows, macOS support
- Light theme / theme switching
- Cloud sync or backend/account system
- Barcode formats other than QR
- Batch/multi-code detection in a single frame

## 7. MVP Acceptance Criteria

- [x] App builds and runs on Android only
- [x] Shell navigation in place with Scanner and History as tabs, ResultDetail as a pushed route
- [x] Live camera scan detects a QR code, decodes it, applies debounce, and navigates to Result Detail
- [x] Result screen correctly distinguishes URL vs plain text and shows the right primary action
- [x] Copy and Share actions work from the Result Detail screen
- [x] Every successful scan is persisted to SQLite and appears in History, surviving app restart
- [x] History supports tap-to-reopen and swipe-to-delete
- [x] Empty history state is handled (not a blank screen)
- [x] Camera permission denial shows an explanation and a working link to system settings
- [ ] Malformed/empty decode results do not create a history entry or crash the app *(deferred to Phase 2: requires malformed QR test vectors)*
- [x] Entire UI renders in dark theme regardless of system theme setting

## 8. Phase 2 Acceptance Criteria

- [ ] Malformed/empty decode results are validated and rejected (test vectors with corrupted/empty QR payloads)
- [ ] Gallery image scanning successfully decodes an embedded QR code
- [ ] Torch and pinch-to-zoom work during live scanning
- [ ] WiFi, vCard, and calendar payloads each trigger their correct type-specific action
- [ ] Favorites and clear-all work in History
- [ ] Generator produces a valid, scannable QR code from user text input, with length validation enforced
- [ ] Settings sound toggle persists across app restarts
