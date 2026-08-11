# Phase 2 Implementation Plan — QR Scanner App

## Status
Draft

## Sources
- .github\docs\plans\qr-scanner-mvp--implementation-plan.md (MVP plan)
- .github/docs/plans/qr-scanner-app-spec-mvp-phase2.md (Spec)
- .github/docs/project-context.md
- .github/docs/architecture.md

## Summary
Phase 2 enhances the shipped MVP with gallery/image scanning, torch & pinch-to-zoom, richer payload parsing (WiFi/vCard/Calendar), History improvements (favorites, clear-all, search), a QR Generator, and Settings (sound/haptic). Follow existing architecture (MVVM, Shell TabBar, sqlite-net-pcl) and project constraints (Android primary target, dark theme, keep UI minimal). Phase 2 begins after MVP acceptance and stabilization.

## Goals
- Add static-image scanning and camera controls for improved UX.
- Expand result classification and type-specific actions for common QR payloads.
- Improve History UX with favorites and management controls.
- Add QR generator with input validation and sharing/saving.
- Add Settings for sound/haptics and persist preferences.
- Harden app: tests, CI, pinned dependencies, and migration-safe database changes.

## Scope (what's in)
- Gallery/static-image scanning using MediaPicker/FilePicker + ZXing image decode.
- Torch (flash) toggle and pinch-to-zoom on live camera preview.
- Parsers and actions for WiFi, vCard, and VEVENT calendar items.
- History favorites (IsFavorite already present in DB), filter toggle, and "Clear All" with confirmation.
- QR Generator page: text input, character counter, validation, generate/share/save image.
- Settings page: sound toggle and haptic toggle persisted via Preferences.
- Unit tests for parsers and storage; basic CI workflow (build + unit tests).

## Non-Goals (out of scope)
- iOS/Windows/macOS support.
- Cloud sync or user accounts.
- Support for non-QR barcode formats.

## Assumptions & Constraints
- Android is the primary supported target for release; Windows is retained as a testing platform only (not a supported release target) unless future work explicitly expands platform support.
- Use existing DB schema — IsFavorite already added to avoid migration.
- Continue using concrete services (SqliteStorageService, ScannerService) unless Phase 2 needs interfaces for testability or extensibility.
- Follow architecture.md: constructor injection, x:DataType in XAML, CollectionView for lists, no nested ScrollView in StackLayout.

## Required Packages & Platform APIs
- ZXing.Net.MAUI (already used) — confirm or pin a newer compatible version if required for writer/decoder helpers.
- sqlite-net-pcl and SQLitePCLRaw.bundle_green (existing) — ensure pinned versions.
- CommunityToolkit.Mvvm (existing)
- Xamarin.Essentials / Microsoft.Maui.Essentials APIs for MediaPicker, Launcher, Clipboard, Share, and AppInfo
- (Optional) ZXing writer dependency or helper for generating QR images; ZXing usually supports writer API.
- Android permissions: CAMERA (already), READ_EXTERNAL_STORAGE / READ_MEDIA_IMAGES as needed by MediaPicker depending on Android API level. Use runtime permission checks compliant with Android 13+ scoped storage rules.

## High-level Epics & Tasks

### Epic: Static-image (gallery) scanning
- Task: Add Gallery button & UI (ScannerPage or Generator flow) — UX: open MediaPicker
- Task: Implement image-decoding pipeline using ZXing image reader APIs
- Task: Validate decoded payloads and reuse existing classification + save flow

### Epic: Torch & Pinch-to-zoom
- Task: Add UI toggle for torch; implement platform-safe control via ZXing/handler
- Task: Implement pinch gesture handling to adjust camera zoom; clamp to supported range

### Epic: Expanded Payload Parsers (WiFi, vCard, Calendar)
- Task: Implement independent parsers: WIFI:S:...; BEGIN:VCARD; BEGIN:VEVENT
- Task: Add type-specific actions: Connect to network (show details + optionally copy), Add contact (invoke Contacts API / intent), Add calendar event (Calendar intent)
- Task: Safety & permission checks for contact/calendar writes — prefer opening external apps via intents instead of writing directly to device (minimize required runtime permissions)

