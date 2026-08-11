---
status: Active
title: Project Context
purpose: Living document describing the current state of development.
audience: Humans and AI.
update_frequency: Frequently.
contains:
  - Current work
  - Recent decisions
  - Known issues
  - Planned work
does_not_contain:
  - Permanent architectural decisions
  - Coding standards
---

# Project Status

## Current Milestone
MVP: Complete — dark-theme Android-only .NET MAUI app with live QR scanning, persistent SQLite history, result actions (Open/Copy/Share), and Shell-based navigation (Scanner/History tabs + ResultDetail route).

Next milestone: Phase 2 — gallery scanning, torch/zoom, expanded result parsers, favorites, generator, and settings.

## Current Sprint / Focus
Phase 2 planning and stabilization: manual QA and bug fixes from MVP verification, dependency pinning and remediation, add CI and basic unit tests, then begin Phase 2 feature work.

## Recently Completed
- AppShell + TabBar and routing for ResultDetail
- Dark-theme enforcement at startup (Application.UserAppTheme = AppTheme.Dark)
- ScanResult model and SqliteStorageService implemented and registered
- Removed unnecessary interfaces for MVP simplicity
- BaseViewModel and MVVM refactor using CommunityToolkit.Mvvm
- ScannerPage XAML and event plumbing created; ScannerService added to centralize debounce and classification logic
- ResultDetailPage and ViewModel (Open/Copy/Share) implemented and wired
- HistoryPage (CollectionView, reverse-chronological list, tap-to-detail, swipe-to-delete) implemented and persisted

# Current Decisions

## UI
- Single dark theme enforced; keep UI minimal for MVP (Scanner, History, ResultDetail).
- Use Shell TabBar for top-level navigation; ResultDetail is a registered route pushed via GoToAsync.

## Persistence
- Local persistence using sqlite-net-pcl with native bundle SQLitePCLRaw.bundle_green.
- Call `SQLitePCL.Batteries_V2.Init()` in MauiProgram before DB access.
- Persist ScanResult with fields: Id, RawValue, Type, ScannedAt, IsFavorite (default false to avoid future migration).

## Save Coalescing
- Writes use SQLiteAsyncConnection; simple initialization+insert pattern used for MVP. No coalescing queue implemented beyond async usage.

## Testing
- Move toward automated tests: add unit tests for ViewModels and storage as high priority for Phase 2.

## CI/CD
- Introduce a minimal CI workflow (build + restore + targeted unit tests + dependency scan) as a Phase 2 priority.

## Performance
- Debounce set to 2000ms to prevent duplicate detections. Disable scanning when navigating away to reduce CPU/battery usage.

## Testing Environment
- **Target Platforms**: Windows and Android 14 (API level 34) on physical device.
- **Rationale**: Single physical Android 14 device represents modern Android targets; Windows desktop testing allows cross-platform validation.
- **Manual Testing**: All feature validation (scanner, gallery, torch, zoom, parsers, history, generator, settings) is performed on this matrix before Phase 2 acceptance.
- **CI Testing**: Automated unit tests run on CI pipeline; device-specific tests (camera, permissions, storage) validated manually on target platforms.

# Current Work

## In Progress / Stabilization
- Manual verification on Android emulator/device: camera feed, barcode detection, debounce, navigation, persistence, and permission flows.
- Address NuGet warnings and dependency advisories (sqlite-net-pcl resolution messages and SQLitePCLRaw advisory) — determine upgrade or mitigation strategy.
- Decide and pin ZXing package version or confirm compatibility approach used in MVP (CameraBarcodeReaderView without a builder extension is validated).
- Add CI workflow and initial unit tests for storage and ViewModels.

## Next Tasks (short-term)
- Fix any runtime issues surfaced by manual QA and edge-case scans (empty/malformed payload handling).
- Implement vulnerability remediation plan or acceptable-mitigation notes for SQLitePCLRaw.
- Create CI workflow: restore, build, targeted unit tests, and dependabot or similar for dependency updates.
- Add basic unit tests covering SqliteStorageService and HistoryViewModel behaviors.

## Planned Phase 2 Work
- Static-image (gallery) scanning, torch/zoom, WiFi/vCard/calendar parsing, favorites in History, QR generator, and Settings.
- Design and create ADRs for any architectural changes required by Phase 2 (e.g., adding interfaces, repository layer).

## Blockers / Resolved
- Resolved: ZXing builder extension confusion — implementation used CameraBarcodeReaderView directly and validated runtime behavior across tested package versions. If a future package exposes a preferred builder extension, revisit.
- Open: Dependency advisory for SQLitePCLRaw.lib.e_sqlite3 requires review and either an upgrade or a documented mitigation.

# Known Issues

## Bugs
- NuGet warnings: sqlite-net-pcl resolution message and a vulnerability advisory for SQLitePCLRaw.lib.e_sqlite3. These require review and possible upgrades; tracked as a remediation task.

## Technical Debt
- A few legacy calls surfaced that referenced Application.Current.MainPage or synchronous dialog patterns; they were converted during MVP refactor but require a quick pass to ensure no remaining obsolete usages.
- Docs: keep the how-to/zxing guide updated with the tested ZXing package/version used for MVP.

## Limitations
- MVP targets Android only. Windows/other targets build but camera controls are Android-specific and may be conditionally included in csproj.

# Future Work

## Planned Features (Phase 2)
- Static-image (gallery) scanning, torch/zoom, WiFi/vCard/calendar parsing, favorites in History, QR generator, and Settings.

## Nice-to-have Improvements
- Expand automated tests (integration tests for navigation and scanning flows where feasible).
- Add dependency vulnerability scanning and scheduled upgrades.
- Improve relative-time formatting with a helper or small library.

## Refactoring Candidates
- Introduce a lightweight Scanner abstraction if additional scan sources are added.
- Extract platform-specific helpers for settings deep-link and permission flows.

# Open Questions

## Decisions Pending
- How to remediate the SQLitePCLRaw advisory in production builds (upgrade or accept with mitigation).
- Scope and cadence for Phase 2: balance rapid feature delivery vs. adding automated test coverage and CI.

## Research Topics
- Best practice for navigating to Android app settings from MAUI reliably across Android API levels.

# Notes

- Implementation references: `.github/docs/plans/qr-scanner-mvp--implementation-plan.md`, `.github/docs/plans/qr-scanner-app-spec-mvp-phase2.md`, and `.github/docs/plans/how-to-use-zxing.net.maui.md`.
- Keep project-context.md focused on short-lived, actionable items — long-term decisions belong in ADRs under `.github/docs/adr/`.
