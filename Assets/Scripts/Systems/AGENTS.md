# Systems — agent context

## Status

**Lives / fail / checkpoint** — implemented.

## Key types

| Type | Role |
|------|------|
| `GameSession` | Run coordinator; refs injected by `GameBootstrap`; respawn, scene reload, course win |
| `CheckpointsManager` | Active checkpoint + spawn lookup; subscribes to serialized `CheckpointTrigger` list |
| `PickupsManager` | Subscribes to serialized `CoinPickup` list; fires `CoinCollected` on collect |
| `PlayerRespawn` | Plain C#: `IWorldRestorable` reset (crumble + pushables) + teleport/camera snap |
| `PlayerHealth` | Applies damage in `TakeHit()`; `Hit` / `Died` events (on player) |
| `CheckpointTrigger` | Volume fires `Activated`; spawn point on trigger or `_checkpointTransform` |
| `CoinPickup` | Trigger fires `Collected` when player enters |
| `GoalTrigger` | Volume → `CourseReached` event (no session/UI refs) |
| `KillZoneTrigger` | Death plane / pit trigger → `ILifeTarget.TakeHit()` |

## Hit flow

1. Fall (death plane) → `PlayerHealth.TakeHit()` → respawn at checkpoint
2. Enemy side hit → `TakeHit(respawnAtCheckpoint: false)` → lose life, keep playing
2. Still alive → `Hit` → `GameSession` respawn
3. 0 lives → `Died` → `GameSession` disables input; `GameHud` shows overlay (HUD bound in `GameBootstrap`)
4. Goal → `GoalTrigger.CourseReached` → `GameSession` disables input; `GameHud` shows course-win overlay

## Tune in Inspector

- `PlayerHealth`: `_startingLives`
- `GameSession`: `_respawnDelay`
- `GameBootstrap`: player, session, HUD, camera, goal refs
- Death plane: `BoxCollider` trigger + `KillZoneTrigger` (see `SceneSetupUtility.CreateDeathPlane`)
- `CheckpointsManager`: `_defaultCheckpoint`, `_playerFallback`
