# Player movement

Horizontal run, custom gravity, and ground detection. Part of assignment build order **#1 Movement** (jump is separate).

**Status:** Implemented  
**Code:** `Assets/_Project/Scripts/Player/`

---

## Horizontal movement

Acceleration servo toward camera-relative target velocity. Motor integrates horizontal velocity directly each step — no `AddForce`, no hard speed cap (knockback can exceed max run speed).

```
targetVelocity = moveDirection * maxSpeed
currentHorizontal = velocity with Y = 0
newHorizontal = MoveTowards(current, target, maxAcceleration * surfaceMultiplier * dt)
set velocity.x/z = newHorizontal
```

Tunables: `PlayerTuning` (speed, acceleration, deadzone). Surface accel: `PlayerMovementSettings` layer map on `PlayerController`.

---

## Gravity

`useGravity` is off. Movement sets **Y velocity directly** each physics step.

```
if grounded and falling: vy = 0
else if rising: vy += rise gravity * dt
else: vy += fall gravity * dt
clamp to terminal fall speed
```

Runs inside `ApplyMovement` after ground detection. Tunables: rise/fall gravity, max fall speed.

---

## Ground detection

`GroundDetector.Detect` once per `FixedUpdate` before movement.

- Sphere cast from capsule feet downward
- Ground layer mask only; triggers ignored
- Rejects slopes steeper than max angle
- Exposes `IsGrounded`, `GroundNormal` — physics truth only

Tunables: `GroundDetectionSettings` on `PlayerController`.

**Debug:** enable **Debug Ground** on `PlayerController` — Scene view green/red (grounded/airborne), blue = normal.

Coyote time and jump buffer are **not** here — see [player-jump.md](player-jump.md).

---

## Update / FixedUpdate split

| Loop | Work |
|------|------|
| `Update` | Read move input, camera-relative direction |
| `FixedUpdate` | Detect ground → `ApplyMovement` |

---

## Test scene

`Assets/_Project/Scenes/PlayerMovementTest.unity` — ground, elevated platforms.

Regenerate: **MarioTest → Create Player Movement Test Scene**.

### Quick tests

1. WASD on flat ground — responsive run, no sinking
2. Walk off `Platform_Ledge` — falls, lands on main ground
3. Debug Ground — green on floor, red in air

---

## Out of scope (this feature)

- Jump — [player-jump.md](player-jump.md)
- Slope-projected movement
- Moving platform velocity transfer
- Knockback decay vector
