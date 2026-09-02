# Player

Agent summary. **Detail:** [docs/features/player-movement.md](../../../../docs/features/player-movement.md) · [docs/features/player-jump.md](../../../../docs/features/player-jump.md) · [docs/README.md](../../../../docs/README.md)

## Status

| Area | State | Doc |
|------|-------|-----|
| Horizontal + gravity + ground | Done | [docs/features/player-movement.md](../../../../docs/features/player-movement.md) |
| Jump | Done | [docs/features/player-jump.md](../../../../docs/features/player-jump.md) |

## Architecture

| Component | Responsibility |
|-----------|----------------|
| `PlayerController` | `Update`: input. `FixedUpdate`: detect → movement |
| `PlayerHealth` | Applies damage in `TakeHit()`; raises `Hit` / `Died` |
| `PlayerMovement` | Plain C#: jump, gravity, horizontal |
| `GroundDetector` | Plain C#: spherecast → `IsGrounded`, `GroundNormal` |
| `IPlayerInput` | Portable read API (`Move`, jump) for `PlayerController` |
| `PlayerInputReader` | Input System + touch merge; `Tick()` then read via `IPlayerInput` |
| `PlayerTuning` (ScriptableObject) / `PlayerMovementSettings` / `GroundDetectionSettings` | Tunables — SO asset + settings on controller |
| `GameBootstrap` | Wires `Initialize(IPlayerInput)` |

**Decisions agents must not violate:**

- Coyote + buffer on **player side**, not `GroundDetector`
- **Stomp “hold = higher bounce”** uses the same `ApplyGravity` branch as variable jump (`vy > 0` + `_jumpHeld`) — not a separate bounce API; see [docs/features/player-jump.md#stomp-bounce](../../../../docs/features/player-jump.md#stomp-bounce)
- `Update` = intent; `FixedUpdate` = physics
- Motor sets **velocity directly** (horizontal + Y for jump/gravity) — no `AddForce` for run; no hard clamp on total horizontal speed
- Zero-friction capsule material — no wall stick; ice/surfaces tune acceleration multiplier, not friction
- Tuning defaults live on `Assets/ScriptableObjects/PlayerTuning.asset`; feature detail in `docs/features/`

**Wiring:** `GameBootstrap` owns Input Actions + reader lifecycle. `PlayerController` uses `GetComponent` for `Rigidbody` + `CapsuleCollider`.

## Test scene

`Assets/_Project/Scenes/PlayerMovementTest.unity` — **MarioTest → Create Player Movement Test Scene** to regenerate.

## Known risks

1. Coyote must not re-arm after intentional jump
2. Single jump path for buffer + coyote overlap
3. Moving platforms need explicit velocity transfer later
4. Knockback decay vector later — motor steering is partial decay today
