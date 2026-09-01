# Player jump

Variable-height jump with coyote time and jump buffer. Completes assignment build order **#1 Movement** (with [player-movement.md](player-movement.md)).

**Status:** Designed — not implemented  
**Code:** `Assets/_Project/Scripts/Player/` (when built)

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

While rising and jump not held: apply cut (e.g. stronger rise gravity or velocity multiplier). Pick one approach at implementation; tune with named field.

### `ApplyMovement` order (per fixed step)

```
1. Resolve jump (buffer + coyote + grounded)
2. If jump: set vy
3. Gravity
4. Horizontal
```

Document exact jump-vs-gravity order when implementing (jump set before gravity on same step is typical).

---

## Tunables (add to `PlayerTuning`)

| Tunable | Purpose |
|---------|---------|
| Jump velocity | Initial upward speed |
| Coyote time | Fall grace after leaving ground |
| Jump buffer | Early press memory |
| Variable-height helper | e.g. jump cut gravity or release multiplier |

Defaults in code — not duplicated here.

---

## Testing plan

1. **Flat ground** — jump; early release = shorter hop
2. **Platform_Ledge** — walk off, jump inside coyote window
3. **Approach ledge** — jump slightly before land (buffer)
4. **Walk off, no jump** — no jump after coyote expires
5. **Debug Ground** — red during coyote jump is expected
6. **Publish** — max height and max horizontal distance at run speed (for course / README)

---

## Out of scope (this slice)

- Touch UI jump button (#2)
- Slope takeoff / moving platforms
- Knockback + jump composition
- Tuning ScriptableObject (#8)
