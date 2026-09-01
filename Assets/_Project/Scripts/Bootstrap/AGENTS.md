# Bootstrap

Agent summary. Input/player wiring is covered in [docs/features/player-movement.md](../../../../docs/features/player-movement.md) (architecture section). No bootstrap-specific feature doc yet.

## Status

In use for input composition.

## GameBootstrap

Composition root for scene-level wiring. `Update` merges input (`Tick`) after `MobileTouchInput` (-50), before `PlayerController` (0).

**Responsibilities:**
- Hold `InputActionAsset` reference (only bootstrap knows about Input System asset)
- Create `PlayerInputReader` and pass to `PlayerController.Initialize(IPlayerInput)`
- Enable / disable input actions in `OnEnable` / `OnDisable`

**Inspector wiring (minimal):**
- `_inputActions` — PlayerInputActions asset
- `_playerController` — player in scene

`PlayerController` has no knowledge of bootstrap or Input System. Same-GameObject deps (`Rigidbody`) use `GetComponent` in `Awake`.

## Future

Extend bootstrap for remote config fetch, game state. Touch UI wired via `MobileTouchInput` → `PlayerInputReader`.