### Epic: History Enhancements
- Task: Add Favorites toggle and filter in History (UI + DB queries)
- Task: Implement Clear All with confirmation dialog and undo snack/toast if feasible
- Task: Add search/filter UI (optional stretch)

### Epic: QR Generator
- Task: Generator page: text input + char counter; enforce max length (recommend 512 chars UI cap)
- Task: Generate QR image (ZXing writer), preview, save-to-storage and share actions
- Task: Validation & unit tests for generator

### Epic: Settings
- Task: Add Settings page / Shell tab or route; persist toggles with Preferences
- Task: Hook sound / haptic toggles into scan success flow

### Epic: Tests, CI, and Dependency Pinning
- Task: Add unit tests for parsers, storage CRUD, and HistoryViewModel behaviors
- Task: Add GitHub Actions workflow: restore, build solution, run unit tests, and dependency-check step
- Task: Pin package versions in csproj and add changelog note

## Implementation Todos

Actionable implementation todos derived from the epics above. Each item is a developer-facing checklist (suitable to convert to GitHub issues or track as personal todos). Follow the order roughly to satisfy Phase 2 acceptance criteria progressively.

1. Implement Gallery/static-image scanning
   - Add Gallery button to ScannerPage with UI to open MediaPicker (see UX in Scope)
   - Integrate MediaPicker.PickPhotoAsync() flow to select image from device storage
   - Decode image using ZXing's image reader API (BarcodeReader or equivalent)
   - Validate decoded payload: reuse existing QR classifier logic (URL vs PlainText)
   - On success, save to SQLite history and navigate to ResultDetail; on failure, show toast/alert
   - Test on Windows and Android 14 (API 34) to verify storage permission handling

2. Implement torch (flash) toggle on Scanner page
   - Add visible UI toggle button (flashlight icon) on ScannerPage camera overlay
   - Integrate platform-safe torch control via ZXing.Net.MAUI handlers or Android platform APIs
   - Clamp torch state to supported device range (some devices may not support flash)
   - Persist user preference during app session (optional: save to Preferences for Phase 2.x)
   - Handle gracefully on devices without flash (disable button, show tooltip)

3. Implement pinch-to-zoom on camera preview
   - Add pinch gesture recognizer to CameraView on ScannerPage
   - Map gesture scale to camera zoom level using ZXing's zoom range (e.g., 1x to 4x)
   - Clamp zoom value to device-supported range to avoid crashes
   - Provide visual feedback (zoom level indicator) during pinch
   - Test on multiple Android API versions to verify zoom behavior

4. Implement payload parsers for WiFi, vCard, and Calendar
   - Create independent parser for WiFi QR format: `WIFI:T:WPA;S:SSID;P:PASSWORD;H:hidden;;`
     - Extract SSID, password, security type; validate format
     - Action: display parsed network details and optionally copy password to clipboard
   - Create independent parser for vCard format: `BEGIN:VCARD...END:VCARD`
     - Extract name, email, phone, organization
     - Action: invoke system intent to add contact (e.g., `Intent.ACTION_INSERT` on Android with contact data)
   - Create independent parser for Calendar/iCal format: `BEGIN:VEVENT...END:VEVENT`
     - Extract title, start time, location, description
     - Action: invoke system intent to add calendar event
   - Add unit tests for each parser with valid and edge-case inputs
   - Ensure parser failures are graceful (fall back to PlainText type)

5. Implement History Favorites and filtering
   - Add IsFavorite toggle UI to ResultDetail page (star icon or checkbox)
   - Implement ToggleFavorite command in ResultDetailViewModel to update SQLite
   - Add Favorites filter toggle on HistoryPage (e.g., "Show All" / "Favorites Only")
   - Update HistoryViewModel to filter results based on toggle state
   - Persist filter preference in Preferences or session state
   - Test toggle between filter states and verify history updates correctly

6. Implement Clear All with confirmation in History
   - Add "Clear All" button to HistoryPage UI (or SwipeView action menu)
   - Show confirmation dialog: "Delete all scan history?" with "Delete" / "Cancel" buttons
   - On confirmation, clear all items from SQLite (DELETE FROM ScanResults)
   - Update UI to show empty state after clear
   - Optional: add toast/snackbar confirmation after successful clear
   - Unit test the HistoryViewModel.ClearAll command and DB cleanup

