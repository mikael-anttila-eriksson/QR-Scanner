# Copilot Instructions (project rules + Copilot behavior)

This file contains the project-specific rules that Copilot and contributors
must follow. It combines the repo's authoritative tradeoffs with Copilot
review/tone guidance. Keep changes here rare and intentional — frequent or
work-in-progress guidance belongs in `.github/docs/project-context.md`.

## Commands (exact, copy-pasteable)
- Build (solution):
  - `dotnet build QRScanner.slnx`
  - `dotnet build UIApp\UIApp.csproj`
- Build for platform (examples):
  - `dotnet build UIApp\UIApp.csproj -f net10.0-android`
  - `dotnet build UIApp\UIApp.csproj -f net10.0-windows10.0.19041.0`
- Run (Android emulator):
  - `dotnet run -p UIApp\UIApp.csproj -f net10.0-android`
- Run (Windows):
  - `dotnet run -p UIApp\UIApp.csproj -f net10.0-windows10.0.19041.0`
- Watch (rebuild on changes):
  - `dotnet watch -p UIApp\UIApp.csproj run -f net10.0-android`
- Clean:
  - `dotnet clean UIApp\UIApp.csproj`
- Tests:
  - No test projects are configured in this repository. Run `dotnet test <test-project.csproj>` once you add a test project.

Only include commands here that require project-specific flags or paths.

## Design Principles (project-specific tradeoffs)
- Single-project MAUI app (UI, services, platform code) — not a multi-project
  library-first architecture.
- Services own business logic directly. Avoid repository/CQRS patterns and
  heavy MVVM frameworks for MVP.
- Layout: prefer Grid over deeply nested StackLayouts.
- Controls: CollectionView (virtualized) for lists, SwipeView for item
  actions, Border for styled containers, BindableLayout for very small lists.
- Readability over DRY where abstraction would reduce clarity.

Persistence and DI mechanics are documented in `architecture.md`, not
restated here. These are decisions — not recommendations. Treat them as
settled unless an explicit ADR or an approved change updates them.

## Boundaries

### Always do (autonomous, safe actions)
- Register services and viewmodels in `MauiProgram.cs` using built-in DI.
- Use `x:DataType` on XAML pages where a ViewModel is used.

### Ask first (requires human review / approval)
- Any architectural change (new layer, new pattern, repository/CQRS,
  splitting the single-project layout).
- Adding a new package dependency (NuGet) or changing target platforms.
- Changing persistence strategy (e.g., moving from JSON to SQLite or a
  remote sync layer).
- Introducing global state that alters service lifetimes (e.g., turning a
  previously transient service into a long-lived singleton for cross-cutting use).

### Never do (hard stops)
- NEVER use ListView or TableView — use CollectionView or BindableLayout.
- NEVER nest ScrollView or CollectionView inside StackLayout (it breaks
  virtualization and scrolling semantics).
- NEVER reference SVGs in UI resources for the MVP; use PNG assets.
- NEVER use Frame for styled containers — use Border instead.
- NEVER bypass the coalescing or concurrency guard in the JsonStorageService
  when writing files.

## Copilot Review Behavior (how Copilot should respond)
- Respect the Design Principles and Boundaries as settled decisions; do not
  re-litigate them in review suggestions.
- Prefer local, minimal fixes over large architectural changes in review
  comments. When proposing architecture-level changes, cite an ADR or
  create one and mark the review as a design proposal.
- Do not suggest enterprise patterns (generic repositories, event buses,
  full-featured DI frameworks) unless explicitly requested.
- When suggesting code changes, include exact edit snippets and the fewest
  lines necessary to implement the fix.
- When a change touches DI registrations or app startup, point
  to `MauiProgram.cs` and `architecture.md`.

## Testing (non-default guidance)
- no set.

## Commit messages (authoritative)
Commit message guidance is maintained in `.github/commit-message-instructions.md`.
Copilot and contributors should follow that file when generating or suggesting
commit messages (Conventional Commits). Use that file as the single source of
truth; do not duplicate detailed commit-format rules here.

## Pointers (authoritative locations)
- System architecture: `.github/docs/architecture.md`
- Current work / active decisions: `.github/docs/project-context.md`
- MVP specification: `.github/docs/mvp-stage/plan/mvp-specification.md`
- ADRs (authoritative decisions): `.github/docs/adr/`
- MVP Testing guidance: `.github/docs/mvp-stage/test/mvp-stage-testing-strategy.md`
- Docs index: `.github/docs/README.md`

Keep this file concise (target <150 lines). If a rule changes often, move it
to `project-context.md` instead of updating this file.

## Documentation Map

Engineering docs are split by how often they change and what they contain. 
Don't restate content that already lives elsewhere — link to it instead. Use 
the four-file test below to determine where new content belongs.
 
**Four-file test — before writing anything new, ask:**
1. Is it a decision between named alternatives, with real tradeoffs? → ADR.
2. Is it a hard rule with no alternative ("never do X")? → Boundaries, above.
3. Is it acceptance criteria or expected behavior? → the current spec.
4. Is it how something is implemented or wired up? → `architecture.md`.
None of these fit → it's probably `project-context.md`, or doesn't need to be
written down at all.