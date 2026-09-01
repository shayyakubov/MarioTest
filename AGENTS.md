# AGENTS.md — MarioTest

Context for AI agents working in this repo. Read this file first.

## Project

NumTalk Unity Developer assignment: a self-contained **mobile touch 3D platformer** prototype (Mario lineage, not a clone). One playable scene, ~8-hour timebox, primitives only.

Full brief requirements live in `.cursor/rules/project-context.mdc`. Assignment constraints are fixed; **implementation is not** — do not assume design choices that have not been made yet.

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
- Each `AGENTS.md` should capture: what the feature does, key decisions, known edge cases, out of scope — **briefly**, with pointers to `docs/features/` for depth.
- **Tuned numbers** — defaults live in code (`PlayerTuning`, etc.) and scene/prefab; feature `AGENTS.md` describes *what* to tune, not duplicate values. Final shipped numbers go in `README.md`.
- Do not invent architecture, APIs, or folder layouts that do not exist in the repo yet.

## Deliverables (for later)

- `README.md` — version, how to run, tuned numbers, layer layout
- `DECISIONS.md` — three hardest technical calls + AI disclosure
- `DESIGN_NOTES.md` — working architecture notes to pull from when writing deliverables

These are written as features ship, not upfront.
