# Bootstrap

## Status

In use for input composition.

## GameBootstrap

Composition root for scene-level wiring. Runs early (`DefaultExecutionOrder(-100)`).

**Responsibilities:**
- Hold `InputActionAsset` reference (only bootstrap knows about Input System asset)
- Create `PlayerInputReader` and pass to `PlayerController.Initialize(IPlayerInput)`
- Enable / disable input actions in `OnEnable` / `OnDisable`

**Inspector wiring (minimal):**
- `_inputActions` — PlayerInputActions asset
- `_playerController` — player in scene

`PlayerController` has no knowledge of bootstrap or Input System. Same-GameObject deps (`Rigidbody`) use `GetComponent` in `Awake`.

## Future

Extend bootstrap for remote config fetch, UI wiring, game state, touch UI feeding `PlayerInputReader.SetTouchMove`.
