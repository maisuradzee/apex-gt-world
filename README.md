# APEX GT WORLD

Android-first, mobile-optimized open-world motorsport and roleplay game foundation.

## Current project state
The Unity starter source tree is committed to `main`. GitHub Actions source expansion and static source/security validation pass. This is a **starter source foundation**, not a completed playable game.

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
- `Assets/Scripts/Backend` — Supabase Auth/PostgREST adapter and diagnostics.
- `Assets/Editor` — starter scene generators and Android build menu.
- `Database/Migrations` — versioned Supabase schema, security, Auth bootstrap, server grants and wallet transaction RPC.
- `Documentation` and `Tests` — architecture, setup, test plan and source validator.

## Unity setup
1. Install Unity Hub and a Unity 6 LTS Editor with the 3D URP template.
2. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open/create the project using this source tree; retain Unity-generated URP settings and merge required package dependencies.
4. Let Unity import and compile, then use `APEX GT WORLD > Setup` to generate the car or motorcycle test scene.
5. The scene generator adds the new scene without deleting other enabled Build Settings scenes.
6. Run the test plan before using `APEX GT WORLD > Build > Build Android APK`.

## Supabase
Target: `GTWORLD` (`hzawgrvonsqyqjyzzreg`, `eu-west-1`), verified `ACTIVE_HEALTHY`. Five migrations are applied:
- `0001_apex_core.sql` — public data model, RLS, restricted client grants and private race-session secrets.
- `0002_apex_security_and_region_indexes.sql` — private-secret isolation and region indexes.
- `0003_apex_auth_user_bootstrap.sql` — Auth trigger provisions profile, zeroed wallet and progression; direct client profile insertion is revoked.
- `0004_apex_service_role_privileges.sql` — trusted server-side access to current and future APEX tables.
- `0005_apex_wallet_transactions.sql` — atomic, idempotent Credits/Premium Tokens adjustment function, executable only by `service_role`.

Verified: 11 public tables + 1 private table with RLS on all 12; Auth trigger rollback test passed; service_role has backend table access; authenticated users can only update their own display_name and cannot write wallet state or call the wallet RPC; Security Advisor reports no findings. Wallet transaction tests covered a credit, duplicate idempotency request, debit, insufficient-funds rejection and payload mismatch rejection. The transaction rollback left no test users or wallet rows. Live Unity Auth/PostgREST smoke tests remain pending. Do not apply these migrations to Qulup or ASIX.

The Unity client may use the publishable key, but must never contain a service-role/secret key, database password, signing key, or server secret. The service-role key belongs only in trusted server-side secrets.

## GitHub and validation
Repository: https://github.com/maisuradzee/apex-gt-world

GitHub Actions validates source structure, migration security expectations, Auth bootstrap, server-only wallet settlement and client request shape, scene preservation, and credential scans. Static checks do not replace Unity Editor compilation, Play Mode physics/input tests, Android device profiling, server-side multiplayer testing, or a production security review.
