# DECISIONS

## Three hardest calls

### 1. Rigidbody over CharacterController

I chose a dynamic Rigidbody because the requirements include physical interactions like crate pushing and knockback. This lets me use Unity's built-in physics instead of recreating those interactions manually.

With a CharacterController, collisions mainly resolve the character's own movement. Pushing Rigidbody objects would need custom code to detect hits, apply push forces, and handle how both objects respond. Knockback on ice or a moving platform gets worse — you're calling Move() without cleanly owning velocity. I also needed custom gravity, coyote time, and variable jump height; a Rigidbody lets me set that directly each frame.

**Pros**
- Unity physics handles crate collisions and knockback
- Less custom collision-response code for push interactions
- Run speed, knockback, and moving platforms compose naturally

**Cons**
- More custom controller logic — ground checks, gravity, movement tuning
- Physics edge cases (wall stick, false ground) need careful handling so physics doesn't fight the intended feel

---

### 2. Zero friction + tighter ground check

The player capsule uses zero friction. Ground detection is a spherecast, but the hit must land under the feet — not a side graze on a platform edge. Braking and ice are done by scaling acceleration in the movement code, not PhysicMaterial friction.

During playtesting the player would hang mid-air against platform walls. The ground check was hitting the top of a platform from the side and treating it as standing, and wall friction was fighting gravity. This approach fixed both without rewriting movement from scratch.

**Pros**
- Fixed wall-hang and false-ground bugs
- You can tune how ice feels by playing with the acceleration change parameter
- Run/brake feel stays consistent — not fighting Unity friction

**Cons**
- Sticky or muddy surfaces need explicit code, not a material value
- Ice platforms need custom code too — a surface component and movement logic, not just a slippery material
- Extra ground-check logic to maintain

---

### 3. One script handles all movement — including knockback

All player movement goes through `PlayerMovement` — run speed, ice slowdown, moving platforms, knockback, all of it. Nothing else should directly push the Rigidbody around, because that's where max speed, acceleration, and surface modifiers get applied.

If knockback just used a physics force (AddForce on hit), that push lives outside my movement code. Next frame my code tries to set speed to max 8 — but the force already added extra speed on top. They work against each other. I could cap total speed at 8 to fix that, but then getting shot barely pushes you. Leave the cap off and one hit sends you flying.

So when a projectile hits, I store the push in a separate knockback value. Each frame the movement script updates run speed from input, fades knockback on its own, adds them together, and writes the result. One place computes movement, everything composes there.

**Pros**
- You can still move while being pushed — you're not locked out
- Hits can feel strong even though max run speed is only 8
- Ice, air, and moving platforms all go through the same path — knockback doesn't need its own special cases
- Easy to tune with `knockbackDecay` and `maxKnockbackSpeed`

**Cons**
- Two speed values to keep in sync instead of one
- Everything that moves the player has to go through this script — bypass it and things break
- Getting push strength and fade-out time to feel right took playtesting

**What I could've done instead**
- **Physics impulse** — add force on hit and let Unity handle it. Less code, but it skips max speed and surface modifiers entirely.
- **One speed, hard cap** — add knockback into the same velocity and clamp to max run speed. Simple, but hits feel weak.
- **Disable input during knockback** — player can't steer until it ends. Easy to implement, but feels bad on mobile.
- **Override velocity on hit** — replace movement entirely for half a second. Strong hit feel, but you lose the "keep control" requirement.

---

## Where I'd take the controller next

Turn responsiveness and stopping — tune `maxAllowedAcceleration` and ground/air deceleration multipliers so direction changes and releasing the stick feel crisp. Maybe higher acceleration when reversing direction (decelerating into a turn).

Jump tuning — short hops feel too low today because early release slams `lowJumpGravity`. Jump cut on button release and retune so taps give a natural hop and holds give a clearly higher full jump.

Surface lookup — platforms already use two interfaces: `IMovingSurface` for carry velocity (moving platforms) and `IMovementModifierSurface` for ice (accel multiplier). Movement queries them in two different places today. I'd pull that into one lookup that returns a small data struct — carry velocity, accel multiplier, etc. — so movement just reads the struct instead of knowing about each interface.


## AI note

**Generated vs mine:** Roughly 40% AI-generated (scene builders, bootstrap wiring, HUD/checkpoint/pickup managers, layer setup). Roughly 60% written or substantially rewritten by me (PlayerMovement, ground detection, stomp/side-hit, knockback composition, most tuning). I code-review AI output before Play on every feature — not much survives unchanged.

**Got wrong:** Jump input was placed after the move deadzone early-return in `PlayerController.Update`, which would skip coyote/buffer when standing still. Caught reading the diff before playtesting — moved jump reading ahead of the deadzone return.

**Rejected:** Tag checks on projectile hits in `OnTriggerEnter`. Brief requires a layer collision matrix; I used `PhysicsLayersSetup` and layer ignores instead.

Workflow: for bigger features I wrote a short feature doc, had the assistant plan against it, reviewed the plan and generated files before Play, then playtested and sent back small fixes. Scene builders and test scenes were the biggest time-saver.
