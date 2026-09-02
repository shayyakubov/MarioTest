# DECISIONS

## Three hardest calls

### 1. Rigidbody over CharacterController

I chose a dynamic Rigidbody because the requirements include physical interactions like crate pushing and knockback. This lets me use Unity's built-in physics instead of recreating those interactions manually.

With a CharacterController, collisions mainly resolve the character's own movement. Pushing Rigidbody objects would need custom code to detect hits, apply push forces, and decide how both objects respond. A Rigidbody gives me a better base for those interactions while still letting me keep the player movement controlled in code.

**Pros**
- Unity physics handles crate collisions and external pushes naturally
- Less custom collision-response code for push interactions
- Knockback and moving-platform motion can compose with the player's existing movement

**Cons**
- More custom controller logic, especially ground checks, gravity, and movement tuning
- Physics edge cases such as wall stick and false grounding need careful handling so physics does not fight the intended feel

---

### 2. Zero friction + tighter ground check

The player capsule uses zero friction. Ground detection is a spherecast, but the hit also has to be under the player's feet rather than just a side graze on a platform edge. Braking and ice are handled by changing acceleration in the movement code instead of relying on PhysicMaterial friction.

During playtesting, the player could hang in the air against platform walls. The ground check was sometimes catching the top of a platform from the side and treating it as valid ground, while wall friction was also fighting gravity. Zero friction plus the tighter ground check fixed both without changing the whole movement model.

**Pros**
- Fixed wall-hang and false-ground cases
- Ice can be tuned directly through acceleration
- Run and braking feel stay predictable instead of depending on Unity friction

**Cons**
- Sticky or muddy surfaces would need explicit movement modifiers
- Ice needs a surface component and movement logic instead of only a PhysicMaterial
- Ground detection has a little more custom logic to maintain

---

### 3. One movement path, including knockback

All player movement goes through `PlayerMovement`: normal movement, ice modifiers, moving-platform velocity, and knockback. Other systems can request an effect, but they should not directly take over the Rigidbody movement.

Knockback is stored separately from normal controlled movement. That means a hit can push the player above normal run speed without the movement code immediately clamping it away. The knockback then decays over time while the player still keeps some steering control.

This also keeps the different movement effects composable: normal input still works while airborne, on ice, or on a moving platform, and knockback does not need a separate movement mode for each case.

**Pros**
- The player can still steer while being pushed
- Hits can feel strong even with a lower normal run speed
- Ice, air movement, moving platforms, and knockback share the same movement path
- Easy to tune with values such as `knockbackDecay` and `maxKnockbackSpeed`

**Cons**
- Normal movement and knockback are separate values that have to be combined correctly
- Anything that moves the player outside this path can break the assumptions
- Push strength and decay still need playtesting to feel right

## Where I'd take the controller next

**Turn responsiveness and stopping** — tune `maxAllowedAcceleration` and the ground/air acceleration values so reversing direction and releasing the stick feel sharper. I would also try a higher acceleration when reversing direction than when accelerating normally.

**Jump tuning** — short hops currently feel too low because early release applies the stronger low-jump gravity too aggressively. I would add or retune the jump cut so taps give a natural short hop while holding still gives a clearly higher full jump.

**Surface lookup** — platforms currently expose `IMovingSurface` for carry velocity and `IMovementModifierSurface` for things like ice. `PlayerMovement` queries them separately. If more surface behaviours were added, I would probably collect those values into one small surface-data result so the movement code reads the final values in one place.

---

## AI note

**Generated vs mine:** Roughly 60% AI-generated, mainly scene builders, bootstrap wiring, HUD/checkpoint/pickup managers, layer setup, and mechanical refactors. Roughly 40% was written or substantially rewritten by me, especially `PlayerMovement`, ground detection, stomp/side-hit logic, knockback composition, and most tuning.

I also used AI for small mechanical edits such as extracting or moving code, exposing fields in the Inspector, and similar refactors. These were mostly faster to describe than type manually, but I reviewed the diff before accepting them.

**Got wrong:** Jump input was originally read after the movement deadzone early-return in `PlayerController.Update`, which meant coyote time and jump buffering could be skipped while standing still. I caught it while reviewing the diff before playtesting and moved jump input ahead of that return.


**Rejected:** The assistant suggested tag checks for projectile hits inside `OnTriggerEnter`. The requirement explicitly asks for the layer collision matrix, so I used `PhysicsLayersSetup` and layer-based collision filtering instead.

a lot of times the AI broke SingleResponsibility and didnt split code into files and methods, sometimes he initialized Scripts with serialized fields that u need to drag
even though we have GameBootstrap class that helps keep references in one place and manually injects them to relevant features (also helps in turning them into regular csharp classes that are not dependant on unity life cycle)


**Workflow:** For bigger features, I wrote a short feature description, had the assistant plan against it, reviewed the plan and generated files, then playtested and sent back small fixes. Scene builders and test-scene setup were the biggest time-savers.

for smaller features i usually describe a small task (should be one responsibility) and code review plus test..
