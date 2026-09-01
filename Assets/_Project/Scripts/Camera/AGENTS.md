# Camera

Agent summary. **Detail:** [docs/features/camera-follow.md](../../../../docs/features/camera-follow.md)

## Status

| Area | State | Doc |
|------|-------|-----|
| Third-person follow (world forward, damping, lead) | Implemented | [docs/features/camera-follow.md](../../../../docs/features/camera-follow.md) |
| Zone triggers to blend world forward | Not started | [docs/features/camera-follow.md](../../../../docs/features/camera-follow.md) |

## Architecture

| Component | Responsibility |
|-----------|----------------|
| `FollowCameraController` | MonoBehaviour: target ref, `LateUpdate`, world-forward API |
| `FollowCamera` | Plain C#: rig offset from world forward + split XZ/Y SmoothDamp |
| `CameraWorldForward` | Plain C#: current course-facing axis; snap / blend for future zones |
| `CameraTuning` | Named tunables serialized on controller |

**World forward:** Default `Vector3.forward` (+Z). Camera trails behind player along `-worldForward`. No velocity yaw — stable input frame for camera-relative controls.

**Future zones:** Call `BlendWorldForward(Vector3.right, duration)` from triggers; `SetWorldForward` to snap back.

**Integration:** Main Camera transform stays `PlayerController._cameraTransform` for camera-relative input.

## Decisions agents must not violate

- **LateUpdate** for follow (after player movement)
- **No velocity-based yaw** — world forward is authored/stateful, not derived from stick/velocity
- **No Cinemachine** / third-party camera kits
- **No fight mid-jump** — `smoothTimeVertical` slower than `smoothTimeHorizontal`
- Frame-rate independent smoothing (`SmoothDamp` + `deltaTime`)

## Test scene

`Assets/_Project/Scenes/PlayerMovementTest.unity` — Main Camera `FollowCameraController`, `_initialWorldForward = (0,0,1)`.