7. Implement QR Generator page
   - Create new Shell route and GeneratorPage (XAML + code-behind)
   - Add text input field (Editor or Entry) for content to encode
   - Add character counter label (live update) and enforce max length (recommend UI cap ~256–512 chars)
   - Disable Generate button when input is empty or exceeds max length
   - Implement Generate command: use ZXing's BarcodeGeneratorView or BarcodeWriter to render QR image
   - Add Preview label/image to display generated QR
   - Add Save and Share buttons:
     - Save: prompt for filename, save image to device storage (using FileSaver or MediaStore on Android)
     - Share: use Share.RequestAsync() to open share intent
   - Add unit tests for input validation and generator output

8. Implement Settings page with sound and haptic toggles
   - Create new Shell route and SettingsPage (XAML + code-behind)
   - Add toggle switch for "Sound on scan" (persistent via Preferences.Set/Get)
   - Add toggle switch for "Haptic feedback on scan" (persistent via Preferences.Set/Get)
   - Wire toggles into ScannerViewModel: call Vibration.Vibrate() and play sound (MediaElement or MediaManager) on successful scan
   - Test toggle state persistence across app restart
   - Verify sound and haptic behavior on device (emulator haptics may not work)

9. Add unit tests for parsers, storage, and ViewModel behaviors
   - Create unit test project (if not already present)
   - Test WiFi, vCard, and Calendar parsers with valid/invalid inputs
   - Test ScanResult CRUD operations (Create, Read, Delete) in SqliteStorageService
   - Test HistoryViewModel filtering and clear-all logic
   - Test GeneratorViewModel input validation and length enforcement
   - Test SettingsViewModel preference persistence
   - Aim for >80% code coverage on critical paths (parsers, storage, filtering)

10. Add GitHub Actions CI workflow and pin package versions
    - Create `.github/workflows/build-and-test.yml` workflow:
      - Trigger on push to main/develop branches and PRs
      - Restore NuGet packages: `dotnet restore`
      - Build solution: `dotnet build` (all target frameworks)
      - Run unit tests: `dotnet test`
      - (Optional) publish test results artifact
    - Pin NuGet package versions in UIApp.csproj (avoid floating versions):
      - ZXing.Net.MAUI (confirm compatible version with Phase 2 features)
      - sqlite-net-pcl, SQLitePCLRaw.bundle_green
      - CommunityToolkit.Mvvm
      - All other dependencies used in Phase 2
    - Document pinned versions and any compatibility notes in a CHANGELOG or comment in csproj
    - Run workflow and verify it passes before merging

11. Validation & manual verification
    - Build and run on Windows and Android 14 (API 34) physical device
    - Manually test all Phase 2 acceptance criteria (see Acceptance Criteria section)
    - Verify no regressions in MVP functionality (scanner, history, result detail)
    - Test edge cases: storage permissions, missing flash, unsupported zoom, corrupted QR, etc.
    - Collect device-specific logs and document any platform quirks

## Data & Migration Plan
- No DB schema additions required for Favorites (IsFavorite present). For future changes:
  - Use `CreateTableAsync<T>()` shape-safe approach; avoid destructive migrations.
  - If adding new columns with defaults, use ALTER TABLE ADD COLUMN with a default or backfill after deploy.
- Backup: encourage a user-level export for history only if requested (out of scope).

## Permissions & Privacy
- Media access: request appropriate runtime permissions only when invoking gallery picker; follow Android 13+ scoped storage guidance.
- Contacts/Calendar: prefer using Intents to open system contact/calendar add screens rather than requesting write permissions. If direct write is implemented, document and request explicit user consent and required permissions.
- Clearly document in UX when the app will open external apps or system settings.

## Acceptance Criteria
Refer to the authoritative Phase 2 acceptance criteria in the spec: .github/docs/plans/qr-scanner-app-spec-mvp-phase2.md — see section "8. Phase 2 Acceptance Criteria" for the canonical list. Remove duplicate criteria from this implementation plan to avoid drift.

