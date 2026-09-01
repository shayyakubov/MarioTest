# Player Movement

## Status

Phase 2 in progress: horizontal movement, ground detection, custom gravity. Jump deferred.

## Model

Dynamic `Rigidbody` player. Horizontal movement uses an acceleration servo; vertical uses custom gravity (Y velocity), jump deferred.

### Horizontal movement

```
targetVelocity = moveDirection * maxSpeed
currentHorizontal = rigidbody.linearVelocity with Y = 0
deltaVelocity = targetVelocity - currentHorizontal
accelerationNeeded = deltaVelocity / fixedDeltaTime
clamp acceleration magnitude to max allowed
AddForce(accelerationNeeded, ForceMode.Acceleration)
```

- Do **not** hard-clamp total horizontal velocity (knockback may exceed max speed).
- Acceleration clamp uses **vector magnitude**, not per-axis.
- Move direction is camera-relative, normalized; input magnitude applied before normalize (clamped to 1).

### Gravity

Runs in `ApplyMovement` after ground detection. Sets **Y velocity directly**; horizontal still uses forces.

```
if grounded and falling: vy = 0
else if rising: vy += rise gravity * dt
else: vy += fall gravity * dt
clamp fall to terminal speed
```

Grounded stick prevents sinking. Walk off a ledge → airborne → fall accelerates to terminal speed.

### Rigidbody settings

| Setting | Value |
|---------|-------|
| Kinematic | false |
| Use gravity | false (custom gravity on movement) |
| Linear / angular damping | 0 |
| Constraints | freeze rotation X and Z |
| Collision detection | Continuous Dynamic |
| Interpolation | Interpolate |
| Collider | Capsule |

## Architecture

| Component | Responsibility |
|-----------|----------------|
| `PlayerController` | MonoBehaviour: input, tuning, ground detect tick, movement tick |
| `IPlayerInput` | Portable input interface (`Move`, `IsJumpPressed`) |
| `PlayerInputReader` | Only class that talks to Input System |
| `PlayerMovement` | Horizontal acceleration servo + custom gravity |
| `GroundDetector` | One non-alloc spherecast per fixed step → `IsGrounded`, `GroundNormal` |
| `PlayerTuning` | Movement tunables — serialized on `PlayerController` |
| `GroundDetectionSettings` | Ground probe tunables — serialized on `PlayerController` |
| `GameBootstrap` | Wires input → `PlayerController.Initialize` |

**Wiring:** `GameBootstrap` owns Input Actions asset and reader lifecycle. `PlayerController` has no Input System or bootstrap knowledge. Same-GameObject `Rigidbody` + `CapsuleCollider` via `GetComponent`.

## Tuning

**Source of truth:** defaults in `PlayerTuning` and `GroundDetectionSettings` C# classes; live values on `PlayerController` in the test scene. Tune in play mode — do not duplicate numbers here.

| Asset / class | What it controls |
|---------------|------------------|
| `PlayerTuning` | Speed, acceleration, deadzone, rise/fall gravity, terminal fall speed |
| `GroundDetectionSettings` | Probe distance, skin width, max walkable slope |

Shipped/final numbers go in `README.md` at deliverable time.

## Input

New Input System via `PlayerInputActions` asset. Only `PlayerInputReader` references Input System types.

- Keyboard: WASD / arrows + Space (dev testing)
- Mobile: touch stick → `SetTouchMove` (not yet implemented)
- Jump wired in asset but not consumed by movement yet

## Ground detection

`GroundDetector.Detect` runs in `FixedUpdate` before movement.

- One `SphereCastNonAlloc` per step, pre-allocated hit buffer
- Cast from capsule feet downward; Ground layer mask only; triggers ignored
- Rejects hits steeper than max slope angle
- `IsGrounded` drives gravity stick; will drive jump/coyote next

All walkable geometry must be on the **Ground** layer (`PhysicsLayers`).

### How to test ground + gravity

1. Open `PlayerMovementTest.unity` (or regenerate via **MarioTest → Create Player Movement Test Scene**).
2. Enable **Debug Ground** on `PlayerController` — Scene view: green/red = grounded/airborne, blue = ground normal.
3. Stand on floor → green, no sinking.
4. Walk off a ledge → red while falling, green on landing.

## Test scene

`Assets/_Project/Scenes/PlayerMovementTest.unity` — ground plane, elevated platforms, `GameBootstrap` wires input.

## Deferred

- Jump, variable height, coyote time, jump buffer
- Slope projection for movement, moving platform velocity transfer
- Knockback decay vector
- Ice / crate (physics-only, no special motor code)

## Known risks

1. **Moving platforms** — need explicit platform velocity transfer, not friction alone.
2. **Mixed force + direct Y** — only set Y velocity on jump/gravity frames, not horizontal.
3. **Knockback decay** — motor steering partially decays knockback; dedicated vector recommended later.
