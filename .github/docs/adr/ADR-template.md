---
title: ADR-XXX - <Decision Title>
status: Proposed # Proposed | Accepted | Superseded | Deprecated
date: YYYY-MM-DD
deciders:
  - <Name>
related:
  - architecture.md
  - AGENTS.md
  - project-context.md
supersedes: null
superseded_by: null
tags:
  - architecture
  - <topic>
purpose: >
  Documents a significant architectural or engineering decision,
  including its context, rationale, consequences, and alternatives.
update_frequency: Never (immutable after Accepted, except status updates).
goal: Capture **why** a decision exists, not write a design document
---

# ADR-XXX: <Decision Title>

## Status *(Required)*

**Proposed** | **Accepted** | **Superseded** | **Deprecated**

---

# Context *(Required)*

Describe the problem, background, and circumstances that led to this
decision.

Questions to answer:

- What problem are we solving?
- Why is this decision needed?
- What constraints exist?
- What requirements influenced the decision?

---

# Decision *(Required)*

Describe the decision that has been made.

State it clearly and unambiguously.

Include important implementation rules if needed.

---

# Rationale *(Required)*

Explain **why** this solution was chosen.

Examples:

- Simplicity
- Performance
- Maintainability
- User experience
- Reliability
- Team preference
- Platform limitations

---

# Consequences *(Required)*

## Benefits

- ...
- ...
- ...

## Drawbacks

- ...
- ...
- ...

## Risks

- ...
- ...

---

# Alternatives Considered *(Recommended)*

## Option A

Description

### Pros

- ...

### Cons

- ...

### Why rejected

...

---

## Option B

...

---

# Implementation Notes *(Optional)*

Implementation details that future developers should know.

Examples:

- Classes involved
- Entry points
- Expected lifecycle
- Performance considerations

Avoid copying code here.

---

# Examples *(Optional)*

Small examples or diagrams illustrating the decision.

Example workflow:

```text
User Action
      ↓
ViewModel
      ↓
SaveService
      ↓
Persistence
```

---

# Related Decisions *(Optional)*

Links to other ADRs.

Examples:

- ADR-001 Project Philosophy
- ADR-003 UI Thread Policy
- ADR-007 Error Handling

---

# Future Considerations *(Optional)*

Things that might cause this ADR to be revisited.

Examples:

- Application grows significantly.
- New platforms are added.
- Performance requirements change.
- Multi-user synchronization is introduced.

---

# References *(Optional)*

External documentation, articles, GitHub issues, discussions, etc.

---

# Revision History *(Optional)*

| Date | Change |
|------|--------|
| YYYY-MM-DD | Initial version |