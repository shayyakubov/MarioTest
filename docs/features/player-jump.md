# Player jump

Variable-height jump with coyote time and jump buffer. Completes assignment build order **#1 Movement** (with [player-movement.md](player-movement.md)).

**Status:** Implemented  
**Code:** `Assets/_Project/Scripts/Player/`

**Variable height:** while rising, use `lowJumpGravity` when jump not held; `riseGravity` when held.

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

---

## Out of scope (this slice)

- Touch UI jump button (#2)
- Slope takeoff / moving platforms
- Knockback + jump composition
- Tuning ScriptableObject (#8)
