# APEX GT WORLD

Android-first, mobile-optimized open-world motorsport and roleplay game foundation.

## Current project state
The initial Unity source tree is committed to `main`. The bootstrap GitHub Actions run successfully assembled the source package, extracted it, and ran `Tests/validate_source.py`. This is a **starter source foundation**, not a completed playable game.

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
- `Database/Migrations` — initial Supabase schema migration.
- `Documentation` and `Tests` — architecture, setup, test plan and source validator.

## Unity setup
1. Install Unity Hub and a Unity 6 LTS Editor with the 3D URP template.
2. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK.
3. Open/create the project using this source tree; retain Unity-generated URP settings and merge required package dependencies.
4. Let Unity import and compile, then use `APEX GT WORLD > Setup` to generate the car or motorcycle test scene.
5. Run the test plan before using `APEX GT WORLD > Build > Build Android APK`.

## Supabase
Target project: `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`). It was verified active and healthy, with no public tables or migrations at the time of inspection. `Database/Migrations/0001_apex_core.sql` is staged in source but has **not** been applied.

Apply it only to the dedicated APEX project, never to Qulup or ASIX. The Unity client may use the publishable key, but must never contain a service-role/secret key, database password, signing key, or server secret. Authentication and RLS ownership checks are still required.

## GitHub and validation
Repository: https://github.com/maisuradzee/apex-gt-world

The source tree is committed on `main`. The first extraction workflow successfully assembled the compressed package and passed the static source validator. `validate-source.yml` runs source checks on future pushes and pull requests.

Static checks do not replace Unity Editor compilation, Play Mode physics/input tests, Android device profiling, server-side multiplayer testing, or a production security review. None of those full runtime validations is claimed yet.
