# Camera follow

Third-person follow camera with damping and look-ahead. Assignment build order **#3 Camera**.

**Status:** Implemented  
**Code:** `Assets/_Project/Scripts/Camera/`

---

## Assignment requirements (brief)

From the NumTalk brief / [project-context.mdc](../../.cursor/rules/project-context.mdc):

- **Third-person follow** — camera tracks the player from behind/above
- **Damping** — smooth motion, not rigid parenting or per-frame snap
- **Lead** — offset toward where the player is heading so the course ahead is visible
- **No fight mid-jump** — camera must not jerk, zoom, or chase vertical motion in a way that competes with jump readability on mobile

Related (already implemented elsewhere):

- **Camera-relative run** — [player-movement.md](player-movement.md); `PlayerController` reads `_cameraTransform.forward/right` (Y zeroed)
- **Mobile touch** — [mobile-touch-input.md](mobile-touch-input.md); stick is screen-space, movement is world-space via camera

Hard constraints that affect camera:

- No third-party camera assets / Cinemachine for this prototype
- Frame-rate independent (`Time.deltaTime` / `SmoothDamp`, not frame-count lerp)
- Primitives-only scene; one main playable scene

---

## Current state

| Item | Today |
|------|--------|
| Main Camera | `FollowCameraController` on Main Camera |
| World forward | Dynamic state, default **+Z** (`_initialWorldForward`) |
| `PlayerController._cameraTransform` | Main Camera — camera-relative input |
| Zone blending | API on controller (`BlendWorldForward` / `SetWorldForward`); triggers not built yet |

---

## World forward model

Course-facing axis is **state**, not velocity:

```
worldForward = authored axis (default +Z)
pivot = player.position + up * pivotHeight
lead = dot(velocity, worldForward) * worldForward   // forward/back only
lookTarget = pivot + lead
camera behind player along -worldForward (+ height/pitch offset)
rotation = LookAt(lookTarget)
```

**Why:** Camera-relative input needs a **stable** forward. Velocity yaw caused strafe spin.

**Future:** `CameraZone` trigger calls `FollowCameraController.BlendWorldForward(newForward, duration)` — e.g. +Z → +X at a corner, blend back at return checkpoint.

---

## Follow model (position)
offset = Quaternion.Euler(pitch, yaw, 0) * Vector3(0, height, -distance)
desiredPosition = pivot + offset
```

Start simple: **fixed pitch + yaw follows horizontal velocity** (or player forward when speed is low).

### Damping (horizontal)

Use `Vector3.SmoothDamp` (or separate XZ SmoothDamp) toward `desiredPosition`:

```
smoothedPosition = SmoothDamp(current, desired, ref velocity, smoothTimeHorizontal, maxSpeed, dt)
```

`smoothTimeHorizontal` is a named tunable — smaller = snappier.

### Lead (look-ahead)

Shift the follow **pivot** or **desired position** in the player’s horizontal velocity direction:

```
leadOffset = horizontalVelocity.normalized * leadDistance * saturate(speed / maxSpeed)
desiredPosition += leadOffset
```

When idle or below deadzone, lead collapses to zero so the camera settles behind the player.

Tunables: `leadDistance`, optional `leadMinSpeed` (below this, no lead).

---

## No fight mid-jump

**Problem:** If the camera tightly follows the player’s Y every frame, short hops cause constant pitch/height bob — feels seasick on mobile and fights jump tuning.

**Approach (recommended): split horizontal and vertical follow**

| Axis | Behavior |
|------|----------|
| **XZ** | Normal damping + lead toward player horizontal motion |
| **Y** | Slower smooth time **or** follow a **soft vertical target** that lags jumps |

```
desiredY = playerY + heightOffset
smoothedY = SmoothDamp(currentY, desiredY, ref yVel, smoothTimeVertical, ..., dt)
```

Where `smoothTimeVertical` > `smoothTimeHorizontal` (e.g. 2–3×) so the camera eases over jump arcs instead of snapping.

**Optional refinement:** while player `vy > 0` (rising), cap how fast camera Y can increase per second (`maxRiseFollowSpeed`).

**Do not:** parent camera to player; scale FOV on jump; hard-lock camera Y to player feet every frame.

---

## Timing

| Loop | Camera work |
|------|-------------|
| `FixedUpdate` | Player physics + ground + movement |
| `Update` | Input + `PlayerInputReader.Tick` |
| **`LateUpdate`** | Camera follow (after movement, use interpolated `Rigidbody` position if needed) |

Camera runs **after** player motion for the frame. Read `target.position` (interpolated rigidbody is fine with `RigidbodyInterpolation.Interpolate`).

---

## Tunables (`CameraTuning`)

Describe what to tune; defaults live in code / scene when implemented.

| Field | Purpose |
|-------|---------|
| `distance` | Behind-player offset along view back axis |
| `height` | Above-pivot offset |
| `pitch` | Downward look angle (degrees) |
| `pivotHeight` | Vertical point on player (chest vs feet) |
| `smoothTimeHorizontal` | XZ damping |
| `smoothTimeVertical` | Y damping (slower = less jump fight) |
| `leadDistance` | Max horizontal look-ahead |
| `leadMinSpeed` | Speed below which lead fades out |
| `maxCameraSpeed` | SmoothDamp cap (optional safety) |

---

## Integration checklist

1. Add `FollowCameraController` to Main Camera
2. Assign Player transform as `target`
3. Keep `PlayerController._cameraTransform` → Main Camera
4. Replace static `SetupCamera` pose in scene builder with follow component + reasonable defaults
5. Playtest: run, full jump, short hop, walk off ledge, change direction while airborne

---

## Test plan

### Feel

1. **Flat run** — camera lags slightly behind, catches up smoothly; no jitter at 60 fps
2. **Direction change** — lead shifts; no snap rotation
3. **Full jump** — player stays in frame; camera does not bob aggressively
4. **Short hop** — minimal camera Y chase; jump readable
5. **Fall off ledge** — camera eventually follows downward (slower Y), does not lose player

### Input

6. Touch stick left / keyboard WASD — move direction still matches camera facing (camera-relative)
7. Jump while holding run — both thumbs; no camera spin

### Edge cases

8. Stand still — camera stops leading, stable framing
9. Hit wall while pushing into it — no camera explosion (velocity near zero)
10. Resize Game view / phone aspect — framing acceptable (no code dependency on fixed pixel sizes)

---

## Out of scope

- Wall collision / occlusion
- Slope-aligned camera (see DESIGN_NOTES — slope movement deferred)
- Checkpoint respawn camera cut
- Sudden-event camera punch (build order **#8**)

---

## Links

- Player input frame: [player-movement.md](player-movement.md) (`GetCameraRelativeDirection`)
- Build order: [project-context.mdc](../../.cursor/rules/project-context.mdc)
