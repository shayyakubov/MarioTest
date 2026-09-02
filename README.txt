NumTalk 3D Platformer


Unity version
Unity 6 — 6000.4.6f1


How to run
Open Assets/Scenes/Course.unity and press Play.
Keyboard: WASD/arrows + Space. Touch: left half move, right half jump.


In
Run/jump, follow camera, moving/ice/crumble platforms, pushable crate, stomp + shooter enemies, 3 lives, checkpoints, goal flag, basic HUD.


Out
Menus beyond game-over/win, custom art/audio, settings, saves, extra levels, moving ice platform (untested yet)

Movement tuning (Assets/ScriptableObjects/PlayerTuning.asset)
max speed 8
max acceleration 50
jump velocity 12.5
coyote time 0.3 s
jump buffer 0.12 s
rise gravity -25 / low-jump gravity -80 / fall gravity -40
max fall speed 20
stomp bounce 14.1
knockback decay 5.5 / max knockback 25

Published jump limits: height 3.1 m, distance 5.0 m, safe gap 3.5 m.
(Gaps authored with higher jump defaults in mind — verify at 30 fps with shipped 12.5 jump.)


Layer layout
6 Ground — platforms
7 Player
8 Enemy
9 Projectile
10 Pushable — crates

Projectile vs Enemy: off. Projectile vs Projectile: off.
Ground check also stands on Pushable and Default.


Known issues
Life-loss respawn restores crumble platforms and pushables only — not stomped enemies or collected coins.
Phone build / device recording not included in repo yet.
StompEnemy is a bit hard to stomp, also you get hit by a side knockback too easily