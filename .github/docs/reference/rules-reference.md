---
description: Rules file authoring reference
document_type: reference
authoritative: true
---

# Rules File — Authoring Reference

## Purpose

Not loaded by any agent at runtime. This is the annotated version of *whichever
file currently holds project rules* — `.github/copilot-instructions.md` for
now, per [ADR-001: Defer AGENTS.md](.github/docs/adr/ADR-001-defer-agents-md.md) If that
decision is superseded and rules move to `AGENTS.md`, this reference still
applies unchanged — the headings and litmus tests don't depend on which file
they live in. Written for whoever maintains the real file (you, or a
documentation-architect agent working on your behalf).

## Sources

Sourced from two points of convergence: ["How to Write an AGENTS.md File" (Atlan,
2026)](https://atlan.com/know/how-to-write-agents-md/) and ["Writing a Good
AGENTS.md" (Schmid, 2026)](https://www.philschmid.de/writing-good-agents), which
cites the ETH Zurich "Evaluating AGENTS.md" study. Both sources describe
AGENTS.md specifically, but the underlying findings are about rules files in
general — they apply just as much to `copilot-instructions.md` while that's the
file doing the job.

## The core fact behind every rule below

The rules file loads into **every session**, whether the task needs it or not.
Every line is a standing token cost. The ETH Zurich study found that content the
model already knows from training — generic principles, restated best
practices, codebase overviews — doesn't just fail to help, it measurably
*reduces* task success while raising inference cost. The file earns its keep
only with things the agent could not otherwise infer: this project's specific
tradeoffs, this project's specific commands, this project's specific hard
stops.

**Litmus test for any candidate line:** does it state a decision between two
reasonable alternatives, specific to this repo? Or does it restate a default the
model already leans toward (simplicity, readability, YAGNI, "clean code")? If the
latter, cut it — you get zero benefit and pay the token cost anyway.

## The four-file split

| File | Purpose | Changes | Loaded when |
|---|---|---|---|
| Rules file (`copilot-instructions.md` now; `AGENTS.md` if the trigger fires) | Rules — how code is written here | Rarely | Every session |
| `.github/copilot-instructions.md`, if separate from the rules file | Copilot-specific tone/review behavior | Rarely | Every Copilot session |
| `architecture.md` | System design, how the app is built | Occasionally | On demand / when relevant |
| `project-context.md` | Current work, active decisions | Frequently | On demand |

For the current, project-specific version of this split — the actual file paths in use — see the Documentation Map section in copilot-instructions.md. This table is the portable pattern; that section is this project's live instance of it.

Anything that changes more than "rarely" doesn't belong in the rules file, full
stop. If you're about to add a line about what you're currently working on, it
goes in `project-context.md` instead.

## Section-by-section

### Commands

The single highest-ROI section in the whole file (Atlan's 2,500-repo analysis).
The agent already knows `pytest` or `dotnet test` exist — it doesn't know your
flags, your env vars, or which command is CI-only. Every command here must be
exact and copy-pasteable. If a command is just the tool's own default with no
project-specific flag, leave it out — it adds nothing.

Good: `dotnet test --filter Category=Unit --logger "console;verbosity=detailed"`
Bad: `dotnet test`

### Design Principles

This is where "SaveAsync only persists — concurrency is handled separately" type
lines live. The shape that works: **name a decision this project made between two
plausible alternatives**, stated as a fact, not advice. Not "we value simplicity"
(model already does) but "coalescing is intentionally use-first, not eager"
(genuinely couldn't be inferred).

Apply the litmus test line-by-line. "Readability over DRY when abstraction would
reduce clarity" passes — it's a tradeoff call, not a restated default. "Prefer
simplicity over cleverness" fails — cut it.

### Boundaries

Three tiers, always in this order. This is the most production-tested pattern
across both sources — don't flatten it into a single list of "guidelines."

- **Always do** — autonomous, no confirmation needed. Small, safe, repeatable
  actions.
- **Ask first** — needs a human before proceeding. Architectural changes, new
  dependencies, anything with blast radius.
- **Never do** — hard stops. Must be *specific* — named files, named patterns,
  named actions. "Never bypass the coalescing queue for writes" works. "Don't
  make mistakes" doesn't — it's not actionable and the model already tries not
  to.

If you find yourself writing an "it depends" boundary, it's not ready for this
file — either make the condition explicit ("ask first if the change touches
`SaveService`") or leave it as a case-by-case call for review time.

### Copilot Review Behavior (when merged into copilot-instructions.md)

Distinct from Design Principles: this is about how Copilot should *respond*,
not what the project's technical decisions are. Tone, review posture, what not
to re-litigate. If rules ever move to a standalone `AGENTS.md`, this section
stays behind in `copilot-instructions.md` rather than migrating with the rest —
it's Copilot-specific, not project-specific.

### Testing

Only include this section if your setup has something non-obvious: a mocking
convention, a coverage threshold, a "never use a live connection in unit tests"
rule. If your testing is just "run the test command," that already lives in
Commands — delete this section rather than restating it.

### What's deliberately absent from the template

- **No codebase/directory overview.** The ETH Zurich data found these don't
  measurably help agents navigate faster — agents discover structure on their
  own about as fast either way. Only add one if this is a monorepo where an
  agent genuinely cannot tell packages apart without help, and even then keep
  it to a flat list with one-line purposes, not prose.
- **No code style prose.** Linters and formatters are faster, cheaper, and
  deterministic. Models are strong in-context learners and will follow the
  patterns already in your code. A style section here would just be restating
  what's already enforceable elsewhere.
- **No architecture explanation.** That's `architecture.md`'s job — pointer,
  not copy.
- **No current status / in-progress notes.** That's `project-context.md`'s job.

## Length target

Aim under ~150 lines. Some teams (HumanLayer) keep theirs under 60. If a section
is empty or thin, delete the heading rather than leaving a placeholder — an
unused heading is still a standing invitation to fill it with prose later.

## Before adding any new line, ask

1. Does this change more than "rarely"? → move it to `project-context.md`.
2. Could the model already infer this from training or from the code itself? →
   cut it.
3. Is it a tradeoff between two named alternatives, or a restated default? →
   keep only the former.
4. Is it a Boundary? → sort it into Always / Ask first / Never — don't leave it
   as flat advice.
5. Is it about Copilot's behavior rather than the project's? → keep it in
   Copilot Review Behavior, separate from Design Principles.
6. Would a link to another file do the job better than a copy? → link instead.
