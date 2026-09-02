# Bootstrap

Agent summary. Input/player wiring is covered in [docs/features/player-movement.md](../../../../docs/features/player-movement.md) (architecture section). No bootstrap-specific feature doc yet.

## Status

Composition root for input and session startup.

## GameBootstrap

Runs at `-10`. `Update` merges input (`Tick`) after `MobileTouchInput` (-50), before `PlayerController` (0).

**Responsibilities:**
- Hold scene references and inject them at startup
- Create `PlayerInputReader` → `PlayerController.Initialize` + `MobileTouchInput.Initialize`
- Resolve player components and inject into `GameSession.Initialize`
- Enable / disable input actions in `OnEnable` / `OnDisable`

**Inspector wiring:**
- `_inputActions` — PlayerInputActions asset
- `_playerController` — player in scene
- `_mobileTouchInput` — touch canvas (optional)
- `_gameSession` — run coordinator in scene
- `_gameHud` — lives + overlays
- `_followCamera` — main follow camera
- `_goalTrigger` — course goal volume

## Future

Extend bootstrap for remote config fetch. Touch UI wired via `MobileTouchInput` → `PlayerInputReader`.