## Traceability: Acceptance Criteria → Implementation Todos
The table below maps each Phase 2 acceptance criterion to the Implementation Todo(s) that implement or verify it. Use this for planning, PR traceability, and test coverage mapping.

| Phase 2 Acceptance Criterion | Implementation Todo(s) |
|---|---|
| Malformed/empty decode results are validated and rejected | 4 (parsers) · 9 (unit tests) · 11 (validation & manual verification) |
| Gallery image scanning successfully decodes an embedded QR code | 1 (Gallery/static-image scanning) · 11 (validation) |
| Torch and pinch-to-zoom work during live scanning | 2 (torch) · 3 (pinch-to-zoom) · 11 (validation) |
| WiFi, vCard, and calendar payloads trigger their correct type-specific actions | 4 (parsers & actions) · 9 (unit tests) |
| Favorites and clear-all work in History | 5 (favorites/filtering) · 6 (clear all) · 9 (unit tests) |
| Generator produces a valid, scannable QR code with length validation enforced | 7 (generator) · 9 (unit tests) · 11 (validation) |
| Settings sound toggle persists across app restarts | 8 (settings) · 9 (unit tests) · 11 (validation) |

Notes
- Each mapping indicates the primary todo responsible for implementation; validation and tests are often shared across multiple todos.
- When opening issues/PRs, reference the above table row(s) in the PR description to show acceptance traceability.

## MAUI Implementation Notes
Platform and code guidance specific to .NET MAUI to reduce implementation friction and avoid common pitfalls.

- Lifecycle & camera ownership
  - Stop scanning and unsubscribe events when pages disappear to avoid camera locks and memory leaks.
  - Example (ScannerPage.xaml.cs):

```csharp
protected override void OnDisappearing()
{
    base.OnDisappearing();
    scannerView.IsScanning = false; // stop camera
    scannerService.Decoded -= OnDecoded;
}
```

- Threading & decoding
  - Perform image decoding and DB I/O off the UI thread (Task.Run / async). Use MainThread.BeginInvokeOnMainThread for UI updates.

- ZXing: torch & zoom
  - ZXing.Net.MAUI does not guarantee torch/zoom APIs across all versions. Implement a small platform handler bridge for Android (Camera2) guarded with `#if ANDROID`.
  - Always check device capability before enabling UI controls.

- Gallery decode pipeline
  - Use MediaPicker to get a FileResult, open a Stream, copy to MemoryStream and pass bytes to ZXing decoder. Avoid heavy bitmap conversions on the UI thread.

- QR Generator output
  - Render PNG at >=512px (1024px recommended) with proper quiet zone and high-contrast colors to maximize scannability.

- Parsers & testability
  - Keep parsers pure (no platform calls) in a static Parsers class to make unit testing trivial.

- Permissions (Android 14)
  - Prefer MediaPicker to avoid broad storage permissions. If direct file access is needed, request `READ_MEDIA_IMAGES` at runtime and handle denial gracefully.

- CI and tests
  - Building MAUI Android/Windows images in CI requires platform workloads (Android SDK, JDK, WinUI workloads). For Phase 2, prioritize running unit tests on CI; document MAUI build runner requirements for future full-app CI.

- Generator/Decoder QA
  - Add a small set of image test vectors (valid, truncated, corrupted payloads) and include them in unit/integration tests where possible.

## Risks & Mitigations
- ZXing writer/reader API differences between versions: pin working package used in MVP and validate writer usage on device early.
- Android storage/permission behavior differences (Android 13+): test on Android 14 (API 34) and verify READ_MEDIA_IMAGES / scoped storage behavior.
- Contact/calendar write permissions are intrusive: prefer external intents to avoid permission complexity.
- Dependency advisory for SQLitePCLRaw: resolve or document acceptable mitigation prior to wide release (project-context flags this as an open item).

## QA & Test Plan
- Unit tests: parsers, storage, ViewModel behaviors (History filtering, favorites).
- Manual tests: gallery scan, torch toggle, pinch zoom, add contact/calendar via intent, generator produce-share workflow, clear-all confirmation.
- Device matrix: Windows (desktop) for UI and build verification; Android 14 (API 34) on physical device for camera/permissions/storage validation.

## Notes
File created from: MVP plan and Phase 2 spec.

