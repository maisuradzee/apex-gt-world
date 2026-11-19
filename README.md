# APEX GT WORLD

Android-first, mobile-optimized open-world motorsport and roleplay game foundation. The first playable milestone is **offline single-player**.

## Current project state
The project is moving to an offline-first playable milestone. The generated car scene now has a main menu, Free Drive, a two-lap local circuit event with four checkpoints and three AI cars, a Garage with saved upgrades, and a device-local save system. The existing cloud backend and race-server work are retained for a later online phase. This is a prototype vertical slice, not the finished open-world game. GitHub Actions validates source/security rules, type-checks Edge Functions, builds the .NET race-server library and Windows Service host, publishes a self-contained Windows x64 artifact, and runs the HTTP contract plus race-session domain tests. This remains a **starter foundation**, not a completed playable game.

## Product and technical baseline
- Android first; keep architecture iOS-compatible.
- Unity 6 LTS + Universal Render Pipeline (URP).
- Connected regions: Metro City, Coastal City, Mountain District, Motorsport Park.
- First-person and third-person cameras.
- Persistent shared world with separate authoritative race sessions.
- Separate car and motorcycle control/physics prototypes.
- Mobile optimization targets: 30 FPS baseline and 60 FPS on capable devices; not yet performance-verified.
- Use original or properly licensed code, maps, art, vehicles, music, and other content.

## Repository structure
- `Assets/Scripts/VehiclePhysics` — car controller/input prototype.
- `Assets/Scripts/MotorcyclePhysics` — motorcycle balance/lean prototype.
- `Assets/Scripts/Camera` — camera mode and follow logic.
- `Assets/Scripts/World` — region metadata and scene-loading foundation.
- `Assets/Scripts/Offline` — local player profile, versioned save store, main-menu flow, career race/checkpoints, AI opponents, Garage upgrades and local economy foundation.
- `Assets/Scripts/Backend` — optional later online Supabase Auth, profile, wallet, career and race reward client.
- `Assets/Editor` — starter scene generators and Android build menu.
- `Database/Migrations` — Supabase schema, security, Auth bootstrap, wallet and race lifecycle.
- `supabase/functions` — JWT-protected reward claim and secret-protected race control functions.
- `Server/RaceServer` — .NET 8 race-session runtime, lifecycle coordinator and protected Supabase control client.
- `Server/RaceServer.Tests` — HTTP contract, race-session domain and coordinator tests.
- `Server/RaceServer.Host` — .NET 8 Windows Service process host.
- `Server/RaceServer/Windows` — Windows publish, environment and service installation scripts.
- `Documentation` and `Tests` — architecture, setup, acceptance tests and validators.

## Unity setup
1. Install Unity Hub and a Unity 6 LTS Editor with the 3D URP template.
2. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open/create the project using this source tree; retain Unity-generated URP settings and merge required package dependencies.
4. Let Unity import and compile, then use `APEX GT WORLD > Setup` to generate the car or motorcycle test scene.
5. The scene generator adds the new scene without deleting other enabled Build Settings scenes.
6. Run the test plan before using `APEX GT WORLD > Build > Build Android APK`.

## Offline-first gameplay
- No login or internet connection is required to start the local game prototype.
- Local profile, credits, career XP/level, selected/owned vehicles, unlocked regions, completed events and distance are stored below Unity `Application.persistentDataPath`.
- The save file is versioned and keeps a backup for recovery. Saves occur periodically and when the app pauses or quits.
- Local rewards are single-player progression only. They are not trusted for online leaderboards, competitive placements or cloud wallet payouts.
- The first generated car scene is a compact placeholder circuit/test area with primitive geometry, one career event and three simple waypoint AI drivers. It does not yet contain the finished Metro City environment or production vehicle art/physics.

## First offline play loop
1. Open the project in Unity 6 LTS with URP.
2. Generate the Starter Car Scene from `APEX GT WORLD > Setup`.
3. Press Play. The offline menu opens without requiring login or internet.
4. Start Free Drive or open Career and start the Metro Circuit Club Cup (two laps, four ordered gates, three AI opponents).
5. Finish the event to earn a one-time local reward of 500 Credits and 250 XP.
6. Open Garage and purchase Engine, Handling or Brakes upgrades (250 Credits each). Upgrades change the starter car's tuning and persist locally.
7. Stop Play Mode and launch again; the local profile and upgrades should remain.

Editor controls: WASD / arrow keys to steer and accelerate, Space to brake, Left Shift for handbrake, V to switch camera, Escape to pause. Android uses the generated touch controls.

## Optional online backend (later phase)
Target: `GTWORLD` (`hzawgrvonsqyqjyzzreg`, `eu-west-1`), verified `ACTIVE_HEALTHY`. Ten migrations are applied:
- `0001_apex_core.sql` — public data model and RLS.
- `0002_apex_security_and_region_indexes.sql` — private-secret isolation and indexes.
- `0003_apex_auth_user_bootstrap.sql` — Auth trigger provisions profile, wallet and progression.
- `0004_apex_service_role_privileges.sql` — trusted server database grants.
- `0005_apex_wallet_transactions.sql` — atomic idempotent wallet adjustment.
- `0006_apex_race_reward_settlement.sql` — server-owned race payout field.
- `0007_apex_authoritative_race_lifecycle.sql` — server-only race start/finalize RPCs.
- `0008_apex_race_result_integer_validation.sql` — unique placements and integer-only race result validation.
- `0009_apex_idempotent_race_session_creation.sql` — private create-request keys and idempotent race session creation.
- `0010_apex_private_request_policy.sql` — explicit deny RLS policy for the private request-key table.

The `settle-race-result` function is ACTIVE with JWT verification. The `race-session-control` function is ACTIVE with its own shared-token verification. The database lifecycle and reward path were transaction-tested; rollback removed all test rows. Security Advisor reports no findings.

**Race server token setup is still required before the control function will execute commands.** In GTWORLD Supabase Edge Function secrets, set `APEX_RACE_SERVER_TOKEN` to a strong random secret (at least 32 characters). Set the exact same value only in the dedicated race-server host environment. Never commit it or include it in Unity. The Edge Function uses the Supabase-managed service-role key internally; the .NET client does not receive that key.

A Windows Service host scaffold and a self-contained Windows x64 artifact are available for the later online phase. The service currently validates its control configuration and stays running; it is not required by offline gameplay and is not yet a playable race server. Authoritative vehicle physics, live player networking, Unity runtime tests, APK build and device performance testing remain incomplete. Do not apply these migrations to Qulup or ASIX.

## GitHub
Repository: https://github.com/maisuradzee/apex-gt-world

CI checks source structure, secret exposure, migration security and server-only operations, type-checks both Deno functions, builds the .NET 8 race-server control client, and runs HTTP contract tests for create/start/finalize, request keys, token headers and input guards, plus race-runtime tests for player limits, checkpoint progression, server timing, DNF/DQ and deterministic results.
