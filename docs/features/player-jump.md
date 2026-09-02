# Player jump

Variable-height jump with coyote time and jump buffer. Completes assignment build order **#1 Movement** (with [player-movement.md](player-movement.md)).

**Status:** Implemented  
**Code:** `Assets/_Project/Scripts/Player/`

**Variable height:** while rising, use `lowJumpGravity` when jump not held; `riseGravity` when held. **Stomp bounce** uses the same rule — see [Stomp bounce](#stomp-bounce) below.

---

## Assignment requirements

- Variable jump height (hold = higher, release early = shorter)
- Coyote time — grace after leaving ground while falling
- Jump buffer — early press remembered before landing
- Named tunables for all three
- Frame-rate independent
- Later (#7 Course): publish max jump height and horizontal distance for gap sizing

Keyboard Space for dev; touch jump comes in build order **#2**.

---

## Architecture principle

**Coyote and buffer live on the player side** (`PlayerController` / `PlayerMovement`), not in `GroundDetector`.

| Component | Jump responsibility |
|-----------|---------------------|
| `GroundDetector` | Reports `IsGrounded` this physics step — nothing else |
| `PlayerController` | Sample jump input in `Update`; track coyote/buffer timers |
| `PlayerMovement` | Execute jump impulse, variable height cut, gravity order |
| `PlayerTuning` | Named tunables |

---

## Input timing

| Phase | Work |
|-------|------|
| `Update` | Jump pressed / held / released; decrement coyote & buffer timers; move input |
| `FixedUpdate` | Detect → resolve jump → `ApplyMovement` |

Intent at display rate; impulse at physics rate (same as movement).

**Likely `IPlayerInput` extension:**

- Jump pressed this frame
- Jump held
- Jump released this frame

`PlayerInputReader` implements; `PlayerController` stays Input-System-free.

---

## When is jump allowed?

Player-side `canJump` each step:

```
canJump = isGrounded
       OR (coyoteTimer > 0 AND falling)
```

**Coyote time**

- When `IsGrounded` goes true → false: start `coyoteTimer = coyoteTime`
- While timer > 0 and `vy <= 0`: jump allowed even though airborne
- On jump execute: clear coyote — do not re-arm mid-air after intentional jump
- Do not extend coyote while rising

**Jump buffer**

- On jump pressed: `bufferTimer = jumpBuffer`
- Decrement each `Update`
- When `canJump` and `bufferTimer > 0`: execute jump, clear buffer

Single `TryJump` path so coyote + buffer cannot double-fire.

---

## Jump execution

On jump frame:

```
vy = jumpVelocity    // direct set, not AddForce
clear coyote + buffer
```

Only set Y velocity on jump frames — horizontal stays force-based.

### Variable height

While rising: `riseGravity` when jump held, `lowJumpGravity` when released (more negative = shorter hop).

This applies to **every** upward arc where `vy > 0`, including stomp bounces — not only jumps from `TryExecuteJump`.

### `ApplyMovement` order (per fixed step)

```
1. Resolve jump (buffer + coyote + grounded)
2. If jump: set vy
3. Gravity
4. Horizontal
```

Document exact jump-vs-gravity order when implementing (jump set before gravity on same step is typical).

---

## Stomp bounce

**Status:** Implemented  
**Code:** `StompableEnemy.cs` → `IBounceReceiver.ApplyBounce()` → `PlayerMovement`

Assignment: land on an enemy from above → enemy dies, player bounces; **holding jump = higher bounce, release early = short hop**.

### Why it looks like one feature but lives in two places

Stomp does **not** call a separate “high bounce” API. It only sets an initial upward speed; **variable height is entirely gravity**, same as a normal jump.

| Step | What happens |
|------|----------------|
| 1. Contact | `StompableEnemy` decides stomp vs side hit (fall speed, feet above enemy midline, top tolerance) |
| 2. Stomp | `ApplyBounce()` sets pending `_bounceVelocity = StompVelocity` (tunable on `PlayerTuning`) |
| 3. Same `FixedUpdate` | `ApplyMovement` applies bounce **before** gravity: `vy = max(vy, StompVelocity)` |
| 4. Same `FixedUpdate` | `ApplyGravity`: if `vy > 0`, use `riseGravity` when `_jumpHeld`, else `lowJumpGravity` |
| 5. Following frames | Player keeps holding or releases jump → arc height follows normal variable-jump rules |

So “higher bounce when holding jump” is **not** a second stomp velocity — it is `ApplyGravity` cutting the arc less aggressively while the thumb stays down.

```
Stomp contact
    → ApplyBounce()  (_bounceVelocity = StompVelocity)
    → ApplyMovement: vy ← max(vy, StompVelocity); clear coyote/buffer/latch
    → ApplyGravity:  vy > 0 ? (jumpHeld ? riseGravity : lowJumpGravity) : fallGravity
    → next frames…   same gravity branch until vy ≤ 0
```

### Stomp vs jump on the bounce frame

On the bounce frame, `_bounceVelocity` wins over `TryExecuteJump` — you cannot also fire a normal jump from buffer/coyote that same step. Coyote, buffer, and jump latch are cleared when bounce applies.

Knockback horizontal decay still runs after gravity on that step.

### Tuning (names only — values in `PlayerTuning` / prefab)

| Tunable | Stomp role |
|---------|------------|
| `StompVelocity` | Initial upward speed after stomp (like `JumpVelocity` for a normal jump) |
| `RiseGravity` | Held jump after stomp — gentler cut, taller arc |
| `LowJumpGravity` | Released jump after stomp — sharp cut, short hop |
| `FallGravity` | Once `vy ≤ 0` after bounce |

`StompVelocity` and `JumpVelocity` may differ or match by design; height difference for hold vs release always comes from the gravity pair above.

### Testing stomp height

1. Stomp enemy while **holding** jump through the arc — should reach roughly full stomp height.
2. Stomp and **release immediately** — clearly shorter hop off the enemy.
3. Side graze at speed — should hit, not bounce (see `StompableEnemy` bounds checks).
4. Full fall speed + 30 fps — stomp discrimination must still register as stomp, not side hit.

Enemy-side detail: `StompableEnemy.cs` (`IsStomp`, `_minFallSpeed`, `_stompTopTolerance`).

---

## Tunables (add to `PlayerTuning`)

| Tunable | Purpose |
|---------|---------|
| Jump velocity | Initial upward speed |
| Coyote time | Fall grace after leaving ground |
| Jump buffer | Early press memory |
| Variable-height helper | e.g. jump cut gravity or release multiplier |

Defaults in `PlayerTuning.cs` — not duplicated here.

---

## Measurement (for course / README)

Record after tuning feels good:

1. **Max jump height** — stand still, full-hold jump, note peak Y delta
2. **Max jump distance** — run at max speed off ledge, jump on takeoff, measure horizontal travel to landing
3. **Coyote distance** — same but jump at end of coyote window

Final numbers go in `README.md` at deliverable time.

---

## Testing plan

1. **Flat ground** — jump; early release = shorter hop
2. **Platform_Ledge** — walk off, jump inside coyote window
3. **Approach ledge** — jump slightly before land (buffer)
4. **Walk off, no jump** — no jump after coyote expires
5. **Debug Ground** — red during coyote jump is expected
6. **Publish** — max height and max horizontal distance at run speed (for course / README)
7. **Stomp** — hold vs release after enemy stomp changes bounce height (same gravity as #1)

---

## Out of scope (this slice)

- Touch UI jump button (#2)
- Slope takeoff / moving platforms
- Knockback + jump composition
- Tuning ScriptableObject (#8)

(Stomp bounce is in scope — documented in [Stomp bounce](#stomp-bounce).)
