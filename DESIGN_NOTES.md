# Design Notes

Working notes for the NumTalk assignment. Pull from here when writing [DECISIONS.md](DECISIONS.md) and `README.md`.

Hard problems with solutions are logged in **[DECISIONS.md](DECISIONS.md)** — add new entries there as they come up.

---

## Movement model

**Choice:** Dynamic Rigidbody with horizontal acceleration servo — not CharacterController, not kinematic mover.

**Why:**
- Crate pushing works through physics collisions, not custom sweep/push code
- Player capsule uses **zero friction** — no wall stick; run/brake owned by motor, not PhysicMaterial
- Ice / surfaces later: scale `horizontalAccelerationMultiplier` from ground hit, not friction
- Knockback and external forces compose with player input without a hard speed cap
- Assignment explicitly notes CharacterController pushing "does not come for free"

**Horizontal approach:**
- Compute a target horizontal velocity from move direction and max speed
- Compare against current horizontal velocity (Y stripped)
- `MoveTowards` current → target, capped by max acceleration × surface multiplier × dt
- Set `velocity.x/z` directly — same servo math as before, without `AddForce`

**Deliberately not done:**
- Hard-clamping total horizontal velocity — knockback must be able to exceed max speed
- Per-axis acceleration clamp — diagonals would reach max speed faster than cardinals

**Vertical (partial):**
- Custom gravity with separate rise/fall strength and terminal fall speed — implemented
- Jump with variable height, coyote time, jump buffer — implemented (see docs/features/player-jump.md)
- Disable built-in gravity; movement applies gravity manually — done

---

## Component architecture

| Component | Responsibility |
|-----------|----------------|
| PlayerController | MonoBehaviour: receives IPlayerInput, tuning, physics tick, scene refs |
| IPlayerInput | Interface: Move, JumpHeld, JumpPressedThisFrame |
| PlayerInputReader | Plain class: only class that talks to Input System; implements IPlayerInput |
| PlayerMovement | Plain class: horizontal acceleration servo |
| GroundDetector | Plain class: one non-alloc spherecast per fixed step on Ground layer |
| PlayerTuning | Serializable tuning data — edited on PlayerController, consumed by PlayerMovement |
| GameBootstrap | Composition root: creates reader, wires Initialize, owns input enable/disable |

**Separation principle:** Controller handles intent and Unity lifecycle; PlayerMovement handles physics logic. Controller delegates movement — it does not apply forces directly.

**Plain C# over MonoBehaviour:** PlayerTuning, PlayerMovement, PlayerInputReader, and GroundDetector are plain classes. PlayerController is a thin MonoBehaviour for lifecycle and scene refs.

**Input wiring:** GameBootstrap holds the Input Actions asset and PlayerController reference. PlayerController.Initialize receives IPlayerInput — no Input System or bootstrap knowledge. Same-GameObject Rigidbody and CapsuleCollider via GetComponent in Awake.

**Layers:** `PhysicsLayers` resolves layers by name from Project Settings (no hardcoded indices). Ground probes use `PhysicsLayers.GroundMask`.

---

## Tuning

**Choice:** PlayerTuning is a serializable class edited directly on PlayerController in the inspector.

**Why:**
- Standard Unity pattern for per-prefab tuning — no separate asset required
- Tuning may eventually be overridden from remote config at runtime by building a new PlayerTuning instance
- PlayerMovement only reads tuning values; it does not care where they came from

**Current flow:** PlayerController holds serialized tuning. On startup it passes that tuning into PlayerMovement.

**Future flow (remote config):** When remote config arrives, build a new PlayerTuning from fetched values and replace what movement uses.

---

## Input

New Input System. PlayerInputActions asset defines Move and Jump. IPlayerInput exposes Move, JumpHeld, and JumpPressedThisFrame.

Keyboard (WASD / arrows) works for editor and desktop testing. Touch virtual stick will call PlayerInputReader.SetTouchMove when implemented.

---

## Rigidbody setup

| Setting | Value |
|---------|-------|
| Kinematic | false |
| Use gravity | false (custom gravity later) |
| Linear damping | 0 |
| Angular damping | 0 |
| Constraints | freeze rotation X and Z |
| Collision detection | Continuous Dynamic |
| Interpolation | Interpolate |
| Collider | Capsule |

Linear damping is zero because movement owns deceleration; drag fights the acceleration servo.

---

## Known deferred risks

1. **Moving platforms** — friction alone won't reliably transfer velocity from kinematic movers; plan explicit platform velocity in GroundDetector / movement.
2. **Knockback decay** — steering partially acts as decay; assignment asks for named impulse + decay — plan a separate knockback velocity vector in Phase 4.
3. **Slopes** — camera-relative input must eventually project onto ground normal from GroundDetector.
4. **Mixed force + direct Y** — only change Y velocity on jump frames; never every frame on horizontal axes.

---

## Phase 1 tuned values (initial)

| Field | Value |
|-------|-------|
| maxSpeed | 6 |
| maxAllowedAcceleration | 50 |
| moveInputDeadzone | 0.1 |

Tune on PlayerController in the inspector (maxSpeed, maxAllowedAcceleration, moveInputDeadzone).

---

## Assignment cut order (reminder)

If time runs out, cut in this order: bonus → feel → sudden event → course length. Never cut the collision block.
