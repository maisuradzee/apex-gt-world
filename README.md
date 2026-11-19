# APEX GT WORLD

Android-first, mobile-optimized open-world motorsport and roleplay game foundation.

## Current project state
The Unity starter source tree is committed to `main`. GitHub Actions successfully expanded the archive into the repository and the static source validator passes. This is a **starter source foundation**, not a completed playable game.

## Product and technical baseline
- Android first; keep architecture iOS-compatible.
- Unity 6 LTS + Universal Render Pipeline (URP).
- Connected regions: Metro City, Coastal City, Mountain District, Motorsport Park.
- First-person and third-person cameras.
- Persistent shared world with separate authoritative race sessions.
- Separate car and motorcycle control/physics prototypes.
- Mobile optimization targets: 30 FPS baseline and 60 FPS on capable devices; these targets are not yet performance-verified.
- Use original or properly licensed code, maps, art, vehicles, music, and other content.

## Repository structure
- `Assets/Scripts/VehiclePhysics` — car controller/input prototype.
- `Assets/Scripts/MotorcyclePhysics` — motorcycle balance/lean prototype.
- `Assets/Scripts/Camera` — camera mode and follow logic.
- `Assets/Scripts/World` — region metadata and scene-loading foundation.
- `Assets/Scripts/Backend` — Supabase client/configuration and diagnostics foundation.
- `Assets/Editor` — starter scene generators and Android build menu.
- `Database/Migrations` — versioned Supabase schema and security migrations.
- `Documentation` and `Tests` — architecture, setup, test plan and source validator.

## Unity setup
1. Install Unity Hub and a Unity 6 LTS Editor with the 3D URP template.
2. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open/create the project using this source tree; retain Unity-generated URP settings and merge required package dependencies.
4. Let Unity import and compile, then use `APEX GT WORLD > Setup` to generate the car or motorcycle test scene.
5. Run the test plan before using `APEX GT WORLD > Build > Build Android APK`.

## Supabase
Target project: `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`), verified `ACTIVE_HEALTHY`. Both migrations have been applied:
- `0001_apex_core.sql` — 11 public tables, RLS, restricted profile grants and private race-session secrets.
- `0002_apex_security_and_region_indexes.sql` — explicit client deny policy for private secrets and region foreign-key indexes.

The database currently contains 11 public tables plus one private table; four region seed rows are present. Security Advisor reports no findings. Live Unity Auth/PostgREST smoke tests are still pending. Do not apply these migrations to Qulup or ASIX.

The Unity client may use the publishable key, but must never contain a service-role/secret key, database password, signing key, or server secret.

## GitHub and validation
Repository: https://github.com/maisuradzee/apex-gt-world

The source tree is committed on `main`. GitHub Actions successfully expanded the compressed source package and the source validation workflow passes. Static checks do not replace Unity Editor compilation, Play Mode physics/input tests, Android device profiling, server-side multiplayer testing, or a production security review.
