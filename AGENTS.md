# AGENTS.md — MarioTest

Context for AI agents working in this repo. Read this file first.

## Project

NumTalk Unity Developer assignment: a self-contained **mobile touch 3D platformer** prototype (Mario lineage, not a clone). One playable scene, ~8-hour timebox, primitives only.

Full brief requirements live in `.cursor/rules/project-context.mdc` (distilled from the NumTalk assignment PDF). Assignment constraints are fixed; **implementation is not** — do not assume design choices that have not been made yet.

## Where things live

| What | Where |
|------|-------|
| Coding standards, syntax, patterns | `.cursor/rules/` (e.g. `unity-csharp.mdc`) |
| Assignment constraints & build order | `.cursor/rules/project-context.mdc` |
| Feature design (agent summary) | Subfolder `AGENTS.md` |
| Feature design (detail) | `docs/features/` |
| Repo-wide process | This file |

Do not duplicate rules across both places.

## Documentation policy

- **This file** — repo-wide process only.
- **Subfolder `AGENTS.md`** — lean agent context per code area: status, architecture, key decisions, links to docs. Not the full feature write-up.
- **`docs/features/`** — fuller feature descriptions (behavior, formulas, test plans). One file per feature when non-trivial. Index: `docs/README.md`.
- Each `AGENTS.md` must **link to `docs/features/`** when a feature doc exists; do not duplicate long-form design in `AGENTS.md`.
- **Tuned numbers** — defaults live in ScriptableObject assets (`PlayerTuning`, etc.) and scene/prefab refs; feature `AGENTS.md` describes *what* to tune, not duplicate values. Final shipped numbers go in `README.txt`.
- Do not invent architecture, APIs, or folder layouts that do not exist in the repo yet.

## Deliverables (for later)

- `README.md` — version, how to run, in/out, known issues, tuned movement numbers, layer layout
- `DECISIONS.md` — three hardest calls + trade-offs; where next; AI note (generated vs rewritten, one assistant mistake, one rejected suggestion)
- `DESIGN_NOTES.md` — working architecture notes to pull from when writing deliverables

These are written as features ship, not upfront.
