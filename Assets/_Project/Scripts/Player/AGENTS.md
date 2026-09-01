# Player Movement

## Status

Phase 1 complete: horizontal movement only.

## Model

Dynamic `Rigidbody` player. Horizontal movement uses an acceleration servo; vertical (jump/gravity) deferred to Phase 3.

### Horizontal formula (Phase 1)

```
targetVelocity = moveDirection * maxSpeed
currentHorizontal = rigidbody.linearVelocity with Y = 0
deltaVelocity = targetVelocity - currentHorizontal
accelerationNeeded = deltaVelocity / fixedDeltaTime
clamp accelerationNeeded magnitude to maxAcceleration
AddForce(accelerationNeeded, ForceMode.Acceleration)
```

- Do **not** hard-clamp total horizontal velocity (knockback may exceed `maxSpeed`).
- Acceleration clamp uses **vector magnitude**, not per-axis.
- `moveDirection` is camera-relative, normalized; input magnitude applied before normalize (clamped to 1).

### Rigidbody settings

| Setting | Value |
|---------|-------|
| `isKinematic` | false |
| `useGravity` | false (custom gravity in Phase 3) |
| `linearDamping` | 0 |
| `angularDamping` | 0 |
| `constraints` | freeze rotation X and Z |
| `collisionDetectionMode` | Continuous Dynamic |
| `interpolation` | Interpolate |
| Collider | Capsule |

## Architecture

| Component | Responsibility |
|-----------|----------------|
| `PlayerController` | MonoBehaviour: receives `IPlayerInput`, tuning, camera ref; ticks `PlayerMovement` |
| `IPlayerInput` | Interface: `Move`, `IsJumpPressed` — portable across projects |
| `PlayerInputReader` | Plain class: only class that talks to Input System; implements `IPlayerInput` |
| `PlayerMovement` | Plain class: horizontal acceleration servo |
| `GroundDetector` | Phase 2 — not implemented |
| `PlayerTuning` | Serializable tuning — edited on `PlayerController`, consumed by `PlayerMovement` |
| `GameBootstrap` | Composition root: creates reader, calls `PlayerController.Initialize`, owns enable/disable |

Tune directly on `PlayerController` in the inspector.

**Wiring:** `GameBootstrap` holds the Input Actions asset and a `PlayerController` reference. On `Awake` it creates `PlayerInputReader` and passes it to `Initialize`. `PlayerController` uses `GetComponent<Rigidbody>()` — no Input System or bootstrap references.

## Phase 1 tuning (initial)

Edit tuning on `PlayerController` in the test scene:

| Field | Value | Notes |
|-------|-------|-------|
| `maxSpeed` | 6 | Tune in play mode |
| `maxAcceleration` | 50 | Tune in play mode |
| `moveInputDeadzone` | 0.1 | |

## Input

New Input System via `PlayerInputActions` asset. Only `PlayerInputReader` references Input System types.

- Keyboard: WASD / arrows for Move, Space for Jump (dev and desktop testing)
- Mobile: touch virtual stick will call `PlayerInputReader.SetTouchMove` (not yet implemented)
- `PlayerController` reads `IPlayerInput.Move`; camera-relative conversion stays on controller

Jump action is wired in the input asset but not yet consumed by movement.

## Test scene

`Assets/_Project/Scenes/PlayerMovementTest.unity` — capsule player on a ground plane, angled camera, `GameBootstrap` object wires input.

Alternative: **MarioTest → Create Player Movement Test Scene** (regenerates scene via editor menu).

## Deferred (not Phase 1)

- Custom gravity, jump Y velocity, variable height, coyote time, jump buffer
- `GroundDetector`, slope projection, moving platform velocity transfer
- Knockback vector + separate decay
- Ice via `PhysicMaterial`, crate push (physics-only, no special motor code)

## Known risks

1. **Moving platforms** — friction alone insufficient for kinematic movers; explicit platform velocity transfer needed.
2. **Mixed force + direct Y** — requires `useGravity = false` and careful jump frame handling.
3. **Knockback decay** — motor steering partially decays knockback; dedicated decay vector recommended in Phase 4.
