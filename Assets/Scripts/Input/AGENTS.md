# Input

Agent summary. **Detail:** [docs/features/mobile-touch-input.md](../../../../docs/features/mobile-touch-input.md)

## Status

| Area | State | Doc |
|------|-------|-----|
| Keyboard / Input Actions | Done | [player-movement.md](../../../../docs/features/player-movement.md) |
| Mobile touch (joystick + jump) | Done | [mobile-touch-input.md](../../../../docs/features/mobile-touch-input.md) |

## Architecture

| Component | Responsibility |
|-----------|----------------|
| `PlayerInputReader` | Merges keyboard + touch in `Tick()`; touch methods called by `MobileTouchInput` |
| `MobileTouchInput` | Enhanced Touch / editor mouse → `PlayerInputReader` |
| `TouchInputSettings` | Screen split, joystick radius, deadzone |
| `GameBootstrap` | Creates reader; wires `MobileTouchInput.Initialize(reader)` |

Touch overrides keyboard for move/jump only while active.

## Test scene

`TouchInputCanvas` on `PlayerMovementTest.unity` — regenerate via **MarioTest → Create Player Movement Test Scene**.
