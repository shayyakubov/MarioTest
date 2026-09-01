# Feature docs

Human-readable feature descriptions. Kept separate from `AGENTS.md` so agent context stays lean.

## What goes where

| Doc | Audience | Content |
|-----|----------|---------|
| **`docs/features/`** (here) | Humans + agents implementing a feature | Behavior, formulas, test plans, edge cases |
| **`AGENTS.md`** (per code folder) | Agents | Status, architecture, key decisions, links to docs |
| **`.cursor/rules/`** | Agents | Assignment constraints, coding standards — not feature design |
| **`DESIGN_NOTES.md`** | Deliverable prep | Cross-cutting notes for README / DECISIONS |
| **`README.md`** | Players / reviewers | How to run, final tuned numbers (when shipped) |

## Conventions

- One file per feature or sub-feature when it gets non-trivial (`player-jump.md`, not a giant monolith).
- **No duplicated tuning numbers** — defaults live in code; docs describe what to tune.
- Pseudocode and diagrams OK; avoid copy-pasted source or line references that go stale.
- When a feature ships, update its doc status line and the folder `AGENTS.md` status table.

## Index

| Feature | Status | Doc |
|---------|--------|-----|
| Player movement (horizontal, gravity, ground) | Implemented | [player-movement.md](features/player-movement.md) |
| Player jump (variable, coyote, buffer) | Designed, not implemented | [player-jump.md](features/player-jump.md) |
