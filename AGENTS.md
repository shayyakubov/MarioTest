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
| Feature design (once decided) | Subfolder `AGENTS.md` |
| Repo-wide process | This file |

Do not duplicate rules across both places.

## Documentation policy

- **This file** — repo-wide process only.
- **Subfolder `AGENTS.md`** — added **when we establish a feature's design**, not before. One per meaningful area (e.g. a scripts subfolder once movement is designed).
- Each subfolder doc should capture: what the feature does, key decisions made, tuned values, known edge cases, and what is explicitly out of scope for that feature.
- Do not invent architecture, APIs, or folder layouts that do not exist in the repo yet.

## Deliverables (for later)

- `README.md` — version, how to run, tuned numbers, layer layout
- `DECISIONS.md` — three hardest technical calls + AI disclosure
- `DESIGN_NOTES.md` — working architecture notes to pull from when writing deliverables

These are written as features ship, not upfront.
