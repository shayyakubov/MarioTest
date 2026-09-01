# Player

## Status

| Area | State | Doc |
|------|-------|-----|
| Horizontal + gravity + ground | Done | [docs/features/player-movement.md](../../../../docs/features/player-movement.md) |
| Jump | Designed, not built | [docs/features/player-jump.md](../../../../docs/features/player-jump.md) |

## Architecture

| Component | Responsibility |
|-----------|----------------|
| `PlayerController` | `Update`: input. `FixedUpdate`: detect → movement |
| `PlayerMovement` | Plain C#: gravity, horizontal (+ jump TBD) |
| `GroundDetector` | Plain C#: spherecast → `IsGrounded`, `GroundNormal` |
| `IPlayerInput` | Portable input (`Move`, `IsJumpPressed`; jump edges TBD) |
| `PlayerInputReader` | Input System only |
| `PlayerTuning` / `GroundDetectionSettings` | Tunables on controller |
| `GameBootstrap` | Wires `Initialize(IPlayerInput)` |

**Decisions agents must not violate:**

- Coyote + buffer on **player side**, not `GroundDetector`
- `Update` = intent; `FixedUpdate` = physics
- Y velocity set directly for jump/gravity; horizontal uses forces
- No hard clamp on total horizontal speed
- Tuning defaults in code; feature detail in `docs/features/`

**Wiring:** `GameBootstrap` owns Input Actions + reader lifecycle. `PlayerController` uses `GetComponent` for `Rigidbody` + `CapsuleCollider`.

## Test scene

`Assets/_Project/Scenes/PlayerMovementTest.unity` — **MarioTest → Create Player Movement Test Scene** to regenerate.

## Known risks

1. Coyote must not re-arm after intentional jump
2. Single jump path for buffer + coyote overlap
3. Moving platforms need explicit velocity transfer later
4. Knockback decay vector later — motor steering is partial decay today
