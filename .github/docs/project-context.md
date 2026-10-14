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
Phase 2 feature work has progressed through implementation-plan item 8 (Settings). Phase 2 is not yet accepted: automated tests, CI, and full acceptance/manual verification remain.

## Current Sprint / Focus
Complete implementation-plan items 9-11: add focused unit tests, establish CI, and verify the Phase 2 acceptance criteria on target devices. Resolve any functional gaps found during that verification.

## Recently Completed
- Added gallery image scanning, live-camera torch and pinch zoom.
- Added WiFi, vCard, and calendar payload parsers and classification.
- Added History favorites, favorites-only filter, and clear-all.
- Added QR generation with a 512-character input limit and save/share actions.
- Added persistent sound and haptic settings.
- Expanded Shell tabs to Scanner, History, Generator, and Settings; retained ResultDetail as a routed page.

# Current Decisions

## UI
- Keep the dark-only theme and existing Shell navigation while completing Phase 2 verification; see `architecture.md` for the current design.

## Persistence
- No persistence strategy change is planned for Phase 2. Current SQLite wiring and model are documented in `architecture.md`.

## Save Coalescing
- No write-coalescing queue is implemented or planned for current scope; live camera duplicates are handled by the scan debounce.

## Testing
- No test project is configured yet. Add unit coverage for parsers, storage, favorites/filtering, and generator/settings behavior.
- Manual verification remains for Android 14 (API 34) on a physical device and the Windows build. Camera and other device-dependent behavior will require manual testing.

## CI/CD
- No GitHub Actions workflow is configured yet. Add a minimal workflow for restore, build, tests, and dependency checks.

## Performance
- Live camera scanning uses a 2-second debounce and is disabled when ScannerPage disappears.

# Current Work

## In Progress
- Add the test project and CI workflow from implementation-plan items 9-10.
- Run manual verification against all Phase 2 acceptance criteria on Android and check Windows build behavior.

## Next Tasks
- Implement tests for payload parsers, storage, HistoryViewModel, and generator/settings behavior.
- Add a GitHub Actions workflow for restore, build, tests, and dependency checks.
- Complete manual testing, including malformed/empty scan data, image decode, camera controls, persisted preferences, and generated QR scannability.
- Confirm the remaining Phase 2 acceptance gaps before calling the milestone complete.


# Known Issues

## Bugs
- Resolve the planned type-specific actions for WiFi, vCard, and calendar values or explicitly defer that acceptance criterion.
- Malformed/empty decode handling still needs test-vector coverage and verification.
- Generator Save writes PNG files to app-private `FileSystem.AppDataDirectory`, so users cannot readily find them in the gallery or normal file browser.
- Generator Save and Share regenerate from the current input rather than the content used for the displayed preview; editing input after generation can make the preview differ from the saved or shared image.

## Technical Debt
- App behavior has no automated test coverage yet.

## Limitations
- Camera, gallery, torch, and zoom behavior still requires target-device verification.

# Future Work

## Planned Features (Phase 2)
- Complete and verify Phase 2; feature work has reached Settings, but tests, CI, and acceptance validation remain.

## Nice-to-have Improvements
- Add integration coverage for navigation and scanning flows where practical.
- Add scheduled dependency updates after the initial CI workflow is in place.

## Refactoring Candidates
- Revisit how structured parser data is carried from scanning into ResultDetail if type-specific actions are added.

# Open Questions

## Research Topics
- Reliable test strategy for MAUI-dependent behavior versus pure parser and ViewModel logic.

# Notes

- Implementation progress: `.github/docs/plans/phase2-implementation-plan.md` (feature work through item 8; items 9-11 and acceptance gaps remain).
- Architecture details: `.github/docs/architecture.md`.
- Keep project-context.md focused on short-lived, actionable items — long-term decisions belong in ADRs under `.github/docs/adr/`.
