# APEX GT WORLD

Android-first, mobile-optimized open-world motorsport and roleplay game foundation.

## Current project state
The Unity starter source tree is committed to `main`. GitHub Actions validates source/security rules and now type-checks Edge Functions plus builds the .NET race-server control client. This remains a **starter foundation**, not a completed playable game.

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
- `Assets/Scripts/Backend` — Supabase Auth, profile, wallet, career and race reward client.
- `Assets/Editor` — starter scene generators and Android build menu.
- `Database/Migrations` — Supabase schema, security, Auth bootstrap, wallet and race lifecycle.
- `supabase/functions` — JWT-protected reward claim and secret-protected race control functions.
- `Server/RaceServer` — .NET 8 race-server control HTTP client.
- `Documentation` and `Tests` — architecture, setup, acceptance tests and validators.

## Unity setup
1. Install Unity Hub and a Unity 6 LTS Editor with the 3D URP template.
2. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open/create the project using this source tree; retain Unity-generated URP settings and merge required package dependencies.
4. Let Unity import and compile, then use `APEX GT WORLD > Setup` to generate the car or motorcycle test scene.
5. The scene generator adds the new scene without deleting other enabled Build Settings scenes.
6. Run the test plan before using `APEX GT WORLD > Build > Build Android APK`.

## Supabase
Target: `GTWORLD` (`hzawgrvonsqyqjyzzreg`, `eu-west-1`), verified `ACTIVE_HEALTHY`. Nine migrations are applied:
- `0001_apex_core.sql` — public data model and RLS.
- `0002_apex_security_and_region_indexes.sql` — private-secret isolation and indexes.
- `0003_apex_auth_user_bootstrap.sql` — Auth trigger provisions profile, wallet and progression.
- `0004_apex_service_role_privileges.sql` — trusted server database grants.
- `0005_apex_wallet_transactions.sql` — atomic idempotent wallet adjustment.
- `0006_apex_race_reward_settlement.sql` — server-owned race payout field.
- `0007_apex_authoritative_race_lifecycle.sql` — server-only race start/finalize RPCs.
- `0008_apex_race_result_integer_validation.sql` — unique placements and integer-only race result validation.
- `0009_apex_idempotent_race_session_creation.sql` — private create-request keys and idempotent race session creation.

The `settle-race-result` function is ACTIVE with JWT verification. The `race-session-control` function is ACTIVE with its own shared-token verification. The database lifecycle and reward path were transaction-tested; rollback removed all test rows. Security Advisor reports no findings.

**Race server token setup is still required before the control function will execute commands.** In GTWORLD Supabase Edge Function secrets, set `APEX_RACE_SERVER_TOKEN` to a strong random secret (at least 32 characters). Set the exact same value only in the dedicated race-server host environment. Never commit it or include it in Unity. The Edge Function uses the Supabase-managed service-role key internally; the .NET client does not receive that key.

The dedicated race simulation process, Unity runtime tests, APK build and device performance testing are not yet complete. Do not apply these migrations to Qulup or ASIX.

## GitHub
Repository: https://github.com/maisuradzee/apex-gt-world

CI checks source structure, secret exposure, migration security and server-only operations, type-checks both Deno functions, and builds `Server/RaceServer/ApexGtWorld.RaceServer.csproj`.
