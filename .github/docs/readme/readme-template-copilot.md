# README Generation Instructions for GitHub Copilot
# 
# This file is a prompt template. When generating a README.md for a .NET MAUI project,
# follow every rule below exactly. Do not invent structure, folders, or features
# that are not present in the actual source files.
#
# RULES:
# 1. Only describe what exists in the repository. Do not add aspirational sections.
# 2. Do not add CI badges unless a workflow file exists in .github/workflows/.
# 3. Do not list folders (Pages/, ViewModels/, Services/, tests/) unless they exist.
# 4. Do not include testing helpers or fakes by name unless they exist in the codebase.
# 5. Use GitHub Markdown callout syntax ([!TIP], [!WARNING]) for tips and warnings.
# 6. Remove the closing "say the word and I'll expand" line — do not include meta-commentary.
# 7. Use present tense. Describe the current state, not intended state.
# 8. All bash commands must be verified against the actual .csproj path and name.

---

# [PROJECT_NAME]

[One sentence: what this project is and what it does. No marketing language.]

> [!TIP]
> [One practical tip about the design philosophy or intended usage pattern.]

## Project Description

[2–4 sentences. What problem it solves, who it's for, and the design approach.
Example: "MAUI_ShoppingList is a cross-platform shopping list app built on .NET MAUI.
It uses local JSON persistence, Shell navigation, and a simple service layer — no ORM,
no complex MVVM framework."]

---

## Technology Stack

[List only technologies actually used. Check the .csproj and using statements.]

- .NET MAUI ([TARGET_FRAMEWORK] — e.g. net10.0)
- C# [VERSION] with nullable-aware types
- XAML with [compiled bindings / standard bindings — check x:DataType usage]
- [Persistence: JSON / SQLite / in-memory — check service implementations]
- [Test framework if tests exist — check tests/ folder]

---

## Prerequisites

[Only list what is actually required to build and run.]

- .NET [VERSION] SDK with MAUI workload (`dotnet workload install maui`)
- [IDE: Visual Studio 20XX / Rider / dotnet CLI]
- [Platform SDK if required: Android SDK, Xcode, etc.]

---

## Quick Start

### Clone and build

```bash
git clone https://github.com/[OWNER]/[REPO].git
cd [REPO]
dotnet build [PATH_TO_CSPROJ]
```

### Run

```bash
# Replace net10.0-android with your target framework
dotnet run -p [PATH_TO_CSPROJ] -f [TARGET_FRAMEWORK]
```

### Live reload during development

```bash
dotnet watch -p [PATH_TO_CSPROJ] run -f [TARGET_FRAMEWORK]
```

> [!WARNING]
> [Describe the most common build failure and how to diagnose it.
> Example: "If the build fails with NETSDK1147, run `dotnet workload install maui`
> and verify with `dotnet workload list`."]

---

## Project Structure

[ONLY LIST FOLDERS AND FILES THAT EXIST IN THE REPOSITORY.
Remove any line below that does not correspond to an actual path.]

```
[PROJECT_FOLDER]/
├── App.xaml(.cs)           — App entry and resource registration
├── AppShell.xaml(.cs)      — Shell navigation definition
├── MainPage.xaml(.cs)      — Primary UI entry point
├── MauiProgram.cs          — DI setup, fonts, logging
├── Pages/                  — [Include only if folder exists]
├── ViewModels/             — [Include only if folder exists]
├── Services/               — [Include only if folder exists]
├── Resources/              — Fonts, images, styles
└── Platforms/              — Platform-specific bootstrapping
tests/                      — [Include only if folder exists]
.github/                    — [Include only if folder exists]
```

---

## Architecture

[Describe the actual layer structure observed in the source.
Include an ASCII diagram only if the project has more than 2 meaningful layers.]

```
[LAYER_1: e.g. Pages + ViewModels (UI)]
        ↓
[LAYER_2: e.g. Services (business logic)]
        ↓
[LAYER_3: e.g. JSON file / AppDataDirectory]
```

---

## Key Features

[Bullet list. Derived from actual code — ViewModels, Services, XAML pages.
Do not list features that are planned but not implemented.]

- [Feature 1]
- [Feature 2]
- [Feature 3]

---

## Persistence

[Describe the actual persistence mechanism found in the codebase.
Where is data stored? How is it loaded? When is it saved?]

- Storage location: `FileSystem.AppDataDirectory` (sandboxed per platform)
- Load: on app startup, if the file is present
- Save: after each mutation (eager persistence)

Implementation notes:
- [Note 1 — e.g. async/await for all file I/O]
- [Note 2 — e.g. no ORM or database; keep services focused]

---

## Configuration and Extensibility

[Describe actual configuration points in the code.
MauiProgram.cs registrations, app settings, feature flags, etc.]

- [Config point 1 — e.g. Register services in MauiProgram.cs]
- [Config point 2 — e.g. Add categories via the items provider service]

---

## Coding Standards

[Include only if there is a consistent pattern visible in the codebase.]

- Async/await for all I/O operations
- Compiled XAML bindings (`x:DataType`) where applicable
- Small, focused service classes — no repositories or generic abstractions unless needed
- `ILogger<T>` for logging
- `Grid` and `CollectionView` for layouts; avoid nesting `ScrollView` inside `CollectionView`

---

## Testing

[Include this section ONLY if a tests/ folder or test project exists.]

```bash
dotnet test [PATH_TO_TEST_PROJECT]
```

- [What is tested: services / viewmodels / persistence]
- [Test helpers available — only list by name if they exist in source]

---

## Troubleshooting

> [!WARNING]
> [Most common runtime issue and how to diagnose it.]

- Build failures: run `dotnet --info` and `dotnet workload list`
- Platform not starting: verify target framework is supported by installed SDK
- Runtime errors: check `ILogger` output and device/emulator logs

---

## Contributing

[Keep this brief. Describe the actual conventions used in the repo.]

- Keep pages thin; put logic in ViewModels and Services
- Use compiled XAML bindings
- Use async/await for all persistence operations
- Add unit tests for new service logic

---

## License

[Choose one of:]
See the [LICENSE](LICENSE) file in the repository root.
<!-- OR if no license exists: -->
No license file detected. Add a LICENSE file before making this repository public.

---
# END OF TEMPLATE
# Remove all comments (lines starting with #) and all [PLACEHOLDER] markers before publishing.
