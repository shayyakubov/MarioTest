# Technical decisions & hard problems

Running log of the trickiest issues in this project — what went wrong, why it was hard, and how we fixed it. Pull the top three into the assignment submission summary when shipping.

**Related docs:** [DESIGN_NOTES.md](DESIGN_NOTES.md) (architecture), [docs/features/](docs/features/) (feature behavior)

---

## How to add an entry

Copy this block:

```markdown
### Short title

**Problem:** What the player / dev saw.

**Why it was hard:** Root cause, non-obvious interactions, false leads.

**Solution:** What we changed and why that works.

**Where:** Files / components touched.
```

---

## 1. Player stuck mid-air against platform walls

**Problem:** Jumping at a platform you can't reach, the capsule hits the side of the cube and the player hangs in mid-air — no fall, feels "smashed" into the wall.

**Why it was hard:** Two separate bugs stacked and looked like one physics issue.

1. **False ground detection** — The downward `SphereCast` from the feet is a sphere (radius ~0.48 m), not a thin ray. When pressed against a platform near its top edge, the sphere can graze the platform's **top face** from the side. That hit has an upward normal, passes the slope filter, and sets `IsGrounded = true` even though the player is not standing on the platform. `PlayerMovement` then zeros vertical velocity while grounded (`vy = 0`), so the player stops falling while still on the wall.

2. **Wall friction** — Default capsule friction (~0.6) plus continuous horizontal `AddForce` into the wall creates friction forces that can oppose gravity ("wall climbing" / sticking).

**Solution:**

1. **Ground under feet** — After a ground hit, require the hit point to be horizontally within the capsule footprint (`IsHitUnderFeet` in `GroundDetector`).

2. **Zero-friction player material** — No wall-stick / friction climb from the capsule.

3. **Motor owns velocity** — Horizontal run uses `MoveTowards` on `velocity.x/z` directly (not `AddForce`). Accel/decel and future ice surfaces tune `maxAllowedAcceleration` × surface multiplier — friction is not used for braking.

**Where:** `GroundDetector.cs`, `PlayerController.ConfigureColliderMaterial()`, `PlayerMovement.ApplyHorizontalMovement`

---

## 2. Unity ignored new scripts (CS0246 / missing types)

**Problem:** New C# files (`GroundDetector`, `GroundDetectionSettings`, `PhysicsLayers`) existed in the repo but Unity did not compile them; other scripts reported missing types.

**Why it was hard:** The `.cs` files looked fine in the IDE. The failure was in `.meta` files — hand-written GUIDs were **31 characters** instead of 32. Unity rejected those metas and never imported the scripts.

**Solution:** Regenerate valid 32-character GUIDs in the affected `.meta` files (or delete metas and let Unity recreate them). **Do not hand-write `.meta` GUIDs.**

**Where:** `Assets/_Project/Scripts/**/*.meta`

---

## 3. Jump input skipped when move input was idle

**Problem:** Coyote time and jump buffer seemed broken when standing still or when move input was below the deadzone.

**Why it was hard:** `PlayerController.Update` had an early `return` when move magnitude was below the deadzone, which ran **before** jump input was forwarded to `PlayerMovement`.

**Solution:** Call `SetJumpInput` first in `Update`, before any move deadzone check or early return.

**Where:** `PlayerController.cs` (`Update`)

---

## 4. Variable jump height — gravity cut vs release event

**Problem:** Short hop vs full jump did not match the intended mental model. Code branched on `JumpHeld` every physics frame while rising (`riseGravity` vs `lowJumpGravity`), which is indirect — "is the button down right now?" rather than "was it a quick tap?"

**Why it was hard:** The gravity-multiply approach (Celeste-style) *does* approximate tap-vs-hold, but the same initial `jumpVelocity` plus per-frame held checks is harder to tune and explain than a one-shot cut on release.

**Solution (current):** Gravity branch on `_jumpHeld` while rising — acceptable for prototype; tunable via `riseGravity` / `lowJumpGravity` in `PlayerTuning`.

**Solution (if revisiting):** Add `JumpReleasedThisFrame` to `IPlayerInput`; on release while `vy > 0`, apply a `shortHopVelocityMultiplier` or cap. Keeps full jump on press, hop on early release — clearer for mobile tap.

**Where:** `PlayerMovement.ApplyGravity`, `IPlayerInput`, `docs/features/player-jump.md`

---

## Assignment submission (fill when shipping)

Pick the **three hardest** calls from the entries above (or add new ones) and summarize here for reviewers.

| # | Hardest call | One-line summary |
|---|--------------|------------------|
| 1 | | |
| 2 | | |
| 3 | | |

### AI disclosure

<!-- How AI tools were used on this assignment (required deliverable). -->
