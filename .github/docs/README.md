# .github/docs — AI-Facing Documentation Hierarchy

This folder holds documentation written for AI coding tools (currently GitHub
Copilot) and for whoever maintains that tooling. It contains the following folders and files:

| Folder | Role |
|---|---|
| `templates/` | Scaffolds to fill in when creating a new instance of something (e.g. `ADR-template.md`). Structural baseline only — no explanation of *why*. |
| `reference/` | Explains how a system works and why: what belongs where, litmus tests for inclusion, process rules. Not loaded at runtime — written for humans (and for a documentation-architect agent) maintaining the templates and the real files. |
| `adr/` | Architecture Decision Records — a durable log of significant, hard-to-reverse decisions and the reasoning behind them. See below. |
| `mvp-stage/` | MVP planning and specifications — short-term product decisions, implementation checklists, and user-flow clarifications. See `mvp-stage/plan` and `mvp-stage/test` for plan and testing material. |
| `testing/` | Testing guidance and strategies. See `.github/docs/mvp-stage/test/mvp-stage-testing-strategy.md` for the MVP testing strategy. |
| `readme/` | README templates for human and AI-facing docs (readme-template-human.md, readme-template-copilot.md). |
| `plans/` | High-level project plans and roadmaps. |

| File | Role |
|---|---|
| `architecture.md` | System design — how the app is built. Occasional changes. |
| `project-context.md` | Current work, active milestones, and frequently changing decisions. Prefer this for short-lived guidance rather than editing rules files. |

## This repo uses an ADR system

Significant architectural and engineering decisions are recorded as ADRs in
`.github/docs/adr/`, one file per decision, numbered sequentially. Immutable once Accepted.

- **Index of all ADRs:** `.github/docs/adr/README.md`
- **How the system works** (when to write one, numbering, status lifecycle,
  superseding): `.github/docs/reference/adr-reference.md`
- **Scaffold for a new one:** `.github/docs/templates/ADR-template.md`

If you're wondering *why* a rule in `copilot-instructions.md` exists, check
whether an ADR covers it before assuming it's undocumented — rules state
*what*, ADRs record *why*.

## Reference material

- `reference/rules-reference.md` — authoring guidance for whichever file
  currently holds project rules (`copilot-instructions.md` for now; see
  `adr/` for the decision behind that).
- `reference/adr-reference.md` — the ADR process itself, as above.

## Related, outside this folder

- `copilot-instructions.md` (`.github/copilot-instructions.md`) — the rules
  file Copilot loads every session. Points back into this hierarchy rather
  than duplicating it.