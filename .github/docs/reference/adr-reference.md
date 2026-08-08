---
description: ADR process reference
document_type: reference
authoritative: true
---

# ADR Process — Reference

Not loaded by any agent at runtime. Explains how the ADR system in this repo
works. For the scaffold to fill in when writing one, see
`.github/docs/templates/ADR-template.md`. For the list of ADRs that exist, see
`docs/adr/README.md` — keep that file a thin index, not a process explainer;
this document is where the process itself lives.

## What counts as ADR-worthy

Not every decision needs an ADR. Write one when a decision is:

- **Significant** — affects architecture, a cross-cutting policy, or how code
  gets written repo-wide (the kind of thing that would otherwise live in
  `copilot-instructions.md` / `AGENTS.md`, but needs the fuller context/
  rationale/alternatives record an instructions file has no room for).
- **Hard to reverse** — changing it later means real rework, not a one-line
  edit.
- **Non-obvious** — a future contributor (or agent) could plausibly ask "why
  is it done this way?" and the answer isn't in the code itself.

Skip an ADR for reversible, local, or self-evident choices — those are just
noise in the index. If in doubt, apply the same litmus test used for rules
files: does this need to persist as a *record*, not just as a *rule*? If it's
a rule going forward, it belongs in the rules file, not an ADR. If it's the
reasoning behind why that rule exists, it's an ADR — and the rule can point
back to it.

## Numbering and location

- One file per decision: `docs/adr/NNN-short-title.md`, zero-padded,
  sequential (`001-`, `002-`, ...).
- Number is assigned at creation time, from the next free number in
  `docs/adr/README.md`. Numbers are never reused, even if an ADR is later
  superseded or deprecated.
- `docs/adr/README.md` is the authoritative index — a table of number, title,
  status, and date, kept in sync whenever an ADR is added or changes status.

## Status lifecycle

| Status | Meaning |
|---|---|
| `Proposed` | Under discussion, not yet acted on |
| `Accepted` | In effect — this is the current decision |
| `Superseded` | Replaced by a newer ADR; kept for history |
| `Deprecated` | No longer relevant, but not replaced by a specific new decision |

**Immutable once Accepted** means the Context, Decision, Rationale, and
Consequences sections don't get rewritten after the fact — if the decision
changes, that's a *new* ADR, not an edit to the old one. The only field that
changes on an existing ADR is `status` (and `superseded_by`, when applicable),
plus an entry in its own Revision History table if you want a record of when
the status changed.

This matters for the same reason rules-file content has to earn its place:
an ADR is a record of what was decided and why *at the time*, not a living
design doc. If it could still be edited freely, it stops being trustworthy as
a record of history.

## Superseding a decision

When a new decision replaces an old one:

1. Write the new ADR normally, with its own next sequential number.
2. Set the new ADR's `supersedes` frontmatter field to the old ADR's number.
3. Set the old ADR's `superseded_by` field to the new ADR's number, and change
   its `status` to `Superseded`. Don't touch its Context/Decision/Rationale —
   those stay as written, describing the reasoning that held at the time.
4. Update `docs/adr/README.md` to reflect both entries' current status.

The old ADR stays in the repo. Deleting a superseded ADR destroys the history
of *why* the previous approach was chosen, which is often exactly what a
future decision-reversal needs to reference.

## Frontmatter fields worth a note

Most of `ADR-template.md`'s frontmatter is self-explanatory from the template
itself. A few worth calling out:

- **`related`** — link to files this decision touches (e.g.
  `copilot-instructions.md`, `architecture.md`), not other ADRs. Use
  `supersedes` / `superseded_by` and the "Related Decisions" body section for
  ADR-to-ADR links.
- **`deciders`** — even for a single-person repo, keep this populated; it's
  the answer to "who made this call" if it's ever questioned later.
- **`tags`** — keep this to a small, reused vocabulary (`architecture`,
  `ai-tooling`, `documentation`, etc.) rather than a new tag per ADR, so
  `docs/adr/README.md` can eventually group or filter by tag if the list grows.

## Keeping `docs/adr/README.md` thin

The index should stay a table, not a narrative:

```markdown
| # | Title | Status | Date |
|---|---|---|---|
| 001 | ... | Accepted | 2026-01-10 |
| 002 | ... | Superseded by 0004 | 2026-03-02 |
```

If you find yourself writing explanation *in* the index, it belongs in this
reference doc instead — same discipline as keeping `project-context.md` free
of content that belongs in `rules-reference.md`.
