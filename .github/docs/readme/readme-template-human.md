# README Template — Human Reference
> Use this as your personal checklist and writing guide when authoring a README.md.
> Sections marked ⚠️ are required. All others are recommended or optional.

## Rule of thumb

### A good README

A **good README** answers three questions for someone who just cloned the repo cold:
> 1. What is this?
> 2. How do I run it?
> 3. How do I contribute without breaking things?

### Which section to use?

Section:
- 1–11 are the sweet spot for a solo MAUI project. 
- 12–16 make sense once you have tests wired up, CI running, or collaborators joining.
- 17-19 are useful for team/open-source projects
- 20-23 look professional

---


## ⚠️ 1. Project Name and One-Line Description

**What to write:** The repo name and a single sentence that tells a stranger what this is.

```
# MyProject
A cross-platform MAUI app for managing a categorised shopping list, built on .NET 10.
```

---

## ⚠️ 2. What It Does / Problem It Solves

**What to write:** 2–4 sentences. What problem does it solve? Who is it for?
Keep this grounded — no marketing language.

> Tip: Write this last. It's easier after you've written the rest.

---

## ⚠️ 3. Technology Stack

**What to write:** Bullet list. Framework, language version, notable libraries, persistence layer.

- .NET MAUI (net10.0)
- C# with nullable-aware types
- XAML with compiled bindings
- Local JSON persistence
- xUnit for unit tests

---

## ⚠️ 4. Prerequisites

**What to write:** What must be installed before the developer can even open this project?

- .NET 10 SDK + MAUI workload
- Visual Studio 2026 (or `dotnet` CLI)
- Android/iOS SDK if targeting those platforms

---

## ⚠️ 5. How to Build and Run

**What to write:** Exact commands. Don't assume knowledge. Include platform-specific notes.

```bash
git clone https://github.com/you/repo.git
cd repo
dotnet build Project/Project.csproj
dotnet run -p Project/Project.csproj -f net10.0-android
```

> Tip: Test these commands on a clean machine before publishing.

---

## ⚠️ 6. Project Structure

**What to write:** A tree or bullet list of top-level folders with one-line descriptions.
Only include folders that actually exist in the repo.

```
ProjectName/
├── App.xaml(.cs)         — App entry and resources
├── AppShell.xaml         — Shell navigation
├── Pages/                — ContentPages
├── ViewModels/           — Page state and commands
├── Services/             — Business logic and persistence
├── Resources/            — Fonts, images, styles
└── Platforms/            — Platform-specific code
tests/                    — Unit tests (shared-source)
```

---

## 7. Architecture Overview

**What to write:** A short prose description of the layers/components and how they connect.
Add an ASCII diagram or Mermaid diagram if the structure is non-obvious.

```
UI (Pages + ViewModels)
    ↓
Services (business logic)
    ↓
JSON file (AppDataDirectory)
```

---

## 8. Key Features

**What to write:** A short bullet list of what the app actually does. Keep it factual.

- Categorical reusable items
- Persistent shopping list (JSON, survives restarts)
- Shell navigation between pages
- Compiled XAML bindings

---

## 9. Persistence / Data Layer Notes

**What to write:** Where data lives, how it's loaded/saved, and any important constraints.

> Tip: If you ever swap out persistence (e.g., JSON → SQLite), update this section first.

---

## 10. Configuration and Environment Setup

**What to write:** Any environment variables, app settings, or config files the developer needs to know about.

---

## 11. Coding Standards

**What to write:** The conventions this repo follows. Helps contributors stay consistent.

- Async/await for all I/O
- Compiled XAML bindings (`x:DataType`)
- Small, focused classes
- `ILogger<T>` for logging
- Threading: ViewModel methods that mutate UI-bound properties assume the caller is on the UI thread. Callers should marshal using <c>MainThread.InvokeOnMainThreadAsync</c> or <c>MainThread.BeginInvokeOnMainThread</c> when invoking those methods from background threads.

---

## 12. Testing

**What to write:** How to run tests, what's covered, and any test helpers or fakes available.

```bash
dotnet test tests/Tests.csproj
```

---

## 13. Troubleshooting / Common Gotchas

**What to write:** The problems you actually hit during development. Save future-you the time.

> [!WARNING]
> If the app fails to start on a platform, verify the MAUI workload is installed:
> `dotnet workload list`

---

## 14. CI/CD (if applicable)

**What to write:** What pipelines exist, where they're defined, and what they check.
Only include this if CI actually exists in the repo.

---

## 15. Contributing

**What to write:** How to contribute. Branch strategy, PR expectations, code style reminders.

---

## 16. License

**What to write:** One line pointing to the LICENSE file.
If no license exists yet, add one before making the repo public.

```
See LICENSE in the repository root.
```

---

## Optional Sections

### Useful for team/open-source projects

17. Deployment Instructions 
    - When the app has a deployment step.
18. Changelog / Versioning
    - When releasing versions publicly.
19. Badges (build, coverage, license)
    - When CI and tests are wired up 

### Optional but professional

20. Roadmap / Known Limitations 
    - When the project is actively develope|
21. Credits / Acknowledgements
    - When using significant third-party work
22. Security Policy
    - When the repo is public and security matters
23. Links to related docs or ADR
    - When ??

---