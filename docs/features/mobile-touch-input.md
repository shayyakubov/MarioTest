# Mobile touch input

Floating left joystick and right-screen jump for assignment build order **#2 Touch input**.

**Status:** Implemented  
**Code:** `Assets/_Project/Scripts/Input/`

---

## Screen layout

| Region | Input |
|--------|--------|
| Left half (`leftScreenFraction`, default 0.5) | Floating virtual joystick — appears at first touch, drag steers |
| Right half | Jump — touch = press, hold = hold, release = release |

Both regions work **simultaneously** (separate finger tracking).

Left/right split uses **`Screen.safeArea`** so notches and home bars don't skew regions on phones.

---

## Analog stick

Joystick deflection is clamped to a **screen-relative radius** and sent as a **0–1 magnitude** vector:

```
radius = min(safeArea.width, safeArea.height) * joystickRadiusScreenFraction
move = direction * (deflection / radius)
```

`PlayerController` already uses `Move` vector magnitude for `maxSpeed` scaling — partial stick = slower run.

---

## Different screen sizes / mobile

| Layer | What scales |
|-------|-------------|
| **Touch logic** | `joystickRadiusScreenFraction` (% of min safe-area side), `leftScreenFraction`, `Screen.safeArea` split |
| **Joystick visuals** | `CanvasScaler` on `TouchInputCanvas` — Scale With Screen Size, ref 1920×1080 |
| **Gameplay** | Unaffected — motor uses normalized input, not pixels |

### Canvas (already on `TouchInputCanvas` prefab / scene)

- **Render Mode:** Screen Space Overlay
- **Canvas Scaler:** Scale With Screen Size
- **Reference Resolution:** 1920 × 1080
- **Match:** 0.5 (blend width/height)

### Build / player settings (when shipping APK)

- **Default orientation:** Landscape (typical for this genre) or portrait — split still works
- **Resolution:** default; input uses `Screen.safeArea` + touch positions in pixels
- Test on at least one narrow phone and one tablet

### Editor vs device

- **Editor:** left mouse in Game view (direct mouse path)
- **Device:** `Touch.activeTouches` (real fingers)

---

## Wiring

```
MobileTouchInput  →  PlayerInputReader (touch methods)
GameBootstrap     →  creates reader, passes to PlayerController + MobileTouchInput
```

Keyboard / gamepad still work via Input Actions when touch is inactive.

---

## Tunables

`TouchInputSettings` on `MobileTouchInput`:

- `leftScreenFraction` — safe-area split (default 0.5)
- `joystickRadiusScreenFraction` — max drag as fraction of min safe side (default 0.1)
- `joystickDeadzone` — inner deadzone before move registers (default 0.1)

---

## Test

- Editor: click **left half** of Game view for joystick, **right half** for jump
- Device: build to Android; test two thumbs + variable jump on release
- Resize Game view aspect ratio in editor to sanity-check scaling
